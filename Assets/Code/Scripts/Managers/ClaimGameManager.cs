using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClaimGameManager : NetworkBehaviour {
    [SerializeField] private MinigameHandler minigameHandler = null;
    [SerializeField] private GameObject[] targets = null;
    private Dictionary<GameObject, int> playerPoints = new();

    // Called by server.
    public void EnableTargets() {
        RpcEnableTargets();

        StartCoroutine(AddPointsEverySecond());
    }

    /// <summary>Replace maze presets on all clients.</summary>
    [ClientRpc]
    public void RpcEnableTargets() {
        foreach (var target in targets) {
            target.SetActive(true);
        }
    }

    public IEnumerator AddPointsEverySecond() {
        foreach (var target in targets) {
            GameObject targetOwner = target.GetComponent<CaptureTarget>().playerOwner;
            if (targetOwner) {
                int pointsToAdd = target.GetComponent<CaptureTarget>().pointsPerSec;
                if (playerPoints.ContainsKey(targetOwner)) {
                    playerPoints[targetOwner] += pointsToAdd;
                } else {
                    playerPoints.Add(targetOwner, pointsToAdd);
                }
            }
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(AddPointsEverySecond());
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
