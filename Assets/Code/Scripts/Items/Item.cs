using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string itemName = "";
    public float useCooldown = 1f;

    public PlayerController playerController;

    public abstract void Use();
}
