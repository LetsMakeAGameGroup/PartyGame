using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainPlayersInsideCollider : MonoBehaviour {
    [HideInInspector] public List<GameObject> playersInside = new();
    public float pointsMultiplier = 0.5f;  // 0.5 indicates a 50% increase

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        playersInside.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;

        playersInside.Remove(other.gameObject);
    }
}
