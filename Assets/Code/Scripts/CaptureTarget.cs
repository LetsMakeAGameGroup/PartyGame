using Mirror;
using UnityEngine;

public class CaptureTarget : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Renderer colorRenderer;

    [HideInInspector] public GameObject playerOwner;

    [Header("Settings")]
    [Tooltip("How many points having this target captured will give every interval.")]
    public int pointsGiven = 1;

    public void SetOwner(GameObject player) {
        playerOwner = player;
        RpcSetColor(player.GetComponent<PlayerController>().playerColor);
    }

    [ClientRpc]
    private void RpcSetColor(string colorName) {
        colorRenderer.material.color = PlayerColorOptions.options[colorName];
    }
}
