using UnityEngine;
using Mirror;

public class Tornado : NetworkBehaviour {
    [Header("Settings")]
    [Tooltip("The amount of force this will knock the hit entity backwards.")]
    [SerializeField] private float knockbackForce = 25f;
    [Tooltip("The amount of force this will knock the hit entity upwards.")]
    [SerializeField] private float verticalForce = 5f;
    [Tooltip("How many seconds the hit entity will be stunned for.")]
    [SerializeField] private float stunTime = 0.5f;

    private void OnTriggerEnter(Collider other) {
        if (!isServer || !other.CompareTag("Player")) return;

        if (other.gameObject.TryGetComponent(out PlayerMovementComponent playerMovementComponent)) {
            Vector3 direction = (other.transform.position - transform.position).normalized;
            direction.x *= knockbackForce;
            direction.y = verticalForce;
            direction.z *= knockbackForce;
            playerMovementComponent.TargetKnockbackCharacter(direction);
            StartCoroutine(playerMovementComponent.StunPlayer(stunTime));
        }
    }
}
