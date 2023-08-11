using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeManager : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private MinigameHandler minigameHandler;
    [SerializeField] private GameObject startingMaze;
    [SerializeField] private GameObject[] mazePresets;
    [SerializeField] private GameObject wisp;

    [HideInInspector] public Dictionary<GameObject, int> playerPoints = new();

    [Header("Settings")]
    [Tooltip("How often the maze will randomize.")]
    [SerializeField] private float randomizeMazeInterval = 15f;
    [Tooltip("How many wisps should spawn when the maze is randomized.")]
    [SerializeField] private int wispsPerRandomization = 2;
    [Tooltip("Assuming the entire grid is a square, this is how many containers fit in one length.")]
    [SerializeField] private int gridSize = 10;
    [Tooltip("How long one length is of each container.")]
    [SerializeField] private float containerLength = 10f;

    private int mazeIndex = -1;

    /// <summary>Start the timers for maze generation as the minigame starts.</summary>
    public void StartMazeIntervals() {
        foreach (var player in CustomNetworkManager.Instance.ClientDatas.Keys) {
            playerPoints.Add(player.identity.gameObject, 0);
        }

        // Timers for each random maze generation in intervals since game has started.
        for (int i = 1; i < minigameHandler.minigameDuration / randomizeMazeInterval; i++) {
            Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
            timer.duration = randomizeMazeInterval * i;
            timer.onTimerEnd.AddListener(RandomizeMaze);
        }
    }

    /// <summary>Choose a random maze preset.</summary>
    public void RandomizeMaze() {
        int lastIndex = mazeIndex;
        mazeIndex = Random.Range(0, mazePresets.Length - 1);
        if (mazeIndex == lastIndex) mazeIndex = (mazeIndex + 1) % mazePresets.Length;

        for (int i = 0; i < wispsPerRandomization; i++) {
            Vector3 randomSpawn;
            while (true) {
                randomSpawn = new Vector3(Random.Range(-(gridSize/2) + 1, (gridSize/2) + 1)*containerLength-(containerLength/2), 1.5f, Random.Range(-(gridSize/2) + 1, (gridSize/2) + 1)*containerLength-(containerLength/2));

                Collider[] intersectingColliders = Physics.OverlapSphere(new Vector3(2, 4, 0), 0.01f);
                if (intersectingColliders.Length > 0) {
                    foreach (var intersectingCollider in intersectingColliders) {
                        if (intersectingCollider.GetComponent<CollectiblePoint>() != null) {
                            continue;
                        }
                    }
                }

                break;
            }

            GameObject currentWisp = Instantiate(wisp, randomSpawn, Quaternion.identity);
            currentWisp.GetComponent<CollectiblePoint>().onPointsAdd.AddListener(AddPoints);
            NetworkServer.Spawn(currentWisp);
        }

        RpcReplaceMazePreset(lastIndex, mazeIndex);
    }

    /// <summary>Replace maze presets on all clients.</summary>
    [ClientRpc]
    public void RpcReplaceMazePreset(int oldIndex, int newIndex) {
        if (oldIndex == -1) {
            startingMaze.SetActive(false);
        } else {
            mazePresets[oldIndex].SetActive(false);
        }
        mazePresets[newIndex].SetActive(true);
    }

    /// <summary>Gives a player points in the current minigame.</summary>
    public void AddPoints(GameObject player, int points) {
        if (playerPoints.ContainsKey(player)) {
            playerPoints[player] += points;
        } else {
            playerPoints.Add(player, points);
        }
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
