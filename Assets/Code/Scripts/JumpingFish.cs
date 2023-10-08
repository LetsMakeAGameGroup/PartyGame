using Mirror;
using System.Collections;
using Unity.VisualScripting;
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

    private float time = 0;
    private bool isJumping = false;
    private float currentJumpDuration = 0;
    private float endJumpDuration = 0;

    private void Start() {
        fishModel.SetActive(false);
        SetupArcPositions();
        endJumpDuration = jumpEndDistance * jumpHeightDistance * jumpSpeed;

        if (!isServer) return;

        StartCoroutine(FishJump());
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player") || !other.GetComponent<NetworkIdentity>().isLocalPlayer) return;

        if (other.gameObject.TryGetComponent(out PlayerMovementComponent playerMovementComponent)) {
            Vector3 direction = transform.forward * knockbackForce;
            direction.y = verticalForce;
            playerMovementComponent.KnockbackCharacter(direction, stunTime);
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

    private void FixedUpdate() {
        if (!isJumping) {
            time = Time.time;
            return;
        }

        time = Time.time - time;

        if (currentJumpDuration < endJumpDuration) {
            float timeRatio = currentJumpDuration / endJumpDuration;
            transform.position = GetArcCoordinates(timeRatio);
            transform.LookAt(GetArcCoordinates(timeRatio + 0.1f));

            currentJumpDuration += Time.deltaTime;
        } else {
            fishModel.SetActive(false);
            isJumping = false;

            if (isServer) {
                StartCoroutine(FishJump());
            }
        }

        time = Time.time;
    }

    private IEnumerator FishJump() {
        yield return new WaitForSeconds(Random.Range(minRandomJumpTime, maxRandomJumpTime));

        RpcEnableFish();
    }

    [ClientRpc]
    private void RpcEnableFish() {
        transform.position = startArcPos;
        fishModel.SetActive(true);

        //currentJumpDuration = (float)(NetworkClient.connection.remoteTimeStamp / 1000);
        currentJumpDuration = 0;
        isJumping = true;
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
