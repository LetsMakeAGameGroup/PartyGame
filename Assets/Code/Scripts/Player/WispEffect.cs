using Mirror;
using UnityEngine;

public class WispEffect : NetworkBehaviour {
    [Header("References")]
    public GameObject wispContainer;

    [HideInInspector] public GameObject holdingWisp;

    [Header("Settings")]
    [Tooltip("How far from the ground the wisp will be dropped.")]
    [SerializeField] private float distanceDroppedFromGround = 0.5f;

    [ClientRpc]
    public void RpcDropWisp() {
        int excludePlayerLayerMask =~ LayerMask.GetMask("Player");
        Vector3 dropLocation;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, excludePlayerLayerMask)) {
            dropLocation = hit.point + new Vector3(0, distanceDroppedFromGround, 0);
        } else {
            dropLocation = transform.position;
        }

        holdingWisp.transform.parent = null;
        holdingWisp.transform.position = dropLocation;

        holdingWisp = null;
    }
}
