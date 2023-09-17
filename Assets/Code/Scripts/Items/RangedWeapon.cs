using Mirror;
using UnityEngine;

public class RangedWeapon : Weapon {
    [Header("Ranged Weapon References")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnTrans;

    [Header("Ranged Weapon Settings")]
    [Tooltip("The forward velocity speed of the bullet.")]
    public float bulletSpeed = 30f;

    public override void Use() {
        playerController.GetComponent<ItemController>().TargetStartItemCooldown(useCooldown);

        // Checks if player is looking at an object within maxDistance and set its endPos at the hit object. If there is no object in sight, end bullet at maxDistance.
        if (Physics.Raycast(playerController.playerCamera.gameObject.transform.position, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.forward), out RaycastHit hit, hitDistance)) {
            SpawnBullet(hit.point);
        } else {
            SpawnBullet(playerController.playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, hitDistance)));
        }
    }

    public virtual void SpawnBullet(Vector3 endPos) {
        // Instantiate the bullet across the server. Starting at bulletSpawnTrans's position and move towards endPos.
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTrans.position, bulletSpawnTrans.rotation);
        bullet.transform.LookAt(endPos);
        bullet.GetComponent<Bullet>().bulletVelocity = bullet.transform.forward * bulletSpeed;
        bullet.GetComponent<Bullet>().shooterPlayer = playerController.gameObject;
        bullet.GetComponent<Bullet>().hitDistance = hitDistance;
        NetworkServer.Spawn(bullet);
    }
}
