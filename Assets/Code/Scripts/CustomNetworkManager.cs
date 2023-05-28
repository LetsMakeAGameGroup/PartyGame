using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System.Net;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections;

public class CustomNetworkManager : NetworkManager {
    public List<GameObject> players = new();
    public List<NetworkConnectionToClient> connections = new();

    private bool initialSceneChange = true;

    public static CustomNetworkManager Instance { get; private set; }

    public override void Awake() {
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
        player.GetComponent<PlayerController>().playerName = $"Player_{numPlayers + 1}";
        NetworkServer.AddPlayerForConnection(conn, player);
        GameManager.Instance.TargetSetCodeUI(conn, ConvertIPAddressToCode(GetExternalIpAddress()));
        players.Add(player);
        connections.Add(conn);
        initialSceneChange = false;
    }

    public override void ServerChangeScene(string newSceneName) {
/*        foreach (GameObject player in players) {
            if (player.GetComponent<NetworkIdentity>().connectionToClient != null) Debug.Log("True!!");
            else Debug.Log("False!!");
            connections.Add(player.GetComponent<NetworkIdentity>().connectionToClient);
            NetworkServer.DestroyPlayerForConnection(connections[connections.Count() - 1]);
        }*/
        base.ServerChangeScene(newSceneName);
    }

    // After loading a new scene, teleport the players to the scene's spawn points.
    // TODO: This needs proper syncing to account for clients still loading scenes. AKA loading screen needed.
    public override void OnServerSceneChanged(string newSceneName) {
        if (initialSceneChange) return;

        SpawnHolder spawnHolder = FindObjectOfType<SpawnHolder>();

        for (int i = 0; i < connections.Count(); i++) {
            GameObject player = Instantiate(playerPrefab, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Length].transform.position, spawnHolder.currentSpawns[numPlayers % spawnHolder.currentSpawns.Length].transform.rotation);
            player.GetComponent<PlayerController>().playerName = $"Player_{i + 1}";

            NetworkServer.ReplacePlayerForConnection(connections[i], player);
        }
    }

    public string ConvertIPAddressToCode(string ipAddress) {
        IPAddress address = IPAddress.Parse(ipAddress);
        byte[] bytes = address.GetAddressBytes();
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

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

    public string GetExternalIpAddress() {
        string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        var externalIp = IPAddress.Parse(externalIpString);

        return externalIp.ToString();
    }
}