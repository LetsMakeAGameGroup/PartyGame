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

    }

    public override void StopWeapon()
    {

    }
}
