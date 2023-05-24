using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void StartGame() {
        RpcLoadScene(minigames[round]);
        round++;
    }

    //[ClientRpc]
    public void RpcLoadScene(string sceneName) {
        StartCoroutine(Countdown(sceneName));
    }

    IEnumerator Countdown(string sceneName) {
        for (int i = 3; i > 0; i--) {
            Debug.Log("Changing scenes in " + i);
            yield return new WaitForSeconds(1);
        }

        Debug.Log("Now changing scenes to " + sceneName);
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
