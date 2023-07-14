using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fist : MeleeWeapon {
    [SerializeField] private float knockbackForce = 25f;
    [SerializeField] private float verticalForce = 5f;
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
