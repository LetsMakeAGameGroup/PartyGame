using Mirror;
using UnityEngine;

public class WispEffect : NetworkBehaviour {
    [Header("References")]
    public GameObject wispContainer;
    [SerializeField] private AudioSource dropAudioSource;

    [HideInInspector] public GameObject holdingWisp;

    [ClientRpc]
    public void RpcDropWisp() {
        int excludePlayerLayerMask =~ LayerMask.GetMask("Player");
        Vector3 dropLocation;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, excludePlayerLayerMask)) {
            dropLocation = hit.point;
        } else {
            dropLocation = transform.position;
        }

        holdingWisp.transform.parent = null;
        holdingWisp.transform.position = dropLocation;

        holdingWisp = null;

        dropAudioSource.Play();
    }
}
