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

    /// <summary>Cycle to the next round, go back to the lobby once finished with the minigame set.</summary>
    public void StartNextRound() {
        if (round < currentMinigames.Count && round < maxRounds) {
            CustomNetworkManager.Instance.ServerChangeScene(currentMinigames[round]);
            round++;
        } else {
            // maxRounds have been reached and sending players back to the lobby
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

    /// <summary>Randomize list of minigames available order chosen into the current set.</summary>
    private void RandomizeCurrentMinigames() {
        // TODO: There should be a more efficient way of picking random minigames that fall under maxRounds. Once we have a LOT of minigames to randomize, there should be a better way. Look more into this later.
        currentMinigames = minigames.OrderBy(x => Random.value).ToList();
    }

    /// <summary>Set the lobby code in the player's UI.</summary>
    // TODO: Change this once we have a better UI system.
    [TargetRpc]
    public void TargetSetCodeUI(NetworkConnectionToClient target, string code) {
        FindObjectOfType<LobbyUIController>().codeText.text = code;
    }
}
