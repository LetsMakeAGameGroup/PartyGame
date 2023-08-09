using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
public class MoveObjectOverTime : NetworkBehaviour {
    [SerializeField] private float moveSpeed = 1f;

    [SerializeField] private Vector3[] pathLocations = null;
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
