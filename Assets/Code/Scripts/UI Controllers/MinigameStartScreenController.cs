using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using Unity.VisualScripting;

public class MinigameStartScreenController : NetworkBehaviour {
    [SerializeField] private TextMeshProUGUI playersReadyText = null;

    [SerializeField] private UnityEvent onStartMinigame = new();

    [SyncVar] private int playersReady = 0;
    [SyncVar] private int totalPlayers = 0;

    private void Start() {
        if (isClient) {
            Debug.Log("helloooo");
            NetworkClient.localPlayer.GetComponent<PlayerMovementComponent>().enabled = false;
        }

        if (isServer) {
            totalPlayers = CustomNetworkManager.Instance.numPlayers;
        }

        UpdatePlayersReady();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReadyUp() {
        CmdReadyUp();
    }

    [Command(requiresAuthority = false)]
    public void CmdReadyUp() {
        playersReady++;
        RpcReadyUp();

        if (playersReady == totalPlayers) {
            RpcDisableUI();
            onStartMinigame?.Invoke();
        }
    }

    [ClientRpc]
    private void RpcReadyUp() {
        UpdatePlayersReady();
    }

    private void UpdatePlayersReady() {
        playersReadyText.text = playersReady + " / " + totalPlayers + " Players Ready";
    }

    [ClientRpc]
    private void RpcDisableUI() {
        GetComponent<Canvas>().enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    [ClientRpc]
    public void RpcSetMovement(bool canMove) {
        NetworkClient.localPlayer.GetComponent<PlayerMovementComponent>().enabled = canMove;
    }
}
