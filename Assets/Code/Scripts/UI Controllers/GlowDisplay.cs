using Mirror;
using UnityEngine;

public class GlowDisplay : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Canvas canvas;

    [TargetRpc]
    public void TargetToggleCanvas(bool isEnabled) {
        canvas.enabled = isEnabled;
    }
}
