using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

public class VeiledThreatManager : NetworkBehaviour {
    [SerializeField] private MinigameHandler minigameHandler = null;
    [SerializeField] private GameObject bombPrefab = null;

    // Called by server.
    public void AssignBombCarrier() {
        List<NetworkConnectionToClient> connections = new List<NetworkConnectionToClient>(CustomNetworkManager.Instance.connectionNames.Keys);
        if (connections[0].identity.gameObject) {
            GameObject bomb = Instantiate(bombPrefab);
            NetworkServer.Spawn(bomb);
            connections[0].identity.GetComponent<BombEffect>().RpcEquipBomb(bomb);
        } else {
            Debug.Log("Cannot equip bomb on player.", transform);
        }
    }
}
