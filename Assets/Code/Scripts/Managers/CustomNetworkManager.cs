using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using Utp;

public class CustomNetworkManager : RelayNetworkManager {
    [Header("Custom References")]
    public Text errorText;
    public NetworkHUD networkHUD;

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

        var constantRotations = FindObjectsOfType<ConstantRotation>();
        foreach (var constantRotation in constantRotations) {
            constantRotation.RpcStartRotation(constantRotation.transform.rotation);
        }

        var movingObjects = FindObjectsOfType<MoveObjectOverTime>();
        foreach (var movingObject in movingObjects) {
            movingObject.RpcStartMovement(movingObject.transform.position, movingObject.pathIndex);
        }

        initialSceneChange = false;
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

    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        ClientDatas.Remove(conn);

        MinigameStartScreenController minigameStartScreenController = FindFirstObjectByType<MinigameStartScreenController>();
        if (minigameStartScreenController != null) {
            minigameStartScreenController.DisconnectedPlayer(conn.identity.gameObject.GetComponent<PlayerController>().playerName);
        }

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
            if (errorText) {
                errorText.text = "Logged in successfully!";
            }

            var data = await CloudSaveService.Instance.Data.LoadAllAsync();
            var playerName = (data.ContainsKey("PlayerName") ? data["PlayerName"] : "");
            var playerColor = (data.ContainsKey("PlayerColor") ? data["PlayerColor"] : "");
            networkHUD.ApplyData(playerName, playerColor);
        } catch (Exception e) {
            isLoggedIn = false;
            Debug.Log(e);
            if (errorText) {
                errorText.text = "Failed to login to Authetication Services. Continuing to attempt...";
            }
            await Task.Delay(1000);
            UnityLogin();
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