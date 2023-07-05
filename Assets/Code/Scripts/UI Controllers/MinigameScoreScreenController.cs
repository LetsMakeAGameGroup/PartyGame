using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MinigameScoreScreenController : NetworkBehaviour {
    [SerializeField] private Transform playerScoreCardHolderTransform;
    [SerializeField] private GameObject playerScoreCardPrefab;

    private int playerCards = 0;

    [ClientRpc]
    public void RpcAddScoreCard(string name, int points) {
        GameObject playerScoreCard = Instantiate(playerScoreCardPrefab, playerScoreCardHolderTransform);
        playerScoreCard.transform.localPosition = new Vector3(0, -100 * playerCards + 400, 0);
        playerScoreCard.GetComponent<PlayerScoreCardController>().SetupScoreCard(name, points);

        playerCards++;
    }


    [ClientRpc]
    public void RpcEnableUI() {
        GetComponent<Canvas>().enabled = true;
    }
}
