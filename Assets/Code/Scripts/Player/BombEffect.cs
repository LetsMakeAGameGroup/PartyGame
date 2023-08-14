using Mirror;
using UnityEngine;

public class BombEffect : NetworkBehaviour {
    [Header("References")]
    public GameObject bombContainer;

    [HideInInspector] public GameObject holdingBomb;


    [ClientRpc]
    public void RpcEquipBomb(GameObject bomb) {
        if (bomb == null) return;

        holdingBomb = bomb;

        holdingBomb.transform.parent = bombContainer.transform;
        holdingBomb.transform.localPosition = Vector3.zero;

        TargetToggleVisability(false);
    }

    [TargetRpc]
    public void TargetToggleVisability(bool isDaylight) {
        GameObject sun = GameObject.FindGameObjectWithTag("Sun");
        if (sun == null) return;

        sun.transform.rotation = Quaternion.Euler(isDaylight ? 50 : -50, sun.transform.rotation.y, sun.transform.rotation.z);
    }
}
