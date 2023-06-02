using System;
using System.Collections;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkHUD : MonoBehaviour {
    [SerializeField] private CustomNetworkManager networkManager = null;

    [Header("UI")]
    [SerializeField] private TMP_InputField codeInputField = null;

    private void Start() => networkManager.UnityLogin();

    public void HostLobby() {
        networkManager.StartRelayHost(networkManager.maxConnections);
        //Debug.Log("code: " + networkManager.ConvertIPAddressToCode(networkManager.GetExternalIpAddress()));
    }

    public void JoinLobby() {
        networkManager.relayJoinCode = codeInputField.text != "" ? codeInputField.text : "localhost";  // Default to localhost if code inputfield is empty.
        networkManager.JoinRelayServer();
    }
}
