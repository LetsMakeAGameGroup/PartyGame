using Mirror;
using UnityEngine;

public abstract class Bullet : NetworkBehaviour {
    [Header("Bullet References")]
    [SerializeField] private Renderer colorRenderer;

    [HideInInspector] public GameObject shooterPlayer = null;
    [HideInInspector] public int damage = 10;
    [HideInInspector] public float hitDistance = float.MaxValue;

    [SyncVar(hook = nameof(SetVelocity)), HideInInspector] public Vector3 bulletVelocity = Vector3.zero;
    [SyncVar(hook = nameof(SetColor)), HideInInspector] public string bulletColor = "";

    private Vector3 initialSpawnPos;

    private void OnEnable() {
        initialSpawnPos = transform.position;
    }

    private void Update() {
        if (!isServer) return;

        if (Vector3.Distance(initialSpawnPos, transform.position) >= hitDistance) {
            NetworkServer.Destroy(gameObject);
        }
    }

    public void SetColor(string oldColor, string newColor) {
        colorRenderer.material.color = PlayerColorOptions.options[newColor];
    }

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
