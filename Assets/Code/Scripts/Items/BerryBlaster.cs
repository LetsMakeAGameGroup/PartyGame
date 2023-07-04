using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBlaster : RangedWeapon
{
    public override void Use() {
        // Checks if player is looking at an object within maxDistance and set its endPos at the hit object. If there is no object in sight, end bullet at maxDistance.
        Vector3 endPos;
        if (Physics.Raycast(playerController.playerCamera.gameObject.transform.position, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.forward), out RaycastHit hit, hitDistance)) {
            endPos = hit.point;
        } else {
            endPos = playerController.playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, hitDistance));
        }

        // Instantiate the bullet across the server. Starting at bulletSpawnTrans's position and move towards endPos.
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTrans.position, bulletSpawnTrans.rotation);
        bullet.transform.LookAt(endPos);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
        bullet.GetComponent<BerryBullet>().SetShooter(playerController.gameObject);
        bullet.GetComponent<BerryBullet>().bulletColor = playerController.playerColor;
        Destroy(bullet, Vector3.Distance(transform.position, endPos) / bulletSpeed + 0.1f);
        NetworkServer.Spawn(bullet);
    }
}
