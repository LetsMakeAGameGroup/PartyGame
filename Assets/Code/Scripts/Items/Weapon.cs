using UnityEngine;

public class Weapon : Item {
    [Header("Weapon Settings")]
    [Tooltip("How much damage this weapon will do on hit.")]
    public int baseDamage = 10;
    [Tooltip("How far this weapon can hit for.")]
    public float hitDistance = 25f;

    public override void Use() {}
}
