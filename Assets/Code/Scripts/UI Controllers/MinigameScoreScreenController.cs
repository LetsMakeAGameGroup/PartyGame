using UnityEngine;
using Mirror;
using TMPro;
using System.Collections;

public class MinigameScoreScreenController : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Transform playerScoreCardHolderTransform;
    [SerializeField] private GameObject playerScoreCardPrefab;
    [SerializeField] private TMP_Text winnerNameText;
    [SerializeField] private TMP_Text winnerScoreText;
    [SerializeField] private RectTransform timerRect;

    private int playerCards = 0;
    private float timerWidth = 1350f;

    [ClientRpc]
    public void RpcAddScoreCard(string name, int points) {
        if (playerCards == 0) {
            SetWinnerCard(name, points);
            return;
        }

        GameObject playerScoreCard = Instantiate(playerScoreCardPrefab, playerScoreCardHolderTransform);
        playerScoreCard.GetComponent<PlayerScoreCardController>().SetupScoreCard(playerCards, name, points);

        playerCards++;
    }

    public void SetWinnerCard(string name, int points) {
        winnerNameText.text = name + " is the winner!";
        winnerScoreText.text = "+" + points.ToString() + " Point" + (points == 1 ? " " : "s");

        playerCards++;
    }


    [ClientRpc]
    public void RpcEnableUI(float time) {
        StartCoroutine(TimerDisplay(time));
        
        GetComponent<Canvas>().enabled = true;
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
