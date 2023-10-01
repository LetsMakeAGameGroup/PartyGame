using UnityEngine;
using Mirror;

public class CollectableWispEffect : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private AudioClip collectAudioClip;
    [SerializeField] private AudioClip spawnAudioClip;

    [Header("Settings")]
    [Tooltip("How many points to give the player when collected.")]
    public int pointsToAdd = 2;

    private void Start() {
        if (!isServer) return;

        RpcPlaySpawnAudio();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player") || !isServer || !other.GetComponent<PlayerMovementComponent>() || !other.GetComponent<PlayerMovementComponent>().CanMove || transform.root.CompareTag("Player")) return;

        if (other.TryGetComponent(out WispEffect wispEffect)) {
            if (wispEffect.holdingWisp || !wispEffect.canPickup) return;

            RpcSetParent(other.gameObject);
            RpcPlayCollectAudio();
            wispEffect.TargetToggleGlowDisplay(true);
        }
    }

    [ClientRpc]
    public void RpcSetParent(GameObject player) {
        gameObject.transform.parent = player.GetComponent<WispEffect>().wispContainer.transform;
        gameObject.transform.localPosition = Vector3.zero;

        player.GetComponent<WispEffect>().holdingWisp = gameObject;
    }

    [ClientRpc]
    private void RpcPlaySpawnAudio() {
        if (collectAudioClip != null) {
            AudioSource.PlayClipAtPoint(spawnAudioClip, transform.position);
        }
    }

    [ClientRpc]
    private void RpcPlayCollectAudio() {
        if (collectAudioClip != null) {
            AudioSource.PlayClipAtPoint(collectAudioClip, transform.position);
        }
    }
}
