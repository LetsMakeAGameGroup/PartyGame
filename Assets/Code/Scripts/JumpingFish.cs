using Mirror;
using System.Collections;
using UnityEngine;

public class JumpingFish : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private GameObject fishModel;

    [Header("Settings")]
    [Tooltip("The minumum amount of seconds before the fish randomly jumps again after entering the water.")]
    [SerializeField] private float minRandomJumpTime;
    [Tooltip("The maximum amount of seconds before the fish randomly jumps again after entering the water.")]
    [SerializeField] private float maxRandomJumpTime;
    [Tooltip("How far the fish will jump.")]
    [SerializeField] private float jumpEndDistance;
    [Tooltip("How high the fish will jump.")]
    [SerializeField] private float jumpHeightDistance;
    [Tooltip("How fast the fish will travel while jumping.")]
    [SerializeField] private float jumpSpeed;
    [Tooltip("The amount of force this will knock the hit entity backwards.")]
    [SerializeField] private float knockbackForce = 25f;
    [Tooltip("The amount of force this will knock the hit entity upwards.")]
    [SerializeField] private float verticalForce = 5f;
    [Tooltip("How many seconds the hit entity will be stunned for.")]
    [SerializeField] private float stunTime = 0.5f;

    private Vector3 startArcPos;
    private Vector3 endArcPos;
    private Vector3 midArcPos;

    private void Start() {
        if (!isServer) return;

        RpcToggleFishActive(false);

        SetupArcPositions();

        StartCoroutine(FishJump());
    }

    private void OnTriggerEnter(Collider other) {
        if (!isServer || !other.CompareTag("Player")) return;

        if (other.gameObject.TryGetComponent(out PlayerMovementComponent playerMovementComponent)) {
            Vector3 direction = transform.forward * knockbackForce;
            direction.y = verticalForce;
            playerMovementComponent.TargetKnockbackCharacter(direction, stunTime);
        }
    }

    private void SetupArcPositions() {
        startArcPos = transform.position;
        endArcPos = transform.position + transform.forward * jumpEndDistance;
        midArcPos = Vector3.Lerp(startArcPos, endArcPos, 0.5f);
        midArcPos.y += jumpHeightDistance;
    }

    private Vector3 GetArcCoordinates(float time) {
        return Mathf.Pow(1-time, 2)*startArcPos + 2*time*(1-time)*midArcPos + Mathf.Pow(time, 2)*endArcPos;

    }

    private IEnumerator FishJump() {
        yield return new WaitForSeconds(Random.Range(minRandomJumpTime, maxRandomJumpTime));

        RpcToggleFishActive(true);

        float currentJumpDuration = 0;
        float endJumpDuration = jumpEndDistance * jumpHeightDistance * jumpSpeed;
        while (currentJumpDuration < endJumpDuration) {
            float timeRatio = currentJumpDuration / endJumpDuration;
            transform.position = GetArcCoordinates(timeRatio);
            transform.LookAt(GetArcCoordinates(timeRatio + 0.1f));

            currentJumpDuration += Time.deltaTime;
            yield return null;
        }

        RpcToggleFishActive(false);

        StartCoroutine(FishJump());
    }

    [ClientRpc]
    private void RpcToggleFishActive(bool isActive) {
        fishModel.SetActive(isActive);
    }

    private void OnDrawGizmosSelected() {
        SetupArcPositions();

        Gizmos.color = Color.green;

        Vector3 startPos = startArcPos;
        for (float i = 0; i <= 1.1f; i += 0.1f) {
            Vector3 endPos = GetArcCoordinates(i);
            Gizmos.DrawLine(startPos, endPos);
            startPos = endPos;
        }
    }
}
