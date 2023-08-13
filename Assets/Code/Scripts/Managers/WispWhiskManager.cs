using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WispWhiskManager : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private MinigameHandler minigameHandler;
    [SerializeField] private GameObject territoryPrefab;
    [SerializeField] private GameObject wispPrefab;
    [SerializeField] private Canvas scoreDisplayCanvas = null;
    [SerializeField] private TextMeshProUGUI scoreDisplayText = null;

    [Header("Settings")]
    [Tooltip("How many seconds between wisps assign points to their holding player.")]
    [SerializeField] private float timeBetweenGivenPoints = 0.25f;
    [Tooltip("Minimum or bottom left 2D vector(X and Z-Axis) of where wisps/territories can spawn in an area.")]
    [SerializeField] private Vector2 minSpawnLocation = Vector2.zero;
    [Tooltip("Maximum or top right 2D vector(X and Z-Axis) of where wisps/territories can spawn in an area.")]
    [SerializeField] private Vector2 maxSpawnLocation = Vector2.zero;
    [Tooltip("The minimum distance between two wisps when spawning.")]
    [SerializeField] private float distanceBetweenWisps = 1f;

    [HideInInspector] public Dictionary<GameObject, int> playerPoints = new();

    // Called by server when the minigame starts.
    public void SpawnWisps() {
        foreach (var player in CustomNetworkManager.Instance.ClientDatas.Keys) {
            playerPoints.Add(player.identity.gameObject, 0);
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
            Vector2 overheadLocation = new Vector2(Random.Range(minSpawnLocation.x, maxSpawnLocation.x + 1), Random.Range(minSpawnLocation.y, maxSpawnLocation.y + 1));

            int excludePlayerLayerMask = ~LayerMask.GetMask("Player");
            if (Physics.Raycast(new Vector3(overheadLocation.x, 10f, overheadLocation.y), Vector3.down, out RaycastHit hit, 15f, excludePlayerLayerMask)) {
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
                NetworkServer.Spawn(wisp);
                break;
            }
            yield return null;
        }
    }

    public IEnumerator SpawnTerritory() {
        while (true) {
            Vector2 overheadLocation = new Vector2(Random.Range(minSpawnLocation.x, maxSpawnLocation.x + 1), Random.Range(minSpawnLocation.y, maxSpawnLocation.y + 1));

            int excludePlayerLayerMask = ~LayerMask.GetMask("Player");
            if (Physics.Raycast(new Vector3(overheadLocation.x, 10f, overheadLocation.y), Vector3.down, out RaycastHit hit, 15f, excludePlayerLayerMask)) {
                // Don't need to check for nearby territories since they immediately start moving anyways.
                GameObject territory = Instantiate(territoryPrefab, hit.point, Quaternion.identity);
                territory.GetComponent<Territory>().wispWhiskManager = this;
                NetworkServer.Spawn(territory);
                StartCoroutine(territory.GetComponent<RandomlyMovingAgent>().MoveTowardsTrans());
                break;
            }

            yield return null;
        }
    }

    /// <summary>Enable score display on all clients.</summary>
    [ClientRpc]
    public void RpcEnableScoreDisplay() {
        scoreDisplayCanvas.enabled = true;
    }

    public IEnumerator AddPointsEveryInterval() {
        foreach (var player in CustomNetworkManager.Instance.ClientDatas.Keys) {
            if (player.identity.GetComponent<WispEffect>() && player.identity.GetComponent<WispEffect>().holdingWisp) {
                int pointsToAdd = player.identity.GetComponent<WispEffect>().holdingWisp.GetComponent<CollectableWispEffect>().pointsToAdd;
                playerPoints[player.identity.gameObject] += pointsToAdd;
                TargetSetScoreDisplay(player.identity.GetComponent<NetworkIdentity>().connectionToClient, playerPoints[player.identity.gameObject]);
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
                if (playerPoint.Value == score) {
                    currentStanding.Add(playerPoint.Key.GetComponent<NetworkIdentity>().connectionToClient);
                }
            }
            minigameHandler.AddWinner(currentStanding);
        }
    }
}
