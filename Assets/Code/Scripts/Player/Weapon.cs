using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EWeaponType 
{
    Melee,
    Ranged
}
public interface IDamagable
{
    bool CanBeDamaged { get; }
    void TakeDamage(float damage);
    GameObject GetDamagableGameObject { get; }  //Used to cast different things
}

public abstract class Weapon : MonoBehaviour
{
    public float weaponBaseDamage;
    protected EWeaponType weaponType;
    public bool weaponInUse = false;
    public bool weaponCanBeEquipped = true;
    public PlayerController weaponOwner;
    protected Coroutine weaponFireCoroutine;

    public abstract void Awake();
    public abstract void StartWeapon();
    public abstract void StopWeapon();

    public virtual IEnumerator WeaponFire() 
    {
        yield return null;
    }


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

        //Remove parents
        transform.SetParent(null);

        return true;
    }

    public void RemoveWeaponOwner() 
    {
        weaponOwner = null;
    }

    public virtual void ApplyOnHitEffect(IDamagable damagableObject) 
    {

    }

    public bool IsWeaponActive() 
    {
        return gameObject.activeSelf;
    }
}
