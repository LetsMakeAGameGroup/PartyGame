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
    public List<Transform> spawns;
    public static CustomNetworkManager Instance { get; private set; }

    public override void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public override void Start() {
        spawns = FindObjectsOfType<NetworkStartPosition>().Select(spawn => spawn.transform).ToList();
    }

    // Add new player to list and spawn them at the spawn points.
    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        // Prevent player from joining if the game has already started.
        if (GameManager.Instance.round > 0) {
            conn.Disconnect();
            return;
        }

        GameObject player = Instantiate(playerPrefab, spawns[numPlayers % spawns.Count].position, spawns[numPlayers % spawns.Count].rotation);
        player.GetComponent<PlayerController>().playerName = $"Player_{numPlayers + 1}";
        NetworkServer.AddPlayerForConnection(conn, player);
        GameManager.Instance.TargetSetCodeUI(conn, ConvertIPAddressToCode(GetExternalIpAddress()));
        players.Add(player);
    }


    // After loading a new scene, teleport the players to the scene's spawn points.
    public override void OnServerSceneChanged(string newSceneName) {
        spawns = FindObjectsOfType<NetworkStartPosition>().Select(spawn => spawn.transform).ToList();

        /*        for (int i = 0; i < players.Count; i++) {
                    GameManager.Instance.TargetTeleportPlayer(players[i].GetComponent<NetworkIdentity>().connectionToClient, spawns[numPlayers % spawns.Count]);
                }*/
        Time.timeScale = 0f;
        for (int i = 0; i < players.Count; i++) {
            if (newSceneName == "LobbyScene") GameManager.Instance.TargetSetCodeUI(players[i].GetComponent<NetworkIdentity>().connectionToClient, ConvertIPAddressToCode(GetExternalIpAddress()));
            //Debug.Log("Teleporting " + players[i].GetComponent<PlayerController>().playerName);
            GameManager.Instance.TargetTeleportPlayer(players[i].GetComponent<NetworkIdentity>().connectionToClient, spawns[numPlayers % spawns.Count].position, spawns[numPlayers % spawns.Count].rotation);
            while (players[i].GetComponent<NetworkIdentity>().isClientOnly && players[i].transform.position != spawns[numPlayers % spawns.Count].position) {
                //Debug.Log("Teleporting " + players[i].GetComponent<PlayerController>().playerName);
                GameManager.Instance.TargetTeleportPlayer(players[i].GetComponent<NetworkIdentity>().connectionToClient, spawns[numPlayers % spawns.Count].position, spawns[numPlayers % spawns.Count].rotation);
            }
        }
        Time.timeScale = 1f;
        //Debug.Log("Finished teleporting");
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