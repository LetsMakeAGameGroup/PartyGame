using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureTarget : NetworkBehaviour {
    public GameObject playerOwner = null;
    public int pointsGiven = 1;

    [SerializeField] private Renderer colorRenderer = null;

    public void SetOwner(GameObject player) {
        playerOwner = player;
        RpcSetColor(player.GetComponent<PlayerController>().playerColor);
    }

    [ClientRpc]
    private void RpcSetColor(string colorName) {
        colorRenderer.material.color = PlayerColorOptions.options[colorName];
    }
}
