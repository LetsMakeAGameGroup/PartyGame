using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour {
    [SerializeField] private Vector3 rotateDirection = Vector3.zero;

    private void Update() {
        transform.Rotate(rotateDirection * Time.deltaTime);
    }
}
