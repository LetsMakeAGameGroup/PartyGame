using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System.Net;
using System;
using Utp;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class CustomNetworkManager : RelayNetworkManager {
    public List<GameObject> players = new();
    public Dictionary<NetworkConnectionToClient, int> connectionScores = new();
    public Dictionary<NetworkConnectionToClient, string> connectionNames = new();
    public Dictionary<NetworkConnectionToClient, string> connectionColors = new();

    public Dictionary<string, Color> colorOptions = new() {{"Red", Color.red}, {"Blue", Color.blue}, {"Green", Color.green}, {"Pink", new Color(1f, 0.6f, 1f)}, {"Orange", new Color(1f, 0.6f, 0f)}, {"Yellow", Color.yellow}, {"Purple", new Color(0.6f, 0f, 0.6f)}, {"Cyan", Color.cyan}};

    private bool initialSceneChange = true;

    /// <summary>Flag to determine if the user is logged into the backend.</summary>
    public bool isLoggedIn = false;

    public static CustomNetworkManager Instance { get; private set; }

    public override void Awake() {
        base.Awake();

        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Add new player to list and spawn them at the spawn points.
    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        // Prevent player from joining if the game has already started.
        if (GameManager.Instance.round > 0) {
            conn.Disconnect();
            return;
        }

        SpawnHolder spawnHolder = FindObjectOfType<SpawnHolder>();

        GameObject player = Instantiate(playerPrefab, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Length].transform.position, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Length].transform.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        players.Add(player);
        connectionScores.Add(conn, 0);
        player.GetComponent<PlayerController>().TargetGetDisplayName();
        foreach (var colorOption in colorOptions) {
            if (!connectionColors.ContainsValue(colorOption.Key)) {
                connectionColors.Add(conn, colorOption.Key);
                player.GetComponent<PlayerController>().playerColor = colorOption.Key;
                break;
            }
        }
        initialSceneChange = false;
    }

    // After loading a new scene, teleport the players to the scene's spawn points.
    // TODO: This needs proper syncing to account for clients still loading scenes. AKA loading screen needed.
    public override void OnServerSceneChanged(string newSceneName) {
        if (initialSceneChange) return;

        SpawnHolder spawnHolder = FindObjectOfType<SpawnHolder>();

        foreach (NetworkConnectionToClient conn in connectionScores.Keys) {
            GameObject player = Instantiate((spawnHolder.playerPrefab != null ? spawnHolder.playerPrefab : playerPrefab), spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Length].transform.position, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Length].transform.rotation);
            player.GetComponent<PlayerController>().playerName = connectionNames[conn];
            player.GetComponent<PlayerController>().playerColor = connectionColors[conn];

            if (!NetworkClient.ready) NetworkClient.Ready();
            // TODO: Fix "There is already a player for this connection." error here. This is most likely because the connected clients haven't switched scenes yet.
            NetworkServer.ReplacePlayerForConnection(conn, player);
        }
    }

    /// <summary>Converts an IP address to bytes, then returns it as a string that can be used as a code to join the server.</summary>
    public string ConvertIPAddressToCode(string ipAddress) {
        IPAddress address = IPAddress.Parse(ipAddress);
        byte[] bytes = address.GetAddressBytes();
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

    /// <summary>Converts a code(converted IP address) into an array of bytes to return the IP address.</summary>
    public string ConvertCodeToIPAddress(string code) {
        string[] tempArray = new string[code.Length / 2];
        for (int i = 0; i < tempArray.Length; i++) {
            tempArray[i] = code[i*2].ToString() + code[i*2+1];
        }

        byte[] bytes = new byte[tempArray.Length];
        for (int i = 0; i < tempArray.Length; i++) {
            bytes[i] = Convert.ToByte(tempArray[i], 16);
        }

        return String.Join(".", bytes);
    }

    /// <summary>Uses a website's JSON string to find the host's public ip.</summary>
    public string GetExternalIpAddress() {
        string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        var externalIp = IPAddress.Parse(externalIpString);

        return externalIp.ToString();
    }

    public async void UnityLogin() {
        try {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Logged into Unity, player ID: " + AuthenticationService.Instance.PlayerId);
            isLoggedIn = true;
        } catch (Exception e) {
            isLoggedIn = false;
            Debug.Log(e);
        }
    }
}