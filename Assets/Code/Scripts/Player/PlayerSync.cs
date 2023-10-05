using Mirror;
using UnityEngine;

public class PlayerSync : NetworkBehaviour {
    [Header("Settings")]
    [Tooltip("How much to lerp per fixed update.")]
    public float lerpRate;

    [SyncVar] private Vector3 syncPosition;
    [SyncVar] private Quaternion syncRotation;

    private void Start() {
        syncPosition = transform.position;
        syncRotation = transform.rotation;
    }

    private void FixedUpdate() {
        TransmitPosition();
        Lerp();
    }
    void Lerp() {
        if (isLocalPlayer) return;
        transform.position = Vector3.Lerp(transform.position, syncPosition, Time.fixedDeltaTime * lerpRate);
        transform.rotation = Quaternion.Lerp(transform.rotation, syncRotation, Time.fixedDeltaTime * lerpRate);
    }

    [ClientCallback]
    void TransmitPosition() {
        if (!isLocalPlayer) return;
        CmdProvidePositionToServer(transform.position, transform.rotation);
    }

    [Command]
    void CmdProvidePositionToServer(Vector3 pos, Quaternion rot) {
        syncPosition = pos;
        syncRotation = rot;
    }
}