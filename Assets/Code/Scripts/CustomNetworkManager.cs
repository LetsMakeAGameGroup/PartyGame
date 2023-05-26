using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System.Net;
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class CustomNetworkManager : NetworkManager {
    public List<GameObject> players;

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
    }


    // After loading a new scene, teleport the players to the scene's spawn points.
    public override void OnServerSceneChanged(string newSceneName) {
        SpawnHolder spawnHolder = FindObjectOfType<SpawnHolder>();

        for (int i = 0; i < players.Count; i++) {
            if (newSceneName == "LobbyScene") GameManager.Instance.TargetSetCodeUI(players[i].GetComponent<NetworkIdentity>().connectionToClient, ConvertIPAddressToCode(GetExternalIpAddress()));
            players[i].GetComponent<PlayerController>().TargetTeleport(spawnHolder.currentSpawns[i % spawnHolder.currentSpawns.Length].transform.position, spawnHolder.currentSpawns[i % spawnHolder.currentSpawns.Length].transform.rotation);
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