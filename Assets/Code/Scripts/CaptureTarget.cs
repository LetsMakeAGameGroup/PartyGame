using Mirror;
using UnityEngine;

public class CaptureTarget : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Renderer[] colorRenderers;
    [SerializeField] private AudioSource captureAudioSource;

    [HideInInspector] public GameObject playerOwner;

    [Header("Settings")]
    [Tooltip("How many points having this target captured will give every interval.")]
    public int pointsGiven = 1;

    public void SetOwner(GameObject player) {
        playerOwner = player;
        RpcSetColor(player.GetComponent<PlayerController>().playerColor);
        TargetPlayCaptureAudio(player.GetComponent<NetworkIdentity>().connectionToClient);
    }

    [ClientRpc]
    private void RpcSetColor(string colorName) {
        foreach (Renderer colorRenderer in colorRenderers) {
            colorRenderer.material.color = PlayerColorOptions.options[colorName];
        }
    }

    [TargetRpc]
    private void TargetPlayCaptureAudio(NetworkConnectionToClient conn) {
        captureAudioSource.Play();
    }
}
