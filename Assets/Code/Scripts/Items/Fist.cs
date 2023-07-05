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
        }
    }
}
