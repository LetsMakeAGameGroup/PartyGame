using Mirror;
using UnityEngine;

public class WispEffect : NetworkBehaviour {
    public GameObject wispContainer = null;

    [HideInInspector] public GameObject holdingWisp = null;

    [SerializeField] private float distanceDroppedFromGround = 1f;

    [ClientRpc]
    public void DropWisp() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f)) {
            holdingWisp.transform.parent = null;
            holdingWisp.transform.position = hit.point + new Vector3(0, distanceDroppedFromGround, 0);

            holdingWisp = null;
        } else {
            Debug.LogError("Player is no where near the ground to drop Wisp.", transform);
        }
    }
}
