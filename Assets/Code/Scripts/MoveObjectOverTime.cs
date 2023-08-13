using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
public class MoveObjectOverTime : NetworkBehaviour {
    [Header("References")]
    [Tooltip("List of positions the object will move towards, even including the first position. Once the object is at the last position, it will loop back towards the first position and keep going.")]
    [SerializeField] private Vector3[] pathLocations = null;

    [Header("Settings")]
    [Tooltip("How fast the object will move.")]
    [SerializeField] private float moveSpeed = 1f;

    private int pathIndex = 0;

    private void Update() {
        if (!isServer) return;

        if (Vector3.Distance(gameObject.transform.position, pathLocations[pathIndex]) > 0.01f) {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathLocations[pathIndex], Time.deltaTime * moveSpeed);
        } else {
            gameObject.transform.position = pathLocations[pathIndex];

            pathIndex++;
            pathIndex %= pathLocations.Length;
        }
    }
}
