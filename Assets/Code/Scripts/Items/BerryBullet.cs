using Mirror;
using UnityEngine;

public class BerryBullet : Bullet {
    [Header("Berry Bullet References")]
    [SerializeField] private Renderer berryRenderer;

    [Header("Berry Bullet Settings")]
    [Tooltip("The amount of force this will knock the hit entity backwards.")]
    [SerializeField] private float knockbackForce = 25f;
    [Tooltip("The amount of force this will knock the hit entity upwards.")]
    [SerializeField] private float verticalForce = 5f;
    [Tooltip("How many seconds the hit entity will be stunned for.")]
    [SerializeField] private float stunTime = 0.5f;

    [SyncVar(hook = nameof(SetColor)), HideInInspector] public string bulletColor = "";

    public void SetColor(string oldColor, string newColor) {
        berryRenderer.material.color = PlayerColorOptions.options[newColor];
    }

    public override void OnHit(GameObject hitObject) {
        base.OnHit(hitObject);

        if (hitObject.TryGetComponent(out CaptureTarget captureTarget)) {
            captureTarget.SetOwner(shooterPlayer);
        }

        if (hitObject.transform.parent && hitObject.transform.parent.TryGetComponent(out PlayerMovementComponent playerMovementComponent)) {
            Vector3 direction = transform.forward * knockbackForce;
            direction.y = verticalForce;
            playerMovementComponent.TargetKnockbackCharacter(direction, stunTime);
        }
    }
}
