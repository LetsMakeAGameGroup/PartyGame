using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

public class WispWhiskManager : NetworkBehaviour {
    [SerializeField] private MinigameHandler minigameHandler = null;
    [SerializeField] private GameObject[] territories = null;
    [SerializeField] private GameObject wispPrefab = null;
    [SerializeField] private float timeBetweenGivenPoints = 0.25f;
    private Dictionary<GameObject, int> playerPoints = new();

    [SerializeField] private Canvas scoreDisplayCanvas = null;
    [SerializeField] private TextMeshProUGUI scoreDisplayText = null;

    // 2D vector of where wisps can spawn in an arena. Not 3D because wisps will always spawn at highest point.
    [SerializeField] private Vector2 minWispSpawnLocation = Vector2.zero;
    [SerializeField] private Vector2 maxWispSpawnLocation = Vector2.zero;
    [SerializeField] private float distanceFromGroundWispSpawn = 0.5f;

    // Called by server.
    public void SpawnWisps() {
        //Spawnwisps here
        int wispCount = 1;
        if (CustomNetworkManager.Instance.players.Count > 4) wispCount = 2;
        if (CustomNetworkManager.Instance.players.Count > 6) wispCount = 3;

        for (int i = 0; i < wispCount; i++) {
            Vector2 overheadLocation = Vector2.Lerp(minWispSpawnLocation, maxWispSpawnLocation, Random.value);

            int excludePlayerLayerMask = ~LayerMask.GetMask("Player");
            if (Physics.Raycast(new Vector3(overheadLocation.x, 10f, overheadLocation.y), Vector3.down, out RaycastHit hit, 15f, excludePlayerLayerMask)) {
                GameObject wisp = Instantiate(wispPrefab, hit.point + new Vector3(0, distanceFromGroundWispSpawn, 0), Quaternion.identity);
                NetworkServer.Spawn(wisp);
            } else {
                Debug.LogError("Unable to find a location to spawn wisp at " + overheadLocation + ". Will attempt to spawn another.", transform);
                wispCount++;
            } 
        }
        RpcEnableScoreDisplay();

        StartCoroutine(AddPointsEveryInterval());
    }

    /// <summary>Enable score display on all clients.</summary>
    [ClientRpc]
    public void RpcEnableScoreDisplay() {
        scoreDisplayCanvas.enabled = true;
    }

    public IEnumerator AddPointsEveryInterval() {
        foreach (var player in CustomNetworkManager.Instance.connectionNames.Keys) {
            if (player.identity.GetComponent<WispEffect>() && player.identity.GetComponent<WispEffect>().holdingWisp) {
                Debug.Log("!!!");
                int pointsToAdd = player.identity.GetComponent<WispEffect>().holdingWisp.GetComponent<CollectableWispEffect>().pointsToAdd;
                foreach (var territory in territories) {
                    if (territory.GetComponent<ContainPlayersInsideCollider>().playersInside.Contains(player.identity.gameObject)) {
                        pointsToAdd += (int)(pointsToAdd * territory.GetComponent<ContainPlayersInsideCollider>().pointsMultiplier);
                        break;
                    }
                }

                if (playerPoints.ContainsKey(player.identity.gameObject)) {
                    playerPoints[player.identity.gameObject] += pointsToAdd;
                } else {
                    playerPoints.Add(player.identity.gameObject, pointsToAdd);
                }

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
            List<GameObject> currentStanding = new();
            foreach (var playerPoint in playerPoints) {
                if (playerPoint.Value == score) {
                    currentStanding.Add(playerPoint.Key);
                }
            }
            minigameHandler.AddWinner(currentStanding);
        }
    }
}
