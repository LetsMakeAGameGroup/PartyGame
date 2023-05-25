using Mirror;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    public int round = 0;
    public int maxRounds = 5;
    public List<string> minigames = new();
    private List<string> currentMinigames = new();
    

    public static GameManager Instance { get; private set; }

    public void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void Start() {
        DontDestroyOnLoad(gameObject);
        RandomizeCurrentMinigames();
    }

    // Cycle to the next round, go back to the lobby once finished with the minigame set.
    public void StartNextRound() {
        if (round < currentMinigames.Count && round < maxRounds) {
            CustomNetworkManager.Instance.ServerChangeScene(currentMinigames[round]);
            round++;
        } else {
            CustomNetworkManager.Instance.ServerChangeScene("LobbyScene");
            round = 0;
            string winner = "";
            int winnerAmount = -1;
            foreach (GameObject player in CustomNetworkManager.Instance.players) {
                if (player.GetComponent<PlayerController>().points > winnerAmount) {
                    winner = player.GetComponent<PlayerController>().playerName;
                    winnerAmount = player.GetComponent<PlayerController>().points;
                }
                player.GetComponent<PlayerController>().points = 0;
            }
            Debug.Log($"Ultimate winner: {winner} with {winnerAmount} total points.");  // Change this to show winners on screen
            RandomizeCurrentMinigames();
        }
    }

    // Tell the player to teleport to a location. We do this because player has position authority(position is sent from client to server).
    [TargetRpc]
    public void TargetTeleportPlayer(NetworkConnectionToClient target, Vector3 telePos, Quaternion teleRot) {
        GameObject player = target.identity.gameObject;
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = telePos;
        player.transform.rotation = teleRot;
        player.GetComponent<CharacterController>().enabled = true;
    }

    // Randomize list of minigames available order chosen into the current set.
    private void RandomizeCurrentMinigames() {
        // TODO: Maybe there is a more efficient way of picking random minigames that fall under maxRounds. Once we have a LOT of minigames to randomize, there might be a better way.
        currentMinigames = minigames.OrderBy(x => Random.value).ToList();
    }

    [TargetRpc]
    public void TargetSetCodeUI(NetworkConnectionToClient target, string code) {
        FindObjectOfType<LobbyUIController>().codeText.text = code;
    }
}
