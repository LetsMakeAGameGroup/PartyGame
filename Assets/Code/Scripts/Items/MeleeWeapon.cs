using UnityEngine;

public abstract class MeleeWeapon : Weapon {
    [Header("Melee Weapon Settings")]
    [Tooltip("The amount of seconds before this can be used again if missed.")]
    public float missCooldown = 0.25f;

    public override void Use() {
        // Checks if player is looking at an object within maxDistance.
        LayerMask layerMask = 1 << LayerMask.NameToLayer("PlayerHitbox") | 1 << LayerMask.NameToLayer("Hittable");
        var reverseHits = Physics.RaycastAll(playerController.playerCamera.gameObject.transform.position + playerController.playerCamera.gameObject.transform.forward * hitDistance, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.back), hitDistance - 0.5f, layerMask);

        if (reverseHits.Length > 1) {
            RaycastHit closestHit = reverseHits[reverseHits.Length - 1];
            bool isDefaultHit = true;
            if (reverseHits.Length > 1) {
                for (int i = 0; i < reverseHits.Length; i++) {
                    if (reverseHits[i].collider.transform.parent && reverseHits[i].collider.transform.parent.gameObject == playerController.gameObject) continue;

                    if (isDefaultHit || reverseHits[i].distance < closestHit.distance) {
                        closestHit = reverseHits[i];
                        isDefaultHit = false;
                    }
                }
            }

            // If the hit object can be interacted with using on-hit, apply on-hit effects to it.
            playerController.GetComponent<ItemController>().StartItemCooldown(useCooldown);
            HitTarget(closestHit.transform.gameObject);
        } else {
            playerController.GetComponent<ItemController>().StartItemCooldown(missCooldown);
        }
    }

    public abstract void HitTarget(GameObject target);
}
