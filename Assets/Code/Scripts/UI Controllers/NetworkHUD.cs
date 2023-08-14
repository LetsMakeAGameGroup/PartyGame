using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        networkManager.StartRelayHost(networkManager.maxConnections);
    }

    public void JoinLobby() {
        networkManager.relayJoinCode = codeInputField.text != "" ? codeInputField.text : "localhost";  // Default to localhost if code inputfield is empty.
        networkManager.JoinRelayServer();
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
