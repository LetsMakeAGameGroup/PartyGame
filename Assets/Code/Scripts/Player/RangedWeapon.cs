using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Weapon
{

    public override void Awake()
    {
        weaponType = EWeaponType.Ranged;
    }

    public override void StartWeapon()
    {
        Debug.Log("Ranged Attack!");
    }

    public override void StopWeapon()
    {

    }

}
