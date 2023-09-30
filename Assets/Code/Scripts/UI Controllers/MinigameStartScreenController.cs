using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MinigameStartScreenController : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI playersReadyText;
    [SerializeField] private UnityEvent onStartMinigame = new();
    [SerializeField] private Camera thumbnailCamera;

    private readonly List<string> readyPlayers = new();
    [SyncVar] private int totalPlayers = 0;

    private void Start() {
        if (isServer) {
            totalPlayers = CustomNetworkManager.Instance.numPlayers;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdatePlayersReady(0);
    }

    public void ReadyUp() {
        CmdReadyUp(NetworkClient.localPlayer.gameObject.GetComponent<PlayerController>().playerName);
    }

    [Command(requiresAuthority = false)]
    public void CmdReadyUp(string playerNameReady) {
        readyPlayers.Add(playerNameReady);
        RpcUpdatePlayers(readyPlayers.Count);

        if (readyPlayers.Count == totalPlayers) {
            RpcDisableUI();
            onStartMinigame?.Invoke();
        }
    }

    [ClientRpc]
    private void RpcUpdatePlayers(int playersReady) {
        UpdatePlayersReady(playersReady);
    }

    private void UpdatePlayersReady(int playersReady) {
        playersReadyText.text = playersReady + " / " + totalPlayers + " Players Ready";
    }

    [ClientRpc]
    private void RpcDisableUI() {
        GetComponent<Canvas>().enabled = false;
        thumbnailCamera.enabled = false;
        if (!NetworkClient.localPlayer.GetComponent<PlayerController>().isPaused) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    [ClientRpc]
    public void RpcSetMovement(bool canMove) {
        NetworkClient.localPlayer.GetComponent<PlayerMovementComponent>().enabled = canMove;
        NetworkClient.localPlayer.GetComponent<PlayerController>().enabled = canMove;
        NetworkClient.localPlayer.GetComponent<ItemController>().enabled = canMove;
    }

    [ClientRpc]
    public void RpcSetPlayerController(bool canMove) {
        NetworkClient.localPlayer.GetComponent<PlayerController>().enabled = canMove;
    }

    public void DisconnectedPlayer(string playerNameDisconnected) {
        if (readyPlayers.Contains(playerNameDisconnected)) {
            readyPlayers.Remove(playerNameDisconnected);
        }
        totalPlayers--;

        RpcUpdatePlayers(readyPlayers.Count);

        if (readyPlayers.Count == totalPlayers) {
            RpcDisableUI();
            onStartMinigame?.Invoke();
        }
    }
}
