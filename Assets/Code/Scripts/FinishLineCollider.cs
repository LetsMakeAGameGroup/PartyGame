using System.Collections.Generic;
using UnityEngine;

public class FinishLineCollider : MonoBehaviour {
    private MinigameHandler minigameHandler;

    private void Start() {
        minigameHandler = FindObjectOfType<MinigameHandler>();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        List<GameObject> player = new() {
            other.gameObject
        };
        minigameHandler.AddWinner(player);
    }
}
