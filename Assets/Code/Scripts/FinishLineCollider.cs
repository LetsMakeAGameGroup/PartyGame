using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class FinishLineCollider : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        List<NetworkConnectionToClient> player = new() {
            other.gameObject.GetComponent<NetworkIdentity>().connectionToClient
        };
        MinigameHandler.Instance.AddWinner(player);
    }
}
