using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class WispMovement : MonoBehaviour {
    [Header("References")]
    [SerializeField] private VisualEffect visualEffect;

    private GameObject[] players;

    [Header("Settings")]
    [Tooltip("Speed of bobbing up and down.")]
    [SerializeField] private float bobSpeed = 1f;
    [Tooltip("Distance from initial position when bopping up and down.")]
    [SerializeField] private float bobDistance = 0.25f;

    private void OnEnable() {
        players = GameObject.FindGameObjectsWithTag("Player");

        StartCoroutine(Bobbing());
    }

    private void FixedUpdate() {
        if (players.Length == 0) return;

        Vector3 closestPlayerPosition = players[0].transform.position;
        float closestPlayerDistance = Vector3.Distance(transform.position, closestPlayerPosition);

        if (players.Length > 1) {
            for (int i = 1; i < players.Length; i++) {
                Vector3 tempPlayerPosition = players[i].transform.position;
                float tempPlayerDistance = Vector3.Distance(transform.position, tempPlayerPosition);

                if (tempPlayerDistance < closestPlayerDistance) {
                    closestPlayerPosition = tempPlayerPosition;
                    closestPlayerDistance = tempPlayerDistance;
                }
            }
        }

        //transform.rotation = Quaternion.LookRotation((closestPlayerPosition - transform.position).normalized);
        transform.LookAt(closestPlayerPosition);
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
