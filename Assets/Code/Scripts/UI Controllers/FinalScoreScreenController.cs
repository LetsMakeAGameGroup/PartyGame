using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;

public class FinalScoreScreenController : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Transform finalScoreCardHolderTransform;
    [SerializeField] private GameObject finalScoreCardPrefab;
    [SerializeField] private TMP_Text winnerNameText;
    [SerializeField] private TMP_Text winnerScoreText;
    [SerializeField] private RectTransform timerRect;

    [Header("Settings")]
    [Tooltip("The amount of seconds before this screen takes you back to the lobby.")]
    [SerializeField] private float endTimer = 10f;
    [Tooltip("The amount of seconds before revealing each standing.")]
    [SerializeField] private float standingInterval = 2.5f;

    private int playerCards = 0;
    private float timerWidth = 1350f;
    private SyncDictionary<string, int> scoresInOrder = new();
    private List<GameObject> finalScoreCards = new();
    
    private void Start() {
        if (isServer) {
            var scoresInOrderTemp = CustomNetworkManager.Instance.ClientDatas.OrderByDescending(pair => pair.Value.score).ToDictionary(pair => pair.Key.identity.GetComponent<PlayerController>().playerName, pair => pair.Value.score);
            foreach(var scoreCard in scoresInOrderTemp) scoresInOrder.Add(scoreCard);
        }

        foreach (var score in scoresInOrder) {
            AddScoreCard(score.Key, score.Value);
        }

        if (isServer) StartCoroutine(RevealScoresAnimation());
    }

    private IEnumerator RevealScoresAnimation() {
        for (int i = finalScoreCards.Count - 1; i >= 0; i--) {
            yield return new WaitForSeconds(standingInterval);

            RpcEnableCard(i);
        }

        yield return new WaitForSeconds(standingInterval);

        RpcEnableWinnerCard();
        RpcEnableTimer(endTimer);

        yield return new WaitForSeconds(endTimer);

        GameManager.Instance.ResetLobby();
    }

    [ClientRpc]
    private void RpcEnableCard(int cardIndex) {
        finalScoreCards[cardIndex].SetActive(true);
    }

    [ClientRpc]
    private void RpcEnableWinnerCard() {
        winnerNameText.enabled = true;
        winnerScoreText.enabled = true;
    }

    public void AddScoreCard(string name, int points) {
        if (playerCards == 0) {
            winnerNameText.text = name + " is the winner!";
            winnerScoreText.text = "+" + points.ToString() + " Point" + (points == 1 ? " " : "s");
        } else {
            GameObject playerScoreCard = Instantiate(finalScoreCardPrefab, finalScoreCardHolderTransform);
            playerScoreCard.GetComponent<FinalScoreCardController>().SetupScoreCard(playerCards, name, points);
            finalScoreCards.Add(playerScoreCard);
        }

        playerCards++;
    }

    [ClientRpc]
    public void RpcEnableTimer(float time) {
        StartCoroutine(TimerDisplay(time));
    }

    private IEnumerator TimerDisplay(float time) {
        timerWidth = timerRect.sizeDelta.x;
        float currentTime = time;

        while (currentTime > 0f) {
            currentTime -= Time.deltaTime;
            timerRect.sizeDelta = new Vector2((currentTime/time) * timerWidth, timerRect.sizeDelta.y);
            yield return null;
        }
    }
}
