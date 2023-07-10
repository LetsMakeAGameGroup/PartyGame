using Mirror;
using UnityEngine;

public class WispEffect : NetworkBehaviour {
    public GameObject wispContainer = null;

    [HideInInspector] public GameObject holdingWisp = null;

    [SerializeField] private float distanceDroppedFromGround = 0.5f;

    [ClientRpc]
    public void RpcDropWisp() {
        int excludePlayerLayerMask =~ LayerMask.GetMask("Player");
        Vector3 dropLocation = Vector3.zero;
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
