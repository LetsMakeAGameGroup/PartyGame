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
        holdingBomb.transform.localRotation = Quaternion.identity;

        TargetToggleVisability(bomb, true);
    }

    [TargetRpc]
    public void TargetToggleVisability(GameObject bomb, bool isHoldingBomb) {
        if (bomb != null) {
            MeshRenderer[] bombRenderers = bomb.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in bombRenderers) {
                renderer.enabled = !isHoldingBomb;
            }
        }

        GameObject sun = GameObject.FindGameObjectWithTag("Sun");
        if (sun == null) return;

        sun.transform.rotation = Quaternion.Euler(isHoldingBomb ? -50 : 50, sun.transform.rotation.y, sun.transform.rotation.z);
    }
}
