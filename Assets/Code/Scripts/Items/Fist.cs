using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fist : MeleeWeapon {
    [SerializeField] private float knockbackForce = 25f;
    [SerializeField] private float verticalForce = 5f;
    public override void HitTarget(GameObject target) {
        if (target.GetComponent<PlayerMovementComponent>()) {
            target.GetComponent<PlayerMovementComponent>().TargetKnockbackCharacter(transform.TransformDirection(new Vector3(0, verticalForce, 1f * knockbackForce)));
        }
    }
}
