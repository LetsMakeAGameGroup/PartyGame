using UnityEngine;

public abstract class Item : MonoBehaviour {
    [HideInInspector] public PlayerController playerController;

    [Header("Item Settings")]
    [Tooltip("The name of this item.")]
    public string itemName = "";
    [Tooltip("How many seconds before this item can be used again.")]
    public float useCooldown = 1f;

    public abstract void Use();
}
