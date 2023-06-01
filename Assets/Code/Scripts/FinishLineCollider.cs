using UnityEngine;

public class FinishLineCollider : MonoBehaviour {
    private MinigameHandler minigameHandler;

    private void Start() {
        minigameHandler = FindObjectOfType<MinigameHandler>();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        minigameHandler.AddWinner(other.gameObject);
    }
}
