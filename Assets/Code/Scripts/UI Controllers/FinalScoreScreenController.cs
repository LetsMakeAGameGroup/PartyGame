using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class FinalScoreScreenController : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Transform finalScoreCardHolderTransform;
    [SerializeField] private GameObject finalScoreCardPrefab;

    private SyncDictionary<string, int> scoresInOrder = new();
    private List<GameObject> finalScoreCards = new();
    
    private void Start() {
        if (isServer) {
            var scoresInOrderTemp = CustomNetworkManager.Instance.connectionScores.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key.identity.GetComponent<PlayerController>().playerName, pair => pair.Value);
            foreach(var scoreCard in scoresInOrderTemp) scoresInOrder.Add(scoreCard);
        }

        foreach (var score in scoresInOrder) {
            AddScoreCard(score.Key, score.Value);
        }

        if (isServer) StartCoroutine(RevealScoresAnimation());
    }

    public void AddScoreCard(string name, int finalScore) {
        GameObject finalScoreCard = Instantiate(finalScoreCardPrefab, finalScoreCardHolderTransform);
        finalScoreCard.transform.localPosition = new Vector3(0, -100 * finalScoreCards.Count + 400, 0);
        finalScoreCard.GetComponent<FinalScoreCardController>().SetupScoreCard(name, finalScore);

        finalScoreCards.Add(finalScoreCard);
    }

    private IEnumerator RevealScoresAnimation() {
        for (int i = finalScoreCards.Count - 1; i >= 0; i--) {
            yield return new WaitForSeconds(1f);

            RpcEnableCard(i);
        }

        yield return new WaitForSeconds(5f);

        GameManager.Instance.ResetLobby();
    }

    [ClientRpc]
    private void RpcEnableCard(int cardIndex) {
        finalScoreCards[cardIndex].SetActive(true);
    }
}
