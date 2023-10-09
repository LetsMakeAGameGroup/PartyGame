using Mirror;
using UnityEngine;

public class MoveObjectOverTime : NetworkBehaviour {
    [Header("References")]
    [Tooltip("List of positions the object will move towards, even including the first position. Once the object is at the last position, it will loop back towards the first position and keep going.")]
    [SerializeField] private Vector3[] pathLocations = null;

    [Header("Settings")]
    [Tooltip("How fast the object will move.")]
    [SerializeField] private float moveSpeed = 1f;
    [Tooltip("The amount of seconds the object will stop moving for once reaching each position.")]
    [SerializeField] private float pauseDuration = 0f;
    [Tooltip("If this object should immediately start moving. If not, it will only start moving once the current minigame starts.")]
    public bool canMove = false;

    [HideInInspector] public int pathIndex = 0;

    private float pauseCurrentTime = 0;
    private float time = 0;

    private void Awake() {
        if (pathLocations.Length == 0) {
            Debug.LogError("PathLocations list on MoveObjectOverTime is empty. Destroying this MoveObjectOverTime as it's not being used.", gameObject);
            Destroy(this);
        }
    }

    private void FixedUpdate() {
        if (!canMove) {
            time = Time.time;
            return;
        }

        time = Time.time - time;

        if (pauseCurrentTime > 0) {
            pauseCurrentTime -= time;
            if (pauseCurrentTime >= 0) {
                time = Time.time;
                return;
            } else {
                time *= -1;
            }
        }

        if (Vector3.Distance(transform.position, pathLocations[pathIndex]) > 0.01f) {
            transform.position = Vector3.MoveTowards(transform.position, pathLocations[pathIndex], time * moveSpeed);
        } else {
            pauseCurrentTime = pauseDuration;

            transform.position = pathLocations[pathIndex];

            pathIndex++;
            pathIndex %= pathLocations.Length;
        }

        time = Time.time;
    }


    [ClientRpc]
    public void RpcStartMovement(Vector3 currentPosition, int index) {
        pathIndex = index;
        transform.position = currentPosition;

        canMove = true;
    }
}
