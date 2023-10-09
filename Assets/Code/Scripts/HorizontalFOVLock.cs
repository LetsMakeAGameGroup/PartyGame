using UnityEngine;

// Lock the cameras horizontal field of view so it will frame the same view in the horizontal regardless of aspect ratio.
[RequireComponent(typeof(Camera))]
public class HorizontalFOVLock : MonoBehaviour {
    [Header("Settings")]
    [Tooltip("The FOV for the resolution 1920 x 1080.")]
    [SerializeField] private float fieldOfView;

    private Camera cam;
    private float cameraAspect;

    private void Awake() {
        cam = GetComponent<Camera>();
    }

    private void Update() {
        if (cameraAspect != cam.aspect) {
            cameraAspect = cam.aspect;
            cam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(fieldOfView, cameraAspect);
        }
    }
}
