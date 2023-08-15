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
        FindObjectOfType<LobbyUIController>().codeText.text = CustomNetworkManager.Instance.relayJoinCode.ToUpper();

        if (isServer) {
            startButton.interactable = true;
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            escapeMenu.SetActive(NetworkClient.localPlayer.GetComponent<PlayerMovementComponent>().CanMove);
        }
    }

    public void ToggleMenu() {
        escapeMenu.SetActive(NetworkClient.localPlayer.GetComponent<PlayerMovementComponent>().CanMove);
        NetworkClient.localPlayer.GetComponent<PlayerController>().TogglePause();
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
