using UnityEngine;

public abstract class MeleeWeapon : Weapon {
    public override void Use() {
        // Checks if player is looking at an object within maxDistance.
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Hittable");
        if (Physics.Raycast(playerController.playerCamera.gameObject.transform.position + playerController.playerCamera.gameObject.transform.forward * 0.5f, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.forward), out RaycastHit hit, hitDistance, layerMask)) {
            // If the hit object can be interacted with using on-hit, apply on-hit effects to it.
            HitTarget(hit.transform.gameObject);
        }
    }

    public abstract void HitTarget(GameObject target);
}
