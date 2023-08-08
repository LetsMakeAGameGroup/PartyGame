using UnityEngine;

public class ConstantRotation : MonoBehaviour {
    [Header("Settings")]
    [Tooltip("What axis the object should rotate around. An increased number in a single axis means it will move faster around that axis.")]
    [SerializeField] private Vector3 rotateDirection;

    private void Update() {
        transform.Rotate(Time.deltaTime * rotateDirection);
    }
}
