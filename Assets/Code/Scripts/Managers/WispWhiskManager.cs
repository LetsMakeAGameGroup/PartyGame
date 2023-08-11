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

    [Header("Settings")]
    [Tooltip("How many seconds between wisps assign points to their holding player.")]
    [SerializeField] private float timeBetweenGivenPoints = 0.25f;
    [Tooltip("Minimum or bottom left 2D vector(X and Z-Axis) of where wisps can spawn in an area.")]
    [SerializeField] private Vector2 minWispSpawnLocation = Vector2.zero;
    [Tooltip("Maximum or top right 2D vector(X and Z-Axis) of where wisps can spawn in an area.")]
    [SerializeField] private Vector2 maxWispSpawnLocation = Vector2.zero;
    [Tooltip("How far from the ground the wisp will spawn.")]
    [SerializeField] private float distanceFromGroundWispSpawn = 0.5f;
    [Tooltip("The minimum distance between two wisps when spawning.")]
    [SerializeField] private float distanceBetweenWisps = 1f;

    private Dictionary<GameObject, int> playerPoints = new();

    // Called by server when the minigame starts.
    public void SpawnWisps() {
        foreach (var player in CustomNetworkManager.Instance.ClientDatas.Keys) {
            playerPoints.Add(player.identity.gameObject, 0);
        }

        //Spawnwisps here
        int wispCount = 1;
        if (CustomNetworkManager.Instance.ClientDatas.Count > 4) wispCount = 2;
        if (CustomNetworkManager.Instance.ClientDatas.Count > 6) wispCount = 3;

        for (int i = 0; i < wispCount; i++) {
            StartCoroutine(SpawnWisp());
        }
        RpcEnableScoreDisplay();

        StartCoroutine(AddPointsEveryInterval());
    }

    private IEnumerator SpawnWisp() {
        while (true) {
            Vector2 overheadLocation = new Vector2(Random.Range(minWispSpawnLocation.x, maxWispSpawnLocation.x + 1), Random.Range(minWispSpawnLocation.y, maxWispSpawnLocation.y + 1));

            int excludePlayerLayerMask = ~LayerMask.GetMask("Player");
            if (Physics.Raycast(new Vector3(overheadLocation.x, 10f, overheadLocation.y), Vector3.down, out RaycastHit hit, 15f, excludePlayerLayerMask)) {
                // Check if there is already a wisp nearby. If there is, it will continue and get a new position.
                bool isNearWisp = false;
                Collider[] intersectingColliders = Physics.OverlapSphere(hit.point + new Vector3(0, distanceFromGroundWispSpawn, 0), distanceBetweenWisps);
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

                GameObject wisp = Instantiate(wispPrefab, hit.point + new Vector3(0, distanceFromGroundWispSpawn, 0), Quaternion.identity);
                NetworkServer.Spawn(wisp);
                break;
            }
            yield return null;
        }

        yield return null;
        StartCoroutine(SpawnWisp());
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
                foreach (var territory in territories) {
                    if (territory.GetComponent<ContainPlayersInsideCollider>().playersInside.Contains(player.identity.gameObject)) {
                        pointsToAdd += (int)(pointsToAdd * territory.GetComponent<ContainPlayersInsideCollider>().pointsMultiplier);
                        break;
                    }
                }

                playerPoints[player.identity.gameObject] += pointsToAdd;

                TargetSetScoreDisplay(player.identity.GetComponent<NetworkIdentity>().connectionToClient, playerPoints[player.identity.gameObject]);
            }
        }

        yield return new WaitForSeconds(timeBetweenGivenPoints);
        StartCoroutine(AddPointsEveryInterval());
    }

    [TargetRpc]
    private void TargetSetScoreDisplay(NetworkConnectionToClient target, int score) {
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
