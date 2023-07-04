using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBlaster : RangedWeapon {
    public override void SpawnBullet(Vector3 endPos) {
        // Instantiate the bullet across the server. Starting at bulletSpawnTrans's position and move towards endPos.
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTrans.position, bulletSpawnTrans.rotation);
        bullet.transform.LookAt(endPos);
        bullet.GetComponent<Bullet>().bulletVelocity = bullet.transform.forward * bulletSpeed;
        bullet.GetComponent<Bullet>().shooterPlayer = playerController.gameObject;
        bullet.GetComponent<BerryBullet>().bulletColor = playerController.playerColor;
        Destroy(bullet, Vector3.Distance(transform.position, endPos) / bulletSpeed + 0.1f);
        NetworkServer.Spawn(bullet);
    }
}
