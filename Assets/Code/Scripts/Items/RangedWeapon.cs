using Mirror;
using System.Collections;
using UnityEngine;

public class RangedWeapon : Weapon {
    public GameObject bulletPrefab;
    public Transform bulletSpawnTrans;
    public float bulletSpeed = 30f;

    public override void Use() {
        // Checks if player is looking at an object within maxDistance and set its endPos at the hit object. If there is no object in sight, end bullet at maxDistance.
        Vector3 endPos;
        if (Physics.Raycast(playerController.playerCamera.gameObject.transform.position, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.forward), out RaycastHit hit, hitDistance)) {
            endPos = hit.point;
            // If the hit object can be interacted with using on-hit, prepare to apply on-hit effects to it.
            // TODO: Make interactable
            /*if (hit.transform.GetComponent<Health>()) {
                StartCoroutine(ShootHitEnemy(hit));
            }*/
        } else {
            endPos = playerController.playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, hitDistance));
        }

        // Instantiate the bullet across the server. Starting at bulletSpawnTrans's position and move towards endPos.
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTrans.position, bulletSpawnTrans.rotation);
        bullet.transform.LookAt(endPos);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
        Destroy(bullet, Vector3.Distance(transform.position, endPos) / bulletSpeed);
        NetworkServer.Spawn(bullet);
    }

    // Once the bullet has arrived at the enemy, deal damage to it.
    IEnumerator ShootHitEnemy(RaycastHit _hit) {
        float travelTime = Vector3.Distance(transform.position, _hit.point) / bulletSpeed;
        yield return new WaitForSeconds(travelTime);
        // TODO: Make interactable
        //if (_hit.transform != null) _hit.transform.GetComponent<Health>().TakeDamage(baseDamage, goldPerHit, playerController);
    }
}
