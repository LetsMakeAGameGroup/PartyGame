using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Handles that status of a minigame. Will slightly change how this works soon.</summary>
// TODO: Instead of handling this on every indivdual minigame, only one instance throughout the game would be better.
public class MinigameHandler : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private MinigameScoreScreenController scoreScreenController;
    public DisplayTimerUI displayTimerUI;
    public UnityEvent onMinigameStart = new();
    public UnityEvent onMinigameEnd = new();
    [SerializeField] private AudioSource countdownAudioSource;

    [HideInInspector] public List<List<NetworkConnectionToClient>> winners = new();
    private List<GameObject> movableObjects = new();

    [Header("Settings")]
    [Tooltip("How many seconds the minigame should last before ending.")]
    public float minigameDuration = 120f;
    [Tooltip("If the game should automatically end after MinigameDuration seconds.")]
    [SerializeField] private bool isTimerBased = true;
    [Tooltip("The amount of seconds the score screen is shown.")]
    [SerializeField] private float scoreScreenTime = 10f;

    private bool isRunning = false;
    private int winnerCount = 0;

    private void Start() {
        if (!isServer) return;

        var moveObjectOverTimes = FindObjectsOfType<MoveObjectOverTime>();
        foreach (var moveObjectOverTime in moveObjectOverTimes) {
            movableObjects.Add(moveObjectOverTime.gameObject);
        }

        var constantRotations = FindObjectsOfType<ConstantRotation>();
        foreach (var constantRotation in constantRotations) {
            movableObjects.Add(constantRotation.gameObject);
        }
    }

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

        // Start moving all movable objects.
        foreach (var movableObject in movableObjects) {
            if (movableObject.TryGetComponent(out MoveObjectOverTime moveObjectOverTime)) {
                moveObjectOverTime.canMove = true;
            }

            if (movableObject.TryGetComponent(out ConstantRotation constantRotation)) {
                constantRotation.RpcStartRotation(constantRotation.transform.rotation);
            }
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
        scoreScreenController.RpcEnableUI(scoreScreenTime);

        yield return new WaitForSeconds(scoreScreenTime);

        GameManager.Instance.StartNextRound();
    }

    private IEnumerator TimeTillCountdownAudio() {
        yield return new WaitForSeconds(5 - countdownAudioSource.clip.length);

        RpcPlayCountdownAudio();
    }

    private void RpcPlayCountdownAudio() {
        countdownAudioSource.Play();
    }
}
