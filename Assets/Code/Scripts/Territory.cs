using UnityEngine;
using Mirror;

public class Territory : NetworkBehaviour {
    [HideInInspector] public WispWhiskManager wispWhiskManager;

    [Header("Settings")]
    [SerializeField] private int pointsToAdd = 50;

    private void OnTriggerEnter(Collider other) {
        if (!isServer || !other.CompareTag("Player")) return;

        if (other.GetComponent<WispEffect>() && other.GetComponent<WispEffect>().holdingWisp) {
            NetworkServer.Destroy(other.GetComponent<WispEffect>().holdingWisp);
            wispWhiskManager.playerPoints[other.gameObject] += pointsToAdd;
            wispWhiskManager.TargetSetScoreDisplay(other.GetComponent<NetworkIdentity>().connectionToClient, wispWhiskManager.playerPoints[other.gameObject]);
            StartCoroutine(wispWhiskManager.SpawnWisp());
            StartCoroutine(wispWhiskManager.SpawnTerritory());
            NetworkServer.Destroy(gameObject);
        }
    }
}
