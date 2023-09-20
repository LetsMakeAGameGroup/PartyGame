using kcp2k;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;
using Utp;

public class NetworkHUD : MonoBehaviour {
    [Header("References")]
    [SerializeField] private CustomNetworkManager networkManager;

    [SerializeField] private TMP_InputField codeInputField = null;

    [SerializeField] private GameObject mainMenuPanel = null;

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

            if (PlayerPrefs.HasKey("PlayerColor")) {
                playerColor = PlayerPrefs.GetString("PlayerColor");
                currentColorText.text = playerColor;
                currentColorText.color = PlayerColorOptions.options[playerColor];

                mainMenuPanel.SetActive(true);

                UpdateNameOnLaunch();
            } else {
                colorSelectPanel.SetActive(true);
            }
        } else {
            nameSelectPanel.SetActive(true);
        }
    }

    private async void UpdateNameOnLaunch() {
        try {
            if (await AuthenticationService.Instance.GetPlayerNameAsync() == null) {
                SaveNameData();
                SaveColorData();
            }
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public void HostLobby() {
        if (!networkManager.isLoggedIn) return;

        networkManager.GetComponent<UtpTransport>().useRelay = true;
        networkManager.GetComponent<UtpTransport>().AllocateRelayServer(networkManager.maxConnections, null,
        (string joinCode) => {
            networkManager.relayJoinCode = joinCode;

            networkManager.StartHost();

            networkManager.transport = networkManager.GetComponent<KcpTransport>();
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

        networkManager.relayJoinCode = codeInputField.text;
        networkManager.GetComponent<UtpTransport>().useRelay = true;
        networkManager.GetComponent<UtpTransport>().ConfigureClientWithJoinCode(codeInputField.text,
        () => {
            networkManager.StartClient();

            networkManager.transport = networkManager.GetComponent<KcpTransport>();
        },
        () => {
            UtpLog.Error($"Failed to join Relay server with code: {codeInputField.text}.");
            errorText.text = "Failed to find/join server. Is the lobby code correct?";
        });
    }

    public void ChangeName() {
        playerName = nameInputField.text;
        if (playerName != PlayerPrefs.GetString("PlayerName")) {
            SaveNameData();
        }

        currentNameText.text = $"Hello, {playerName}!";
        nameSelectPanel.SetActive(false);
        ;
        if (PlayerPrefs.HasKey("PlayerColor")) {
            mainMenuPanel.SetActive(true);
        } else {
            colorSelectPanel.SetActive(true);
        }
    }

    private async void SaveNameData() {
        try {
            PlayerPrefs.SetString("PlayerName", playerName);

            string cloudPlayerName = await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);

            var data = new Dictionary<string, object> { { "PlayerName", cloudPlayerName } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public void SetColorPref(string colorName) {
        playerColor = colorName;
        if (playerColor != PlayerPrefs.GetString("PlayerColor")) {
            SaveColorData();
        }

        currentColorText.text = playerColor;
        currentColorText.color = PlayerColorOptions.options[playerColor];
        colorSelectPanel.SetActive(false);

        mainMenuPanel.SetActive(true);
    }

    private async void SaveColorData() {
        try {
            PlayerPrefs.SetString("PlayerColor", playerColor);

            var data = new Dictionary<string, object> { { "PlayerColor", playerColor } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }
}
