using UnityEngine;

// Lock the cameras horizontal field of view so it will frame the same view in the horizontal regardless of aspect ratio.
[RequireComponent(typeof(Camera))]
public class HorizontalFOVLock : MonoBehaviour {
    private Camera cam;
    private float fixedHorizontalFOV;
    private float cameraAspect;

    private void Awake() {
        cam = GetComponent<Camera>();
        fixedHorizontalFOV = cam.fieldOfView;
        cameraAspect = cam.aspect;
    }

    private void Update() {
        if (cameraAspect != cam.aspect) {
            cameraAspect = cam.aspect;
            cam.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(fixedHorizontalFOV * Mathf.Deg2Rad * 0.5f) / cameraAspect) * Mathf.Rad2Deg;
        }
    }
}
