using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public interface IInteractable
{
    void Interact(PlayerController player);
    public bool CanBeInteracted { get; }
    public string InteractableHUDMessege { get; }
}

public delegate void OnInteract(PlayerController player);

public class InteractableComponent : NetworkBehaviour, IInteractable
{
    public OnInteract onInteract;
    public OnInteract OnInteractDelegate { get { return onInteract; } set { onInteract += value; } }

    bool canBeInteracted = true;
    public bool CanBeInteracted { get { return canBeInteracted; } }

    [SerializeField] private string interactableHUDMessege;
    public string InteractableHUDMessege { get { return interactableHUDMessege; } }

    public void Interact(PlayerController player)
    {
        Debug.Log(transform.name + " was interacted by " + player.playerName);

        if (onInteract != null)
        {
            onInteract(player);
        }
    }
}