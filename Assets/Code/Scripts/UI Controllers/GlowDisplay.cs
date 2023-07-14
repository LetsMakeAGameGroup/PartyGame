using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowDisplay : NetworkBehaviour {
    [SerializeField] private Canvas canvas = null;

    [TargetRpc]
    public void TargetToggleCanvas(bool isEnabled) {
        canvas.enabled = isEnabled;
    }
}
