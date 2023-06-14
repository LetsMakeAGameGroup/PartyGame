using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    public override void Awake()
    {
        weaponType = EWeaponType.Melee;
    }

    public override void StartWeapon()
    {
        Debug.Log("Melee Attack!");

        RaycastHit hit;

        if (Physics.Raycast(weaponOwner.playerCamera.transform.position, weaponOwner.playerCamera.transform.forward, out hit, 1f)) 
        {
            IDamagable damagable = hit.transform.GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.TakeDamage(weaponBaseDamage);
                ApplyOnHitEffect(damagable);
            }
        }
    }

    public override void StopWeapon()
    {

    }

}
