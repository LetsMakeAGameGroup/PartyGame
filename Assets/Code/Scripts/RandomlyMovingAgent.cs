using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkTransform))]
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
        SetSpeed(speed, speed);
    }

    public Vector3 RandomNavmeshLocation(float radius) {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        Vector3 finalPosition = Vector3.zero;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, 1)) {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    public IEnumerator MoveTowardsTrans() {
        Vector3 randomDest = RandomNavmeshLocation(destinationRadius);
        navMeshAgent.destination = randomDest;

        while (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(randomDest.x, randomDest.z)) > 0.001f) {
            yield return null;
        }

        StartCoroutine(MoveTowardsTrans());
    }

    private void SetSpeed(float oldSpeed, float newSpeed) {
        navMeshAgent.speed = newSpeed;
    }
}
