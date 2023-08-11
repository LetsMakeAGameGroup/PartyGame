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
        if (target.TryGetComponent(out PlayerMovementComponent playerMovementComponent)) {
            playerMovementComponent.TargetKnockbackCharacter(transform.TransformDirection(new Vector3(0, verticalForce, knockbackForce)));
            StartCoroutine(playerMovementComponent.StunPlayer(stunTime));

            if (target.TryGetComponent(out WispEffect wispEffect) && wispEffect.holdingWisp) {
                wispEffect.RpcDropWisp();
            }

            if (target.TryGetComponent(out BombEffect bombEffect) && !bombEffect.holdingBomb && playerController.GetComponent<BombEffect>().holdingBomb) {
                bombEffect.RpcEquipBomb(playerController.GetComponent<BombEffect>().holdingBomb);

                playerController.GetComponent<BombEffect>().TargetToggleVisability(true);
                playerController.GetComponent<BombEffect>().holdingBomb = null;
            }
        }
    }
}
