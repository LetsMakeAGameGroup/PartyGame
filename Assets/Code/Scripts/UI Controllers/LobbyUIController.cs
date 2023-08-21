using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIController : NetworkBehaviour {
    [Header("References")]
    public TextMeshProUGUI codeText;
    [SerializeField] private GameObject escapeMenu;
    [SerializeField] private Button startButton;

    private void Start() {
        if (codeText.text != null && CustomNetworkManager.Instance.relayJoinCode != null) {
            codeText.text = CustomNetworkManager.Instance.relayJoinCode.ToUpper();
		}

        if (isServer) {
            startButton.interactable = true;
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
}
