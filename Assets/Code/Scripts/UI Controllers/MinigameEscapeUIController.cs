using Mirror;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class MinigameEscapeUIController : NetworkBehaviour {
    [Header("UI References")]
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text disconnectButtonText;

    private Canvas canvas;

    private void Awake() {
        canvas = GetComponent<Canvas>();

        if (codeText != null && CustomNetworkManager.Instance != null && CustomNetworkManager.Instance.relayJoinCode != null) {
            codeText.text = CustomNetworkManager.Instance.relayJoinCode.ToUpper();
        }

        if (isServer) {
            disconnectButtonText.text = "Stop Server";
        }
    }

    public void TogglePlayerPause() {
        NetworkClient.localPlayer.GetComponent<PlayerController>().TogglePause();
    }

    public void ToggleMenu() {
        bool isPaused = NetworkClient.localPlayer.GetComponent<PlayerController>().isPaused;
        canvas.enabled = isPaused;

        if (!isPaused) {
            GameObject myEventSystem = GameObject.Find("EventSystem");
            myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        }
    }

    public void CopyLobbyCode() {
        GUIUtility.systemCopyBuffer = codeText.text;
    }

    public void DisconnectGame() {
        if (isServer) {
            CustomNetworkManager.Instance.StopHost();
        } else {
            CustomNetworkManager.Instance.StopClient();
        }
    }
}
