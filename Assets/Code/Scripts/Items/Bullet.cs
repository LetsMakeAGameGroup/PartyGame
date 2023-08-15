using UnityEngine;
using Mirror;

public abstract class Bullet : NetworkBehaviour {
    [HideInInspector] public GameObject shooterPlayer = null;
    [HideInInspector] public int damage = 10;

    [SyncVar(hook = nameof(SetVelocity)), HideInInspector] public Vector3 bulletVelocity = Vector3.zero;

    private void OnTriggerEnter(Collider other) {
        if (!isServer || other.gameObject == shooterPlayer || (other.transform.parent && other.transform.parent.gameObject == shooterPlayer)) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerHitbox") || other.gameObject.layer == LayerMask.NameToLayer("Hittable")) {
            OnHit(other.gameObject);
        }
        Destroy(gameObject);
    }

    public virtual void OnHit(GameObject hitObject) {
        // Check if hit object has health and do damage here.
    }

    public void SetVelocity(Vector3 oldVelocity, Vector3 newVelocity) {
        GetComponent<Rigidbody>().velocity = newVelocity;
    }
}
