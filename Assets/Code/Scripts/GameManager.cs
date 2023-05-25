using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    public int round = 0;
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

    public void StartNextRound() {
        if (round < currentMinigames.Count) {
            CustomNetworkManager.Instance.ServerChangeScene(currentMinigames[round]);
            round++;
        } else {
            CustomNetworkManager.Instance.ServerChangeScene("LobbyScene");
            round = 0;
            RandomizeCurrentMinigames();
        }
    }

    [TargetRpc]
    public void TargetTeleportPlayer(NetworkConnectionToClient target, Vector3 telePos) {
        GameObject player = target.identity.gameObject;
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = telePos;
        player.GetComponent<CharacterController>().enabled = true;
    }

    private void RandomizeCurrentMinigames() {
        currentMinigames = minigames.OrderBy(x => Random.value).ToList();
    }
}
