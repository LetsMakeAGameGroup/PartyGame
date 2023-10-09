using Mirror;
using System.Collections;
using UnityEngine;

public class WispEffect : NetworkBehaviour {
    [Header("References")]
    public GameObject wispContainer;
    [SerializeField] private AudioSource dropAudioSource;
    public Canvas glowDisplayCanvas;

    [HideInInspector] public GameObject holdingWisp;

    [HideInInspector] public bool canPickup = true;

    public void DropWisp() {
        CmdDropWisp();
    }

    [Command]
    public void CmdDropWisp() {
        RpcDropWisp();
        TargetToggleGlowDisplay(false);
    }

    [ClientRpc]
    public void RpcDropWisp() {
        int playerLayerMask = 1 << LayerMask.GetMask("Player");
        Vector3 dropLocation;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, playerLayerMask)) {
            dropLocation = hit.point;
            if (hit.collider.GetComponent<MoveObjectOverTime>() || hit.collider.GetComponent<ConstantRotation>()) {
                holdingWisp.transform.SetParent(hit.transform);
            } else {
                holdingWisp.transform.parent = null;
            }
        } else {
            dropLocation = transform.position;
        }

        holdingWisp.transform.position = dropLocation;

        holdingWisp = null;

        dropAudioSource.Play();
        StartCoroutine(PickupBuffer());
    }

    private IEnumerator PickupBuffer() {
        canPickup = false;

        yield return new WaitForSeconds(0.1f);

        canPickup = true;
    }

    [TargetRpc]
    public void TargetToggleGlowDisplay(bool isEnabled) {
        glowDisplayCanvas.enabled = isEnabled;
    }

    [ClientRpc]
    public void RpcPlayDropAudio() {
        dropAudioSource.Play();
    }
}
