using UnityEngine;

public class Fist : MeleeWeapon {
    [Header("Fist Settings")]
    [Tooltip("The amount of force this will knock the hit entity backwards.")]
    [SerializeField] private float knockbackForce = 25f;
    [Tooltip("The amount of force this will knock the hit entity upwards.")]
    [SerializeField] private float verticalForce = 5f;
    [Tooltip("How many seconds the hit entity will be stunned for.")]
    [SerializeField] private float stunTime = 0.5f;

    public override void HitTarget(GameObject target) {
        if (target && target.TryGetComponent(out PlayerMovementComponent playerMovementComponent)) {
            Vector3 direction = playerController.transform.forward * knockbackForce;
            direction.y = verticalForce;
            playerMovementComponent.TargetKnockbackCharacter(direction, stunTime);

            if (target.TryGetComponent(out WispEffect wispEffect) && wispEffect.holdingWisp) {
                wispEffect.RpcDropWisp();
                wispEffect.TargetToggleGlowDisplay(false);
            }

            if (target.TryGetComponent(out BombEffect bombEffect) && !bombEffect.holdingBomb && playerController.GetComponent<BombEffect>().holdingBomb) {
                bombEffect.RpcEquipBomb(playerController.GetComponent<BombEffect>().holdingBomb);

                playerController.GetComponent<BombEffect>().TargetToggleVisability(playerController.GetComponent<BombEffect>().holdingBomb, false);
                playerController.GetComponent<BombEffect>().holdingBomb = null;
            }
        }
    }
}
