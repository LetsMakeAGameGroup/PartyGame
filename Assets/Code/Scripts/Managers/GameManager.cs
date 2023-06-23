using Mirror;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
            // maxRounds have been reached, sending players to the final score scene to see the game results before going back to the lobby.
            CustomNetworkManager.Instance.ServerChangeScene("FinalScoreScene");
        }
    }

    /// <summary>Send players to the lobby and reset everything for a new game. Includes resetting scores and minigames.</summary>
    public void ResetLobby() {
        CustomNetworkManager.Instance.ServerChangeScene("LobbyScene");

        round = 0;

        foreach (var key in CustomNetworkManager.Instance.connectionScores.Keys.ToList()) {
            CustomNetworkManager.Instance.connectionScores[key] = 0;
        }

        RandomizeCurrentMinigames();
    }

    /// <summary>Randomize list of minigames available order chosen into the current set.</summary>
    private void RandomizeCurrentMinigames() {
        // TODO: There should be a more efficient way of picking random minigames that fall under maxRounds. Once we have a LOT of minigames to randomize, there should be a better way. Look more into this later.
        currentMinigames = minigames.OrderBy(x => Random.value).ToList();
    }

    /// <summary>Set the lobby code in the player's UI.</summary>
    [TargetRpc]
    public void TargetSetCodeUI(NetworkConnectionToClient target, string code) {
        FindObjectOfType<LobbyUIController>().codeText.text = code;
    }

    /// <summary>Set the lobby code in the everyones UI.</summary>
    [ClientRpc]
    public void RpcSetCodeUI(string code) {
        FindObjectOfType<LobbyUIController>().codeText.text = code;
    }
}
