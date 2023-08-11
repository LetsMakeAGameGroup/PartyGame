using System.Collections.Generic;
using UnityEngine;

public class ContainPlayersInsideCollider : MonoBehaviour {
    [Header("Settings")]
    [Tooltip("What percentage bonus of points should be applied when inside this collider. For example: 0.75f indicates a 75% increase in points given.")]
    [Range(0f, 1f)] public float pointsMultiplier = 0.5f;

    [HideInInspector] public List<GameObject> playersInside = new();

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        playersInside.Add(other.gameObject);

        if (other.gameObject.TryGetComponent(out GlowDisplay glowDisplay)) {
            glowDisplay.TargetToggleCanvas(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;

        playersInside.Remove(other.gameObject);

        if (other.gameObject.TryGetComponent(out GlowDisplay glowDisplay)) {
            glowDisplay.TargetToggleCanvas(false);
        }
    }
}
