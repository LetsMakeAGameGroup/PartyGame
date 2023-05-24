using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    public int minPlayers = 2;
    public int round = 0;
    public List<string> minigames = new();

    public static GameManager Instance { get; private set; }

    public void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void Start() {
        DontDestroyOnLoad(gameObject);
    }

    public void StartNextRound() {
        StartCoroutine(CountdownMinigame(minigames[round]));
        round++;
    }

    IEnumerator CountdownMinigame(string sceneName) {
        for (int i = 3; i > 0; i--) {
            // TODO: Make UI show countdown here
            yield return new WaitForSeconds(1);
        }

        CustomNetworkManager.Instance.ServerChangeScene(sceneName);
    }

    [TargetRpc]
    public void TargetTeleportPlayer(NetworkConnectionToClient target, Vector3 telePos) {
        GameObject player = target.identity.gameObject;
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = telePos;
        player.GetComponent<CharacterController>().enabled = true;
    }
}
