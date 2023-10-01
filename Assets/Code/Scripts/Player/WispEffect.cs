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

    [ClientRpc]
    public void RpcDropWisp() {
        int excludePlayerLayerMask =~ LayerMask.GetMask("Player");
        Vector3 dropLocation;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, excludePlayerLayerMask)) {
            dropLocation = hit.point;
            holdingWisp.transform.SetParent(hit.transform);
        } else {
            dropLocation = transform.position;
        }

        holdingWisp.transform.position = dropLocation;

        holdingWisp = null;

        //dropAudioSource.Play();
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
