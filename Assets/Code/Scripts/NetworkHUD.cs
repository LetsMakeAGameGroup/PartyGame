using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkHUD : MonoBehaviour {
    [SerializeField] private CustomNetworkManager networkManager = null;

    [Header("UI")]
    [SerializeField] private TMP_InputField ipInputField = null;

    public void HostLobby() {
        networkManager.StartHost();
    }

    public void JoinLobby() {
        networkManager.networkAddress = ipInputField.text != "" ? ipInputField.text : "localhost";  // Default to localhost if IP inputfield is empty.
        networkManager.StartClient();
    }
}
