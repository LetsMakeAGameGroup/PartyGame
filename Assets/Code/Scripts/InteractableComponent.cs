using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public interface IInteractable
{
    void Interact();
    public bool CanBeInteracted { get; }
}

public delegate void OnInteract();

public class InteractableComponent : NetworkBehaviour, IInteractable
{
    public OnInteract onInteract;
    public OnInteract OnInteractDelegate { get { return onInteract; } set { onInteract += value; } }

    bool canBeInteracted;
    public bool CanBeInteracted { get { return canBeInteracted; } }

    public void Interact()
    {
        if (onInteract != null)
        {
            onInteract();
        }
    }
}