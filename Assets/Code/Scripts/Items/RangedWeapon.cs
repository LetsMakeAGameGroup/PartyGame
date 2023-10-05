using UnityEngine;

public class RangedWeapon : Weapon {
    [Header("Ranged Weapon References")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnTrans;

    [Header("Ranged Weapon Settings")]
    [Tooltip("The forward velocity speed of the bullet.")]
    public float bulletSpeed = 30f;

    public override void Use() {
        playerController.GetComponent<ItemController>().StartItemCooldown(useCooldown);

        // Checks if player is looking at an object within maxDistance and set its endPos at the hit object. If there is no object in sight, end bullet at maxDistance.
        Vector3 endPos;
        if (Physics.Raycast(playerController.playerCamera.gameObject.transform.position, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.forward), out RaycastHit hit, hitDistance)) {
            endPos = hit.point;
        } else {
            endPos = playerController.playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, hitDistance));
        }
        playerController.GetComponent<ItemController>().SpawnBullet(endPos);
    }
}