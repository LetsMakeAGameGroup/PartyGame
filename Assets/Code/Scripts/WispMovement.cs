using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class WispMovement : MonoBehaviour {
    [Header("References")]
    [SerializeField] private VisualEffect visualEffect;

    private MinigameHandler minigameHandler;

    [Header("Settings")]
    [Tooltip("Speed of bobbing up and down.")]
    [SerializeField] private float bobSpeed = 1f;
    [Tooltip("Distance from initial position when bopping up and down.")]
    [SerializeField] private float bobDistance = 0.25f;

    private void OnEnable() {
        minigameHandler = FindFirstObjectByType<MinigameHandler>();

        StartCoroutine(Bobbing());
    }

    private void FixedUpdate() {
        if (!minigameHandler) return;

        transform.LookAt(NetworkClient.localPlayer.transform.position);
        transform.rotation *= Quaternion.FromToRotation(Vector3.left, Vector3.forward);
    }

    private IEnumerator Bobbing() {
        Vector3 topBobPosition = transform.localPosition + new Vector3(0, bobDistance, 0);
        Vector3 bottomBobPosition = transform.localPosition - new Vector3(0, bobDistance, 0);
        float travelTime = 0.5f;

        while (true) {
            travelTime += Time.deltaTime * bobSpeed;

            transform.localPosition = Vector3.Slerp(bottomBobPosition, topBobPosition, Mathf.PingPong(travelTime, 1));
            visualEffect.SetFloat("Height", transform.localPosition.y);

            yield return null;
        }
    }
}
