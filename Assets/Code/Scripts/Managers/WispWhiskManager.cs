using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WispWhiskManager : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private MinigameHandler minigameHandler;
    [SerializeField] private GameObject[] territories;
    [SerializeField] private GameObject wispPrefab;
    [SerializeField] private Canvas scoreDisplayCanvas = null;
    [SerializeField] private TextMeshProUGUI scoreDisplayText = null;
    public InGameScoreboardController inGameScoreboardController;

    private List<GameObject> wisps = new();

    [Header("Settings")]
    [Tooltip("How many seconds between wisps assign points to their holding player.")]
    [SerializeField] private float timeBetweenGivenPoints = 0.25f;
    [Tooltip("Minimum or bottom left 3D vector of where wisps/territories can spawn in a cubed area.")]
    [SerializeField] private Vector3 minSpawnLocation = Vector3.zero;
    [Tooltip("Maximum or top right 3D vector of where wisps/territories can spawn in a cubed area.")]
    [SerializeField] private Vector3 maxSpawnLocation = Vector3.zero;
    [Tooltip("The minimum distance between two wisps when spawning.")]
    [SerializeField] private float distanceBetweenWisps = 1f;
    [Tooltip("The height or below that a wisp will be forced to respawn if found at.")]
    [SerializeField] private float wispRespawnHeight = 1f;
    [Tooltip("How many points to reduce from the player when they fall. Player's points will not surpass 0.")]
    [SerializeField] private int respawnPointDeduction = 0;

    [HideInInspector] public Dictionary<GameObject, int> playerPoints = new();

    private void Awake() {
        Vector3 bottomSquare = new Vector3(minSpawnLocation.x > maxSpawnLocation.x ? maxSpawnLocation.x : minSpawnLocation.x, minSpawnLocation.y > maxSpawnLocation.y ? maxSpawnLocation.y : minSpawnLocation.y, minSpawnLocation.z > maxSpawnLocation.z ? maxSpawnLocation.z : minSpawnLocation.z);
        Vector3 topSquare = new Vector3(minSpawnLocation.x < maxSpawnLocation.x ? maxSpawnLocation.x : minSpawnLocation.x, minSpawnLocation.y < maxSpawnLocation.y ? maxSpawnLocation.y : minSpawnLocation.y, minSpawnLocation.z < maxSpawnLocation.z ? maxSpawnLocation.z : minSpawnLocation.z);
        minSpawnLocation = bottomSquare;
        maxSpawnLocation = topSquare;
    }

    private void Update() {
        if (!isServer) return;

        List<int> removeWispIndex = new();
        for (int i = 0; i < wisps.Count; i++) {
            if (wisps[i] == null) {
                removeWispIndex.Add(i);
                continue;
            }

            if (wisps[i].transform.position.y <= wispRespawnHeight) {
                removeWispIndex.Add(i);
                if (wisps[i].transform.parent != null && wisps[i].transform.parent.parent != null && wisps[i].transform.parent.parent.TryGetComponent(out WispEffect wispEffect)) {
                    wispEffect.TargetToggleGlowDisplay(false);
                    wispEffect.RpcPlayDropAudio();
                }
                NetworkServer.Destroy(wisps[i]);
                StartCoroutine(SpawnWisp());
            }
        }

        foreach (var index in removeWispIndex) {
            wisps.Remove(wisps[index]);
        }
    }

    // Called by server when the minigame starts.
    public void SpawnWisps() {
        foreach (var player in CustomNetworkManager.Instance.ClientDatas.Keys) {
            playerPoints.Add(player.identity.gameObject, 0);
            player.identity.GetComponent<PlayerMovementComponent>().TargetSetMinigameManagerObject(gameObject);
        }

        //Spawn wisps and territories here
        int wispCount = 1;
        if (CustomNetworkManager.Instance.ClientDatas.Count > 4) wispCount = 2;
        if (CustomNetworkManager.Instance.ClientDatas.Count > 6) wispCount = 3;

        for (int i = 0; i < wispCount; i++) {
            StartCoroutine(SpawnWisp());
            StartCoroutine(SpawnTerritory());
        }
        RpcEnableScoreDisplay();

        StartCoroutine(AddPointsEveryInterval());
    }

    public IEnumerator SpawnWisp() {
        while (true) {
            Vector3 overheadLocation = new Vector3(Random.Range(minSpawnLocation.x, maxSpawnLocation.x), maxSpawnLocation.y, Random.Range(minSpawnLocation.z, maxSpawnLocation.z));

            int excludePlayerLayerMask = LayerMask.NameToLayer("Player") | LayerMask.NameToLayer("PlayerHitbox") | LayerMask.NameToLayer("PlayerHitbox") | LayerMask.NameToLayer("Ignore Raycast");
            if (Physics.Raycast(overheadLocation, Vector3.down, out RaycastHit hit, Mathf.Abs(minSpawnLocation.y) + Mathf.Abs(maxSpawnLocation.y), excludePlayerLayerMask)) {
                // Check if there is already a wisp nearby. If there is, it will continue and get a new position.
                bool isNearWisp = false;
                Collider[] intersectingColliders = Physics.OverlapSphere(hit.point, distanceBetweenWisps);
                if (intersectingColliders.Length > 0) {
                    foreach (var intersectingCollider in intersectingColliders) {
                        if (intersectingCollider.GetComponent<CollectiblePoint>() != null) {
                            isNearWisp = true;
                            break;
                        }
                    }
                }
                if (isNearWisp) {
                    yield return null;
                    continue;
                }

                GameObject wisp = Instantiate(wispPrefab, hit.point, Quaternion.identity);
                wisps.Add(wisp);
                NetworkServer.Spawn(wisp);
                break;
            }
            yield return null;
        }
    }

    public IEnumerator SpawnTerritory() {
        GameObject enabledTerritory;
        while (true) {
            enabledTerritory = territories[Random.Range(0, territories.Length)];

            if (!enabledTerritory.GetComponent<Territory>().isActive) {
                break;
            }

            yield return null;
        }

        enabledTerritory.GetComponent<Territory>().wispWhiskManager = this;
        enabledTerritory.GetComponent<Territory>().isActive = true;
    }

    /// <summary>Enable score display on all clients.</summary>
    [ClientRpc]
    public void RpcEnableScoreDisplay() {
        scoreDisplayCanvas.enabled = true;
        inGameScoreboardController.enabled = true;
    }

    public IEnumerator AddPointsEveryInterval() {
        foreach (var player in CustomNetworkManager.Instance.ClientDatas.Keys) {
            if (player.identity.GetComponent<WispEffect>() && player.identity.GetComponent<WispEffect>().holdingWisp) {
                int pointsToAdd = player.identity.GetComponent<WispEffect>().holdingWisp.GetComponent<CollectableWispEffect>().pointsToAdd;
                playerPoints[player.identity.gameObject] += pointsToAdd;
                TargetSetScoreDisplay(player.identity.GetComponent<NetworkIdentity>().connectionToClient, playerPoints[player.identity.gameObject]);
                inGameScoreboardController.RpcUpdateScoreCard(player.identity.GetComponent<PlayerController>().playerName, playerPoints[player.identity.gameObject]);
            }
        }

        yield return new WaitForSeconds(timeBetweenGivenPoints);
        StartCoroutine(AddPointsEveryInterval());
    }

    [TargetRpc]
    public void TargetSetScoreDisplay(NetworkConnectionToClient target, int score) {
        scoreDisplayText.text = score.ToString();
    }

    /// <summary>Determine order of most points to assign standings in the MinigameHandler.</summary>
    /// Sorts player's points and group ties together. Is there a better way of doing this?
    public void DetermineWinners() {
        // Get a list of the scores without duplicates and order by descending.
        List<int> scores = new();
        foreach (var playerPoint in playerPoints) {
            if (!scores.Contains(playerPoint.Value)) {
                scores.Add(playerPoint.Value);
            }
        }
        scores = scores.OrderByDescending(s => s).ToList();

        // Go down the list of scores and find all players with that score to group together. Once grouped, add to winners.
        foreach (int score in scores) {
            List<NetworkConnectionToClient> currentStanding = new();
            foreach (var playerPoint in playerPoints) {
                if (playerPoint.Value == score && playerPoint.Key != null) {
                    currentStanding.Add(playerPoint.Key.GetComponent<NetworkIdentity>().connectionToClient);
                }
            }
            minigameHandler.AddWinner(currentStanding);
        }
    }

    public void RespawnPointDeduction(GameObject player) {
        if (respawnPointDeduction == 0) return;

        playerPoints[player] -= respawnPointDeduction;
        if (playerPoints[player] < 0) {
            playerPoints[player] = 0;
        }

        TargetSetScoreDisplay(player.GetComponent<NetworkIdentity>().connectionToClient, playerPoints[player]);
        inGameScoreboardController.RpcUpdateScoreCard(player.GetComponent<PlayerController>().playerName, playerPoints[player]);
    }

    void OnDrawGizmosSelected() {
        // Display the cubed area of where boxes can spawn
        Gizmos.color = Color.yellow;
        Vector3 bottomSquare = new Vector3(minSpawnLocation.x > maxSpawnLocation.x ? maxSpawnLocation.x : minSpawnLocation.x, minSpawnLocation.y > maxSpawnLocation.y ? maxSpawnLocation.y : minSpawnLocation.y, minSpawnLocation.z > maxSpawnLocation.z ? maxSpawnLocation.z : minSpawnLocation.z);
        Vector3 topSquare = new Vector3(minSpawnLocation.x < maxSpawnLocation.x ? maxSpawnLocation.x : minSpawnLocation.x, minSpawnLocation.y < maxSpawnLocation.y ? maxSpawnLocation.y : minSpawnLocation.y, minSpawnLocation.z < maxSpawnLocation.z ? maxSpawnLocation.z : minSpawnLocation.z);
        Gizmos.DrawWireCube(new Vector3((topSquare.x + bottomSquare.x)/2, (topSquare.y + bottomSquare.y)/2, (topSquare.z + bottomSquare.z)/2), new Vector3(Mathf.Abs(topSquare.x - bottomSquare.x), Mathf.Abs(topSquare.y - bottomSquare.y), Mathf.Abs(topSquare.z - bottomSquare.z)));
    }
}
