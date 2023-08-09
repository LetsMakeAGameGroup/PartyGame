using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FlowerChildManager : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private MinigameHandler minigameHandler;
    [SerializeField] private GameObject flowerSpirit;
    [SerializeField] private Canvas scoreDisplayCanvas;
    [SerializeField] private TextMeshProUGUI scoreDisplayText;

    [Header("Settings")]
    [Tooltip("How many seconds between targets assign points to their according player.")]
    [SerializeField] private float timeBetweenGivenPoints = 0.25f;
    [Tooltip("The amount of speed the FlowerSpirit will increase by.")]
    [SerializeField] private float speedIncrease = 2f;
    [Tooltip("How many seconds between increasing the FlowerSpirit speed.")]
    [SerializeField] private float speedIncreaseTimeInterval = 30f;

    private Dictionary<GameObject, int> playerPoints = new();

    // Called by server when the minigame is started.
    public void StartMovingFlowerSpirit() {
        foreach (var player in CustomNetworkManager.Instance.ClientDatas.Keys) {
            playerPoints.Add(player.identity.gameObject, 0);
        }

        StartCoroutine(flowerSpirit.GetComponent<RandomlyMovingAgent>().MoveTowardsTrans());
        StartCoroutine(IncreaseSpeedAfterInterval());

        RpcEnableScoreDisplay();
        StartCoroutine(AddPointsEveryInterval());
    }

    /// <summary>Enable score display on all clients.</summary>
    [ClientRpc]
    public void RpcEnableScoreDisplay() {
        scoreDisplayCanvas.enabled = true;
    }

    private IEnumerator IncreaseSpeedAfterInterval() {
        yield return new WaitForSeconds(speedIncreaseTimeInterval);

        flowerSpirit.GetComponent<RandomlyMovingAgent>().speed += speedIncrease;

        StartCoroutine(IncreaseSpeedAfterInterval());
    }

    public IEnumerator AddPointsEveryInterval() {
        foreach (var player in flowerSpirit.GetComponent<ContainPlayersInsideCollider>().playersInside) {
            playerPoints[player]++;

            TargetSetScoreDisplay(player.GetComponent<NetworkIdentity>().connectionToClient, playerPoints[player]);
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
