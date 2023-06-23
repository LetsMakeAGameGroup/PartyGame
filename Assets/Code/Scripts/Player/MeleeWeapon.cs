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
    }

    public override void StopWeapon()
    {

    }

}
