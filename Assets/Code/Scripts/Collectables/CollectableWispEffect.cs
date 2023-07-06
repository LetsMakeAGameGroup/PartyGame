using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CollectableWispEffect : NetworkBehaviour {
    public int pointsToAdd = 2;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player") || !isServer || !other.GetComponent<PlayerMovementComponent>() || !other.GetComponent<PlayerMovementComponent>().CanMove) return;

        if (other.TryGetComponent(out WispEffect wispEffect)) {
            if (wispEffect.holdingWisp) return;

            RpcSetParent(other.gameObject);
        }
    }

    [ClientRpc]
    public void RpcSetParent(GameObject player) {
        gameObject.transform.parent = player.GetComponent<WispEffect>().wispContainer.transform;
        gameObject.transform.localPosition = Vector3.zero;

        player.GetComponent<WispEffect>().holdingWisp = gameObject;
    }
}
