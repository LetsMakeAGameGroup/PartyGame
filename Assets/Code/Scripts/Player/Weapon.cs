using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EWeaponType 
{
    Melee,
    Ranged
}

public abstract class Weapon : MonoBehaviour
{  
    protected EWeaponType weaponType;
    public bool weaponInUse = false;
    public bool weaponCanBeEquipped = true;
    public PlayerController weaponOwner;

    public abstract void Awake();
    public abstract void StartWeapon();
    public abstract void StopWeapon();

    //OnWeaponEquip returns a bool to see if it was sucesfully equipped
    public virtual bool OnWeaponEquip(PlayerController controller) 
    {
        if (!weaponCanBeEquipped) { return false; }

        weaponOwner = controller;
        return true;
    }

    //OnWeaponUnEquip returns a bool to see if it was sucesfully unequipped
    public virtual bool OnWeaponUnEquip(PlayerController controller) 
    {
        if (weaponOwner != controller) { return false; }

        weaponOwner = null;
        return true;
    }

    public void RemoveWeaponOwner() 
    {
        weaponOwner = null;
    }
}
