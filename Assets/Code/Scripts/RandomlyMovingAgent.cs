using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomlyMovingAgent : NetworkBehaviour {
    private NavMeshAgent navMeshAgent;

    [Header("Settings")]
    [Tooltip("The initial speed this will move. When the speed is increased mid game, this variable will represent that.")]
    [SyncVar(hook = nameof(SetSpeed))] public float speed = 1f;
    [Tooltip("The radius from the center of the scene that this can navigate towards.")]
    [SerializeField] private float destinationRadius = 35f;

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
    }

    public Vector3 RandomNavmeshLocation() {
        Vector3 randomDirection = Random.insideUnitSphere * destinationRadius;
        Vector3 finalPosition = Vector3.zero;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, destinationRadius, 1)) {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    [ClientRpc]
    public void RpcSetDestination(Vector3 currentPosition, Vector3 destination) {
        StartCoroutine(MoveTowardsPosition(currentPosition, destination));
    }

    public IEnumerator MoveTowardsPosition(Vector3 currentPosition, Vector3 destination) {
        transform.position = currentPosition;
        navMeshAgent.destination = destination;

        while (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destination.x, destination.z)) > 0.001f && navMeshAgent.CalculatePath(destination, navMeshAgent.path) && navMeshAgent.path.status == NavMeshPathStatus.PathComplete) {
                yield return null;
        }

        if (isServer) {
            RpcSetDestination(transform.position, RandomNavmeshLocation());
        }
    }

    private void SetSpeed(float oldSpeed, float newSpeed) {
        navMeshAgent.speed = newSpeed;
    }
}
