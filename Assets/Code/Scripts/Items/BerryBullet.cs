using UnityEngine;

public class BerryBullet : Bullet {
    [Header("Berry Bullet Settings")]
    [Tooltip("The amount of force this will knock the hit entity backwards.")]
    [SerializeField] private float knockbackForce = 25f;
    [Tooltip("The amount of force this will knock the hit entity upwards.")]
    [SerializeField] private float verticalForce = 5f;
    [Tooltip("How many seconds the hit entity will be stunned for.")]
    [SerializeField] private float stunTime = 0.5f;

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
