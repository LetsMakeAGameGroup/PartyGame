using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Handles that status of a minigame. Will slightly change how this works soon.</summary>
// TODO: Instead of handling this on every indivdual minigame, only one instance throughout the game would be better.
public class MinigameHandler : MonoBehaviour {
    public float minigameDuration = 120f;
    public List<List<GameObject>> winners = new();
    [SerializeField] private DisplayTimerUI displayTimerUI = null;
    [SerializeField] private MinigameScoreScreenController scoreScreenController = null;

    private bool isRunning = false;
    [SerializeField] private bool canEndGameEarly = true;

    public UnityEvent onMinigameStart = new();
    public UnityEvent onMinigameEnd = new();

    private void Start() {
        Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
        timer.duration = 3f;
        timer.onTimerEnd.AddListener(StartMinigame);

        //displayTimerUI.RpcStartCountdown(3);
    }

    /// <summary>Buffer for starting a minigame.</summary>
    public void StartMinigame() {
        Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
        timer.duration = minigameDuration;
        timer.onTimerEnd.AddListener(EndMinigame);

        displayTimerUI.RpcStartCountdown(minigameDuration);

        onMinigameStart?.Invoke();

        isRunning = true;
    }

    /// <summary>Adds player to the winners list according to position placed.</summary>
    public void AddWinner(List<GameObject> players) {
        if (!isRunning) return;

        winners.Add(players);

        if (canEndGameEarly && winners.Count == CustomNetworkManager.Instance.players.Count) {
            EndMinigame();
        }
    }

    /// <summary>Ready to end the minigame and start the next round.</summary>
    public void EndMinigame() {
        if (!isRunning) return;

        onMinigameEnd?.Invoke();

        // Assign points to winners accordingly
        int assignPoints = CustomNetworkManager.Instance.players.Count;
        foreach (List<GameObject> position in winners) {
            foreach (GameObject player in position) {
                player.GetComponent<PlayerController>().points += assignPoints;
                CustomNetworkManager.Instance.connectionScores[player.GetComponent<NetworkIdentity>().connectionToClient] += assignPoints;
                scoreScreenController.RpcAddScoreCard(player.GetComponent<PlayerController>().playerName, assignPoints);
            }
            assignPoints -= position.Count;
        }

        StartCoroutine(EndGameTransition());
    }

    IEnumerator EndGameTransition() {
        scoreScreenController.RpcEnableUI();

        yield return new WaitForSeconds(5f);

        GameManager.Instance.StartNextRound();
    }
}
