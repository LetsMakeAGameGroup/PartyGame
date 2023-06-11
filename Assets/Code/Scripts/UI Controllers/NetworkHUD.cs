using System;
using System.Collections;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkHUD : MonoBehaviour {
    [SerializeField] private CustomNetworkManager networkManager = null;

    private string playerName = "";

    [Header("UI")]
    [SerializeField] private TMP_InputField codeInputField = null;

    [SerializeField] private GameObject nameSelectPanel = null;
    [SerializeField] private TMP_InputField nameInputField = null;
    [SerializeField] private TextMeshProUGUI currentNameText = null;

    private void Start() {
        networkManager.UnityLogin();

        if (PlayerPrefs.HasKey("PlayerName")) {
            playerName = PlayerPrefs.GetString("PlayerName");
            currentNameText.text = $"Hello, {playerName}!";
            nameInputField.text = playerName;
        } else {
            nameSelectPanel.SetActive(true);
        }
    }

    public void HostLobby() {
        networkManager.StartRelayHost(networkManager.maxConnections);
        //Debug.Log("code: " + networkManager.ConvertIPAddressToCode(networkManager.GetExternalIpAddress()));
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
}
