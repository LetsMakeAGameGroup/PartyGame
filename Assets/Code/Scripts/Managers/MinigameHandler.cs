using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Handles that status of a minigame. Will slightly change how this works soon.</summary>
// TODO: Instead of handling this on every indivdual minigame, only one instance throughout the game would be better.
public class MinigameHandler : MonoBehaviour {
    [Header("References")]
    [SerializeField] private MinigameScoreScreenController scoreScreenController;
    public DisplayTimerUI displayTimerUI;
    public UnityEvent onMinigameStart = new();
    public UnityEvent onMinigameEnd = new();
    [SerializeField] private CountdownMinigame countdownMinigame;

    [HideInInspector] public List<List<NetworkConnectionToClient>> winners = new();

    [Header("Settings")]
    [Tooltip("How many seconds the minigame should last before ending.")]
    public float minigameDuration = 120f;
    [Tooltip("If the game should automatically end after MinigameDuration seconds.")]
    [SerializeField] private bool isTimerBased = true;

    private bool isRunning = false;
    private int winnerCount = 0;

    // This is called once all players are ready to start the minigame. Starts a countdown before calling StartMinigame.
    public void StartCountdown() {
        FindObjectOfType<MinigameStartScreenController>().RpcSetPlayerController(true);

        Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
        timer.duration = 5f;
        timer.onTimerEnd.AddListener(StartMinigame);
        timer.onTimerEnd.AddListener(delegate { FindObjectOfType<MinigameStartScreenController>().RpcSetMovement(true); });

        displayTimerUI.RpcStartCountdown(5);

        StartCoroutine(TimeTillCountdownAudio());
    }

    /// <summary>Buffer for starting a minigame.</summary>
    /// Called once the countdown from StartCountdown hits zero.
    public void StartMinigame() {
        if (isTimerBased) {
            Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
            timer.duration = minigameDuration;
            timer.onTimerEnd = onMinigameEnd;

            displayTimerUI.RpcStartCountdown(minigameDuration);
        }

        onMinigameStart?.Invoke();

        isRunning = true;
    }

    /// <summary>Adds player to the winners list according to position placed.</summary>
    /// Should be called from a minigame specific manager event.
    public void AddWinner(List<NetworkConnectionToClient> players) {
        if (!isRunning) return;

        winnerCount += players.Count;
        winners.Add(players);

        if (winnerCount == CustomNetworkManager.Instance.ClientDatas.Count) {
            EndMinigame();
        }
    }

    /// <summary>Ready to end the minigame and start the next round.</summary>
    public void EndMinigame() {
        if (!isRunning) return;

        // Assign points to winners accordingly
        int assignPoints = CustomNetworkManager.Instance.ClientDatas.Count;
        foreach (List<NetworkConnectionToClient> position in winners) {
            foreach (NetworkConnectionToClient player in position) {
                CustomNetworkManager.Instance.ClientDatas[player].score += assignPoints;
                scoreScreenController.RpcAddScoreCard(CustomNetworkManager.Instance.ClientDatas[player].displayName, assignPoints);
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

    private IEnumerator TimeTillCountdownAudio() {
        yield return new WaitForSeconds(5 - countdownMinigame.countdownAudioSource.clip.length);

        countdownMinigame.RpcPlayCountdownAudio();
    }
}
