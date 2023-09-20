using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIController : NetworkBehaviour {
    [Header("References")]
    public TMP_Text codeText;
    [SerializeField] private GameObject escapeMenu;
    [SerializeField] private GameObject startButtonObject;
    [SerializeField] private TMP_Text disconnectText;

    private void Start() {
        if (codeText != null && CustomNetworkManager.Instance.relayJoinCode != null) {
            codeText.text = CustomNetworkManager.Instance.relayJoinCode.ToUpper();
        }

        if (isServer) {
            startButtonObject.SetActive(true);
            disconnectText.text = "Stop Server";
        }
    }

    public void TogglePlayerPause() {
        NetworkClient.localPlayer.GetComponent<PlayerController>().TogglePause();
    }

    public void ToggleMenu() {
        escapeMenu.SetActive(NetworkClient.localPlayer.GetComponent<PlayerController>().isPaused);
    }

    public void CopyLobbyCode() {
        GUIUtility.systemCopyBuffer = CustomNetworkManager.Instance.relayJoinCode.ToUpper();
    }

    public void StartGame() {
        if (GameManager.Instance.round == 0) {
            GameManager.Instance.StartNextRound();
        }
    }

    public void DisconnectLobby() {
        if (isServer) {
            CustomNetworkManager.Instance.StopHost();
        } else {
            CustomNetworkManager.Instance.StopClient();
        }
    }
}
