using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class CollectiblePoint : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private AudioClip collectAudioClip;
    [SerializeField] private AudioClip spawnAudioClip;

    [HideInInspector] public UnityEvent<GameObject, int> onPointsAdd = new();

    private void Start() {
        if (!isServer) return;

        RpcPlaySpawnAudio();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player") || !isServer) return;

        RpcPlayCollectAudio();

        onPointsAdd?.Invoke(other.gameObject, 1);
    }

    [ClientRpc]
    private void RpcPlaySpawnAudio() {
        if (collectAudioClip != null) {
            AudioSource.PlayClipAtPoint(spawnAudioClip, gameObject.transform.position);
        }
    }

    [ClientRpc]
    private void RpcPlayCollectAudio() {
        if (collectAudioClip != null) {
            AudioSource.PlayClipAtPoint(collectAudioClip, gameObject.transform.position);
        }
        Destroy(gameObject);
    }
}
