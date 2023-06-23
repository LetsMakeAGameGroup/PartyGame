using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class CollectiblePoint : NetworkBehaviour {
    public UnityEvent<GameObject,int> onPointsAdd = new();

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        if (!isClientOnly) onPointsAdd?.Invoke(other.gameObject, 1);

        Destroy(gameObject);
    }
}
