using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ClaimGameManager : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private MinigameHandler minigameHandler;
    [SerializeField] private Canvas scoreDisplayCanvas;
    [SerializeField] private TextMeshProUGUI scoreDisplayText;
    [SerializeField] private GameObject[] targets;

    [Header("Settings")]
    [Tooltip("How many seconds between targets assign points to their according player.")]
    [SerializeField] private float timeBetweenGivenPoints = 0.25f;

    private Dictionary<GameObject, int> playerPoints = new();

    // Called by server when the minigame is started.
    public void EnableTargets() {
        foreach (var player in CustomNetworkManager.Instance.players) {
            playerPoints.Add(player, 0);
        }

        RpcEnableTargets();

        StartCoroutine(AddPointsEverySecond());
    }

    /// <summary>Replace maze presets on all clients.</summary>
    [ClientRpc]
    public void RpcEnableTargets() {
        scoreDisplayCanvas.enabled = true;

        foreach (var target in targets) {
            target.SetActive(true);
        }
    }

    public IEnumerator AddPointsEverySecond() {
        foreach (var target in targets) {
            GameObject targetOwner = target.GetComponent<CaptureTarget>().playerOwner;
            if (targetOwner) {
                int pointsToAdd = target.GetComponent<CaptureTarget>().pointsGiven;
                playerPoints[targetOwner] += pointsToAdd;

                TargetSetScoreDisplay(targetOwner.GetComponent<NetworkIdentity>().connectionToClient, playerPoints[targetOwner]);
            }
        }

        yield return new WaitForSeconds(timeBetweenGivenPoints);
        StartCoroutine(AddPointsEverySecond());
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
