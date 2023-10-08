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
    [SerializeField] private MinigameEscapeUIController escapeUIController;
    public DisplayTimerUI displayTimerUI;
    public UnityEvent onMinigameStart = new();
    public UnityEvent onMinigameEnd = new();
    [SerializeField] private AudioSource countdownAudioSource;
    [SerializeField] private AudioSource whistleStopAudioSource;

    [HideInInspector] public List<List<NetworkConnectionToClient>> winners = new();
    private List<GameObject> movableObjects = new();

    [Header("Music References")]
    [SerializeField] private AudioSource startMusicAudioSource;
    [SerializeField] private AudioSource minigameMusicAudioSource;
    [SerializeField] private AudioSource endMusicAudioSource;

    [Header("Settings")]
    [Tooltip("How many seconds the minigame should last before ending.")]
    public float minigameDuration = 120f;
    [Tooltip("If the game should automatically end after MinigameDuration seconds.")]
    [SerializeField] private bool isTimerBased = true;
    [Tooltip("The amount of seconds the score screen is shown.")]
    [SerializeField] private float scoreScreenTime = 10f;

    [HideInInspector] public bool isRunning = false;
    [HideInInspector] public bool isStarting = true;
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
        escapeUIController.GetComponent<Canvas>().enabled = false;

        Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
        timer.duration = 5f;
        timer.onTimerEnd.AddListener(StartMinigame);
        timer.onTimerEnd.AddListener(delegate { FindObjectOfType<MinigameStartScreenController>().RpcSetMovement(true); });

        displayTimerUI.RpcStartCountdown(5, false);

        RpcCountdownAudio();
    }

    /// <summary>Buffer for starting a minigame.</summary>
    /// Called once the countdown from StartCountdown hits zero.
    public void StartMinigame() {
        if (isTimerBased) {
            Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
            timer.duration = minigameDuration;
            timer.onTimerEnd = onMinigameEnd;

            displayTimerUI.RpcStartCountdown(minigameDuration, true);
        }

        // Start moving all movable objects.
        foreach (var movableObject in movableObjects) {
            if (movableObject.TryGetComponent(out MoveObjectOverTime moveObjectOverTime)) {
                moveObjectOverTime.RpcStartMovement(movableObject.transform.position, moveObjectOverTime.pathIndex);
            }

            if (movableObject.TryGetComponent(out ConstantRotation constantRotation)) {
                constantRotation.RpcStartRotation(constantRotation.transform.rotation);
            }
        }

        onMinigameStart?.Invoke();

        isRunning = true;
        isStarting = false;
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
        isRunning = false;
        RpcPlayWhistleStopAudio();

        // Assign points to winners accordingly
        int assignPoints = CustomNetworkManager.Instance.ClientDatas.Count;
        foreach (List<NetworkConnectionToClient> position in winners) {
            foreach (NetworkConnectionToClient player in position) {
                CustomNetworkManager.Instance.ClientDatas[player].score += assignPoints;
                scoreScreenController.RpcAddScoreCard(CustomNetworkManager.Instance.ClientDatas[player].displayName, assignPoints);

                player.identity.GetComponent<PlayerController>().enabled = false;
                player.identity.GetComponent<PlayerMovementComponent>().enabled = false;
                player.identity.GetComponent<ItemController>().enabled = false;
            }
            assignPoints -= position.Count;
        }
        StartCoroutine(EndGameTransition());
    }

    IEnumerator EndGameTransition() {
        scoreScreenController.RpcEnableUI(scoreScreenTime);
        RpcPlayEndMusic();

        yield return new WaitForSecondsRealtime(scoreScreenTime);

        GameManager.Instance.StartNextRound();
    }

    [ClientRpc]
    private void RpcCountdownAudio() {
        startMusicAudioSource.Stop();
        float delay = (float)(NetworkClient.connection.remoteTimeStamp / 1000);
        StartCoroutine(TimeTillCountdownAudio(5 - countdownAudioSource.clip.length - delay));
    }

    private IEnumerator TimeTillCountdownAudio(float duration) {
        yield return new WaitForSecondsRealtime(duration);

        countdownAudioSource.Play();

        yield return new WaitForSecondsRealtime(countdownAudioSource.clip.length);

        minigameMusicAudioSource.Play();
    }

    [ClientRpc]
    private void RpcPlayEndMusic() {
        minigameMusicAudioSource.Stop();
        endMusicAudioSource.Play();
    }

    [ClientRpc]
    private void RpcPlayWhistleStopAudio() {
        whistleStopAudioSource.Play();
    }
}
