using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using Utp;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class CustomNetworkManager : RelayNetworkManager {
    // TODO: Setup a struct for clients to handle this data.
    [HideInInspector] public List<GameObject> players = new();
    [HideInInspector] public Dictionary<NetworkConnectionToClient, int> connectionScores = new();
    [HideInInspector] public Dictionary<NetworkConnectionToClient, string> connectionNames = new();
    [HideInInspector] public Dictionary<NetworkConnectionToClient, string> connectionColors = new();

    /// <summary>Prevents OnServerSceneChanged from being called when the server first goes online.</summary>
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

        GameObject player = Instantiate(playerPrefab, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Count()].transform.position, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Count()].transform.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        players.Add(player);
        connectionScores.Add(conn, 0);
        player.GetComponent<PlayerController>().TargetGetDisplayName();
        player.GetComponent<PlayerController>().TargetGetPlayerColorPref();
        initialSceneChange = false;
    }

    // After loading a new scene, teleport the players to the scene's spawn points.
    // TODO: This needs proper syncing to account for clients still loading scenes. AKA loading screen needed.
    public override void OnServerSceneChanged(string newSceneName) {
        if (initialSceneChange) return;

        players.Clear();

        SpawnHolder spawnHolder = FindObjectOfType<SpawnHolder>();

        foreach (NetworkConnectionToClient conn in connectionScores.Keys) {
            GameObject player = Instantiate((spawnHolder.playerPrefab != null ? spawnHolder.playerPrefab : playerPrefab), spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Count()].transform.position, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Count()].transform.rotation);
            player.GetComponent<PlayerController>().playerName = connectionNames[conn];
            player.GetComponent<PlayerController>().playerColor = connectionColors[conn];

            if (!NetworkClient.ready) NetworkClient.Ready();
            // TODO: Fix "There is already a player for this connection." error here. This is most likely because the connected clients haven't switched scenes yet.
            NetworkServer.ReplacePlayerForConnection(conn, player);

            players.Add(player);
        }
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

    public void DeterminePlayerColor(GameObject player, string playerColor) {
        if (!connectionColors.ContainsValue(playerColor)) {
            connectionColors.Add(player.GetComponent<NetworkIdentity>().connectionToClient, playerColor);
            player.GetComponent<PlayerController>().playerColor = playerColor;
        } else {
            foreach (var colorOption in PlayerColorOptions.options) {
                if (!connectionColors.ContainsValue(colorOption.Key)) {
                    connectionColors.Add(player.GetComponent<NetworkIdentity>().connectionToClient, colorOption.Key);
                    player.GetComponent<PlayerController>().playerColor = colorOption.Key;
                    break;
                }
            }
        }
    }

    public void DeterminePlayerName(GameObject player, string displayName) {
        if (!connectionNames.ContainsValue(displayName)) {
            connectionNames.Add(player.GetComponent<NetworkIdentity>().connectionToClient, displayName);
            player.GetComponent<PlayerController>().playerName = displayName;
        } else {
            for (int i = 2; i < 9; i++) {
                string newName = displayName + " (" + i.ToString() + ")";
                if (!connectionNames.ContainsValue(newName)) {
                    connectionNames.Add(player.GetComponent<NetworkIdentity>().connectionToClient, newName);
                    player.GetComponent<PlayerController>().playerName = newName;
                    return;
                }
            }
        }
    }
}