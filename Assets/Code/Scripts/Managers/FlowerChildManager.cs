using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FlowerChildManager : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private MinigameHandler minigameHandler;
    [SerializeField] private GameObject wispPrefab;
    [SerializeField] private GameObject[] tornados;
    [SerializeField] private Canvas scoreDisplayCanvas;
    [SerializeField] private TextMeshProUGUI scoreDisplayText;
    [SerializeField] private InGameScoreboardController inGameScoreboardController;

    [Header("Settings")]
    [Tooltip("Minimum or bottom left 2D vector(X and Z-Axis) of where wisps can spawn in an area.")]
    [SerializeField] private Vector2 minWispSpawnLocation = Vector2.zero;
    [Tooltip("Maximum or top right 2D vector(X and Z-Axis) of where wisps can spawn in an area.")]
    [SerializeField] private Vector2 maxWispSpawnLocation = Vector2.zero;
    [Tooltip("How many seconds between spawning a wisp.")]
    [SerializeField] private float wispSpawnTimeInterval = 5f;
    [Tooltip("The minimum distance between two wisps when spawning.")]
    [SerializeField] private float distanceBetweenWisps = 1f;
    [Tooltip("The amount of speed the Tornado will increase by.")]
    [SerializeField] private float speedIncrease = 2f;
    [Tooltip("How many seconds between increasing the Tornado speed.")]
    [SerializeField] private float speedIncreaseTimeInterval = 30f;
    [Tooltip("How many points to reduce from the player when they fall. Player's points will not surpass 0.")]
    [SerializeField] private int respawnPointDeduction = 0;

    private Dictionary<GameObject, int> playerPoints = new();

    // Called by server when the minigame is started.
    public void StartGame() {
        foreach (var player in CustomNetworkManager.Instance.ClientDatas.Keys) {
            playerPoints.Add(player.identity.gameObject, 0);
            player.identity.GetComponent<PlayerMovementComponent>().TargetSetMinigameManagerObject(gameObject);
        }

        StartCoroutine(SpawnWisp());

        foreach (GameObject tornado in tornados) {
            if (tornado.TryGetComponent(out RandomlyMovingAgent randomlyMovingAgent)) {
                randomlyMovingAgent.RpcSetDestination(randomlyMovingAgent.transform.position, randomlyMovingAgent.RandomNavmeshLocation());
            }
        }
        StartCoroutine(IncreaseSpeedAfterInterval());

        RpcEnableScoreDisplay();
    }

    private IEnumerator SpawnWisp() {
        while (this) {
            Vector2 overheadLocation = new Vector2(Random.Range(minWispSpawnLocation.x, maxWispSpawnLocation.x + 1), Random.Range(minWispSpawnLocation.y, maxWispSpawnLocation.y + 1));

            int excludePlayerLayerMask = ~LayerMask.GetMask("Player");
            if (Physics.Raycast(new Vector3(overheadLocation.x, 10f, overheadLocation.y), Vector3.down, out RaycastHit hit, 15f, excludePlayerLayerMask)) {
                // Check if there is already a wisp nearby. If there is, it will continue and get a new position.
                bool isNearSomething = false;
                Collider[] intersectingColliders = Physics.OverlapSphere(hit.point, distanceBetweenWisps);
                if (intersectingColliders.Length > 0) {
                    foreach (var intersectingCollider in intersectingColliders) {
                        if (intersectingCollider.GetComponent<CollectiblePoint>() != null || intersectingCollider.GetComponent<PlayerController>() != null) {
                            isNearSomething = true;
                            break;
                        }
                    }
                }
                if (isNearSomething) {
                    yield return null;
                    continue;
                }

                GameObject wisp = Instantiate(wispPrefab, hit.point, Quaternion.identity);
                wisp.GetComponent<CollectiblePoint>().onPointsAdd.AddListener(AddPoints);
                NetworkServer.Spawn(wisp);
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(wispSpawnTimeInterval);

        if (minigameHandler.isRunning) {
            StartCoroutine(SpawnWisp());
        }
    }

    /// <summary>Gives a player points in the current minigame.</summary>
    public void AddPoints(GameObject player, int points) {
        playerPoints[player] += points;
        TargetSetScoreDisplay(player.GetComponent<NetworkIdentity>().connectionToClient, playerPoints[player]);
        inGameScoreboardController.RpcUpdateScoreCard(player.GetComponent<PlayerController>().playerName, playerPoints[player]);
    }

    /// <summary>Enable score display on all clients.</summary>
    [ClientRpc]
    public void RpcEnableScoreDisplay() {
        scoreDisplayCanvas.enabled = true;
        inGameScoreboardController.enabled = true;
    }

    private IEnumerator IncreaseSpeedAfterInterval() {
        yield return new WaitForSeconds(speedIncreaseTimeInterval);

        foreach (GameObject tornado in tornados) {
            if (tornado.TryGetComponent(out RandomlyMovingAgent randomlyMovingAgent)) {
                randomlyMovingAgent.speed += speedIncrease;
            }
        }

        StartCoroutine(IncreaseSpeedAfterInterval());
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
}
