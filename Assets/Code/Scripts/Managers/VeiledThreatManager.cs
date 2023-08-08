using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VeiledThreatManager : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private MinigameHandler minigameHandler;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject spectatorCamera;

    private GameObject currentBomb;
    private List<NetworkConnectionToClient> playerDeaths = new();

    [Header("Settings")]
    [Tooltip("The amount of seconds before the bomb explodes. Listed by the amount of players with 2 players being the last in the list. The amount listed should be one less than the max amount of players possible.")]
    [SerializeField] private float[] bombTimeIntervals = { 15f, 15f, 15f, 15f, 18f, 20f, 22f };

    private Dictionary<GameObject, int> activePlayerBombCarrierTime = new();

    private void FixedUpdate() {
        if (!isServer) return;

        if (currentBomb) {
            RpcSpectateBomb(currentBomb.transform.position);
        }
    }

    [ClientRpc]
    public void RpcSpectateBomb(Vector3 bombPos) {
        spectatorCamera.transform.position = bombPos + new Vector3(0, 30 - bombPos.y, 0);
    }

    // Called by server when the minigame starts.
    public void AssignInitialBombCarrier() {
        List<NetworkConnectionToClient> connections = new(CustomNetworkManager.Instance.connectionNames.Keys);
        foreach (NetworkConnectionToClient connection in connections) activePlayerBombCarrierTime.Add(connection.identity.gameObject, 0);

        StartCoroutine(AssignBombCarrier());
        StartCoroutine(AddBombCarrierTimer());
    }

    public IEnumerator AssignBombCarrier() {
        if (activePlayerBombCarrierTime.Count() == 1) {
            DetermineWinners();
            yield break;
        }

        int minBombCarrierTime = activePlayerBombCarrierTime.Min(kvp => kvp.Value);
        List<GameObject> minBombCarriers = new();
        foreach (var player in activePlayerBombCarrierTime) {
            if (player.Value == minBombCarrierTime) {
                minBombCarriers.Add(player.Key);
            }
        }

        GameObject bombCarrier = minBombCarriers[Random.Range(0, minBombCarriers.Count())];
        currentBomb = Instantiate(bombPrefab);
        NetworkServer.Spawn(currentBomb);
        bombCarrier.GetComponent<BombEffect>().RpcEquipBomb(currentBomb);

        float currentBombTimeInterval = bombTimeIntervals[8 - activePlayerBombCarrierTime.Count];
        minigameHandler.displayTimerUI.RpcStartCountdown(currentBombTimeInterval);
        yield return new WaitForSeconds(currentBombTimeInterval);

        activePlayerBombCarrierTime.Remove(currentBomb.transform.root.gameObject);
        playerDeaths.Add(currentBomb.transform.root.gameObject.GetComponent<NetworkIdentity>().connectionToClient);
        NetworkConnectionToClient bombCarrierConn = currentBomb.transform.root.gameObject.GetComponent<NetworkIdentity>().connectionToClient;
        NetworkServer.Destroy(currentBomb.transform.root.gameObject);
        TargetEnableSpectator(bombCarrierConn);

        StartCoroutine(AssignBombCarrier());
    }

    public IEnumerator AddBombCarrierTimer() {
        if (currentBomb && activePlayerBombCarrierTime.ContainsKey(currentBomb.transform.root.gameObject)) {
            activePlayerBombCarrierTime[currentBomb.transform.root.gameObject] += 1;
        }

        yield return new WaitForSeconds(0.25f);
        StartCoroutine(AddBombCarrierTimer());
    }

    [TargetRpc]
    public void TargetEnableSpectator(NetworkConnectionToClient conn) {
        spectatorCamera.GetComponent<Camera>().enabled = true;
        spectatorCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void DetermineWinners() {
        List<NetworkConnectionToClient> playerList = new();
        foreach (var player in activePlayerBombCarrierTime.Keys) {
            playerList.Add(player.GetComponent<NetworkIdentity>().connectionToClient);
        }
        minigameHandler.AddWinner(playerList);

        for (int i = playerDeaths.Count - 1; i >= 0; i--) {
            List<NetworkConnectionToClient> tempPlayerList = new() {
                playerDeaths[i]
            };
            minigameHandler.AddWinner(tempPlayerList);
        }
    }
}
