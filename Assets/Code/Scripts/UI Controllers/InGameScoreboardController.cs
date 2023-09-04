using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(Animator))]
public class InGameScoreboardController : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Transform playerCurrentScoreHolderTransform;
    [SerializeField] private GameObject playerCurrentScoreCardPrefab;

    private Dictionary<string, PlayerCurrentScoreCardController> playerCurrentScoreCards = new();
    private Animator animator;

    private void OnEnable() {
        animator = GetComponent<Animator>();
        
        if (!isServer) return;

        foreach (var player in CustomNetworkManager.Instance.ClientDatas) {
            RpcAddPlayerCurrentScoreCard(player.Value.displayName);
        }
    }

    [ClientRpc]
    private void RpcAddPlayerCurrentScoreCard(string playerName) {
        GameObject newPlayerCurrentScoreCard = Instantiate(playerCurrentScoreCardPrefab, playerCurrentScoreHolderTransform);
        newPlayerCurrentScoreCard.GetComponent<PlayerCurrentScoreCardController>().InitializeCard(playerName);

        if (NetworkClient.localPlayer.GetComponent<PlayerController>().playerName == playerName) {
            newPlayerCurrentScoreCard.GetComponent<PlayerCurrentScoreCardController>().SetLocalTextColor();
        }

        playerCurrentScoreCards.Add(playerName, newPlayerCurrentScoreCard.GetComponent<PlayerCurrentScoreCardController>());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            animator.SetBool("isEnabled", true);
        } else if (Input.GetKeyUp(KeyCode.Tab)) {
            animator.SetBool("isEnabled", false);
        }
    }

    [ClientRpc]
    public void RpcUpdateScoreCard(string playerName, int score) {
        if (playerCurrentScoreCards.ContainsKey(playerName)) {
            playerCurrentScoreCards[playerName].UpdateScore(score);
        }
    }
}
