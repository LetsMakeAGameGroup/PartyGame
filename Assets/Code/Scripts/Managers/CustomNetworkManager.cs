using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using Utp;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class CustomNetworkManager : RelayNetworkManager {
    public Dictionary<NetworkConnectionToClient, ClientData> ClientDatas { get; private set; }

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

    public override void OnStartHost() {
        ClientDatas = new Dictionary<NetworkConnectionToClient, ClientData>();
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

        ClientDatas[conn] = new ClientData(conn.connectionId);

        player.GetComponent<PlayerController>().TargetGetDisplayName();
        player.GetComponent<PlayerController>().TargetGetPlayerColorPref();
        initialSceneChange = false;

        Debug.Log($"numPlayers: {numPlayers}");
        Debug.Log($"ClientDatas.Count: {ClientDatas.Count}");
    }

    // After loading a new scene, teleport the players to the scene's spawn points.
    // TODO: This needs proper syncing to account for clients still loading scenes. AKA loading screen needed.
    public override void OnServerSceneChanged(string newSceneName) {
        if (initialSceneChange) return;

        SpawnHolder spawnHolder = FindObjectOfType<SpawnHolder>();

        foreach (NetworkConnectionToClient conn in ClientDatas.Keys) {
            GameObject player = Instantiate((spawnHolder.playerPrefab != null ? spawnHolder.playerPrefab : playerPrefab), spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Count()].transform.position, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Count()].transform.rotation);
            player.GetComponent<PlayerController>().playerName = ClientDatas[conn].displayName;
            player.GetComponent<PlayerController>().playerColor = ClientDatas[conn].playerColor;

            if (!NetworkClient.ready) NetworkClient.Ready();
            // TODO: Fix "There is already a player for this connection." error here. This is most likely because the connected clients haven't switched scenes yet.
            NetworkServer.ReplacePlayerForConnection(conn, player);
        }
    }

    // Add new player to list and spawn them at the spawn points.
    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        ClientDatas.Remove(conn);

        base.OnServerDisconnect(conn);
    }

    public async void UnityLogin() {
        try {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn) {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Logged into Unity, player ID: " + AuthenticationService.Instance.PlayerId);
            isLoggedIn = true;
        } catch (Exception e) {
            isLoggedIn = false;
            Debug.Log(e);
        }
    }

    private bool ColorAlreadyChosen(string playerColor) {
        foreach (var clientData in ClientDatas.Values) {
            if (clientData.playerColor == playerColor) return true;
        }

        return false;
    }

    public void DeterminePlayerColor(NetworkConnectionToClient conn, string playerColor) {
        if (!ColorAlreadyChosen(playerColor)) {
            ClientDatas[conn].playerColor = playerColor;
            conn.identity.GetComponent<PlayerController>().playerColor = playerColor;
        } else {
            foreach (var colorOption in PlayerColorOptions.options) {
                if (!ColorAlreadyChosen(colorOption.Key)) {
                    ClientDatas[conn].playerColor = colorOption.Key;
                    conn.identity.GetComponent<PlayerController>().playerColor = colorOption.Key;
                    break;
                }
            }
        }
    }

    private bool NameAlreadyChosen(string displayName) {
        foreach (var clientData in ClientDatas.Values) {
            if (clientData.displayName == displayName) return true;
        }

        return false;
    }

    public void DeterminePlayerName(NetworkConnectionToClient conn, string displayName) {
        if (!NameAlreadyChosen(displayName)) {
            ClientDatas[conn].displayName = displayName;
            conn.identity.GetComponent<PlayerController>().playerName = displayName;
        } else {
            for (int i = 2; i < 9; i++) {
                string newName = displayName + " (" + i.ToString() + ")";
                if (!NameAlreadyChosen(newName)) {
                    ClientDatas[conn].displayName = newName;
                    conn.identity.GetComponent<PlayerController>().playerName = newName;
                    return;
                }
            }
        }
    }
}