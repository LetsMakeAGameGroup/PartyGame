using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utp;

public class NetworkHUD : MonoBehaviour {
    [Header("References")]
    [SerializeField] private CustomNetworkManager networkManager;

    [SerializeField] private TMP_InputField codeInputField = null;

    [SerializeField] private GameObject nameSelectPanel = null;
    [SerializeField] private TMP_InputField nameInputField = null;
    [SerializeField] private TextMeshProUGUI currentNameText = null;

    [SerializeField] private GameObject colorSelectPanel = null;
    [SerializeField] private TextMeshProUGUI currentColorText = null;
    [SerializeField] private Button[] colorOptionButtons = null;

    [SerializeField] private Text errorText;

    private string playerName = "";
    private string playerColor = "";

    private void Awake() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int optionIndex = 0;
        foreach (var option in PlayerColorOptions.options) {
            colorOptionButtons[optionIndex].onClick.AddListener(delegate { SetColorPref(option.Key); });
            colorOptionButtons[optionIndex].GetComponentInChildren<TextMeshProUGUI>().text = option.Key;
            colorOptionButtons[optionIndex].GetComponentInChildren<TextMeshProUGUI>().color = option.Value;
            colorOptionButtons[optionIndex].interactable = true;
            optionIndex++;
        }
    }

    private void Start() {
        networkManager.UnityLogin();

        if (PlayerPrefs.HasKey("PlayerName")) {
            playerName = PlayerPrefs.GetString("PlayerName");
            currentNameText.text = $"Hello, {playerName}!";
            nameInputField.text = playerName;
        } else {
            nameSelectPanel.SetActive(true);
        }

        if (PlayerPrefs.HasKey("PlayerColor")) {
            playerColor = PlayerPrefs.GetString("PlayerColor");
            currentColorText.text = playerColor;
            currentColorText.color = PlayerColorOptions.options[playerColor];
        } else {
            colorSelectPanel.SetActive(true);
        }
    }

    public void HostLobby() {
        if (!networkManager.isLoggedIn) return;

        networkManager.GetComponent<UtpTransport>().useRelay = true;
        networkManager.GetComponent<UtpTransport>().AllocateRelayServer(networkManager.maxConnections, null,
        (string joinCode) => {
            networkManager.relayJoinCode = joinCode;

            networkManager.StartHost();
        },
        () => {
            UtpLog.Error($"Failed to start a Relay host.");
            errorText.text = "Failed to start a server. Try restarting the game.";
        });
    }

    public void JoinLobby() {
        if (!networkManager.isLoggedIn) return;

        if (codeInputField.text == string.Empty) {
            errorText.text = "Enter a lobby code before attempting to join.";
            return;
        }

        networkManager.GetComponent<UtpTransport>().useRelay = true;
        networkManager.GetComponent<UtpTransport>().ConfigureClientWithJoinCode(codeInputField.text,
        () => {
            networkManager.StartClient();
        },
        () => {
            UtpLog.Error($"Failed to join Relay server.");
            errorText.text = "Failed to find/join server. Is the lobby code correct?";
        });
    }

    public void ChangeName() {
        playerName = nameInputField.text;
        currentNameText.text = $"Hello, {playerName}!";
        nameSelectPanel.SetActive(false);
        PlayerPrefs.SetString("PlayerName", playerName);
    }

    public void SetColorPref(string colorName) {
        playerColor = colorName;
        currentColorText.text = playerColor;
        currentColorText.color = PlayerColorOptions.options[playerColor];
        colorSelectPanel.SetActive(false);
        PlayerPrefs.SetString("PlayerColor", colorName);
    }
}
