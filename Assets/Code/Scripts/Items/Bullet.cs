using UnityEngine;
using Mirror;

public abstract class Bullet : NetworkBehaviour {
    [HideInInspector] public int damage = 10;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") || other.gameObject.layer == LayerMask.NameToLayer("Hittable")) {
            OnHit(other.gameObject);
        }
        Destroy(gameObject);
    }

    public virtual void OnHit(GameObject hitObject) {
        // Check if hit object has health and do damage here.
    }
}
