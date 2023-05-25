using UnityEngine;

public class FinishLineCollider : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        FindObjectOfType<MinigameHandler>().AddWinner(other.gameObject);
    }
}
