using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {
    private Camera currentCamera;

    private void Start() {
        currentCamera = Camera.current;
    }

    private void LateUpdate() {
        if (currentCamera == null) currentCamera = Camera.allCameras[0];  // Get the new current camera.
        if (currentCamera == null) return;  // If still cannot get the current camera, do nothing further.

        Vector3 newRotation = currentCamera.transform.eulerAngles;
        newRotation.x = 0;
        newRotation.z = 0;
        transform.eulerAngles = newRotation;
    }
}
