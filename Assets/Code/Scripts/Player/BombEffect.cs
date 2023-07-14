using Mirror;
using UnityEngine;

public class BombEffect : NetworkBehaviour {
    public GameObject bombContainer = null;

    [HideInInspector] public GameObject holdingBomb = null;


    [ClientRpc]
    public void RpcEquipBomb(GameObject bomb) {
        holdingBomb = bomb;

        holdingBomb.transform.parent = bombContainer.transform;
        holdingBomb.transform.localPosition = Vector3.zero;

        TargetToggleVisability(false);
    }

    [TargetRpc]
    public void TargetToggleVisability(bool isDaylight) {
        GameObject sun = GameObject.FindGameObjectWithTag("Sun");

        sun.transform.rotation = Quaternion.Euler(isDaylight ? 50 : -50, sun.transform.rotation.y, sun.transform.rotation.z);
    }
}
