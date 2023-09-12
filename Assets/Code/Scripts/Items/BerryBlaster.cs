using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBlaster : RangedWeapon {
    [Header("Berry Blaster References")]
    [SerializeField] private Renderer ammoRenderer;

    private void Start() {
        ammoRenderer.material.color = PlayerColorOptions.options[playerController.playerColor];
    }

    public override void SpawnBullet(Vector3 endPos) {
        // Instantiate the bullet across the server. Starting at bulletSpawnTrans's position and move towards endPos.
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTrans.position, bulletSpawnTrans.rotation);
        bullet.transform.LookAt(endPos);
        bullet.GetComponent<Bullet>().bulletVelocity = bullet.transform.forward * bulletSpeed;
        bullet.GetComponent<Bullet>().shooterPlayer = playerController.gameObject;
        bullet.GetComponent<Bullet>().hitDistance = hitDistance;
        bullet.GetComponent<BerryBullet>().bulletColor = playerController.playerColor;
        NetworkServer.Spawn(bullet);
    }
}
