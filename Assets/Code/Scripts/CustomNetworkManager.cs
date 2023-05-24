using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

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
        GameObject player = Instantiate(playerPrefab, FindObjectOfType<NetworkStartPosition>().transform.position, Quaternion.identity);
        player.GetComponent<PlayerController>().playerName = $"Player_{numPlayers + 1}";
        NetworkServer.AddPlayerForConnection(conn, player);
        players.Add(player);
        if (players.Count == GameManager.Instance.minPlayers) {
            GameManager.Instance.StartNextRound();
        }
    }


    // After loading a new scene, teleport the players to the scene's spawn points.
    public override void OnServerSceneChanged(string newSceneName) {
        List<Vector3> spawns = FindObjectsOfType<NetworkStartPosition>().Select(spawn => spawn.transform.position).ToList();

        for (int i = 0; i < players.Count; i++) {
            GameManager.Instance.TargetTeleportPlayer(players[i].GetComponent<NetworkIdentity>().connectionToClient, spawns[i]);
        }
    }
}