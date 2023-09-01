using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkTransformReliable))]
public class ConstantRotation : NetworkBehaviour {
    [Header("Settings")]
    [Tooltip("What axis the object should rotate around. An increased number in a single axis means it will move faster around that axis.")]
    [SerializeField] private Vector3 rotateDirection;
    [Tooltip("If this object should immediately start moving. If not, it will only start moving once the current minigame starts.")]
    public bool canMove = false;

    private void Awake() {
        if (rotateDirection == Vector3.zero) {
            Debug.LogError("rotateDirection on ConstantRotation is empty. Destroying this ConstantRotation as it's not being used.", gameObject);
            Destroy(this);
        }
    }

    private void Update() {
        if (!isServer || !canMove) return;

        transform.Rotate(Time.deltaTime * rotateDirection);
    }
}
