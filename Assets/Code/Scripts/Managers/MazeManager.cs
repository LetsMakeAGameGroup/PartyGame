using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeManager : NetworkBehaviour {
    [SerializeField] private MinigameHandler minigameHandler = null;
    [SerializeField] private float mazePerSec = 15f;
    [SerializeField] private GameObject[] mazePresets = null;
    [SerializeField] private GameObject startingMaze = null;
    public Dictionary<GameObject, int> playerPoints = new();

    [SerializeField] private int wispsPerMaze = 2;
    [SerializeField] private GameObject wisp = null;

    private int mazeIndex = -1;

    private void Start() {
        if (!isServer) return;

        // Initial randomized maze
        //RpcReplaceMazePreset(0, Random.Range(0, mazePresets.Length));
        //RandomizeMaze();
    }

    /// <summary>Start the timers for maze generation.</summary>
    public void StartMazeIntervals() {
        // Timers for each random maze generation in intervals since game has started.
        for (int i = 1; i < minigameHandler.minigameDuration / mazePerSec; i++) {
            Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
            timer.duration = mazePerSec * i;
            timer.onTimerEnd.AddListener(RandomizeMaze);
        }
    }

    /// <summary>Choose a random maze preset.</summary>
    public void RandomizeMaze() {
        int lastIndex = mazeIndex;
        mazeIndex = Random.Range(0, mazePresets.Length - 1);
        if (mazeIndex == lastIndex) mazeIndex = (mazeIndex + 1) % mazePresets.Length;

        CollectiblePoint[] newWisps = FindObjectsOfType<CollectiblePoint>();
        foreach (CollectiblePoint wisp in newWisps) {
            NetworkServer.Destroy(wisp.gameObject);
        }

        for (int i = 0; i < wispsPerMaze; i++) {
            Vector3 randomSpawn = new Vector3(Random.Range(-4, 5)*10-5, 1.5f, Random.Range(-4, 5)*10-5);
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
            List<GameObject> currentStanding = new();
            Debug.Log("Adding for score " + score);
            foreach (var playerPoint in playerPoints) {
                if (playerPoint.Value == score) {
                    currentStanding.Add(playerPoint.Key);
                    Debug.Log("Player " + playerPoint.Key.GetComponent<PlayerController>().playerName + " with score " + score);
                }
            }
            minigameHandler.AddWinner(currentStanding);
        }
    }
}
