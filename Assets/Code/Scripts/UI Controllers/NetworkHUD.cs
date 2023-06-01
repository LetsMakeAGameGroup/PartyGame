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

    public void HostLobby() {
        networkManager.StartHost();
        Debug.Log("code: " + networkManager.ConvertIPAddressToCode(networkManager.GetExternalIpAddress()));
    }

    public void JoinLobby() {
        networkManager.networkAddress = codeInputField.text != "" ? networkManager.ConvertCodeToIPAddress(codeInputField.text) : "localhost";  // Default to localhost if code inputfield is empty.
        networkManager.StartClient();
    }
}
