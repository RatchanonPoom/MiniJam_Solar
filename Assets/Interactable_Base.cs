using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Interactable_Base : MonoBehaviour
{
    protected bool isInteractable = true;

    protected Creature interactingCreature;

    //only the creature touching it first gets to interact
    private void OnTriggerEnter(Collider other)
    {
        if (!isInteractable || (interactingCreature != null && interactingCreature.isAlive))
            return;

        if (other.TryGetComponent(out Creature creature))
        {
            interactingCreature = creature;
            interactingCreature.interactTarget = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Creature creature) && creature == interactingCreature)
        {
            interactingCreature.interactTarget = null;
            interactingCreature = null;
        }
    }

    protected virtual void CycleStart(int cycle)
    {
        Debug.Log($"Ahh... a new cycle");
    }

    protected void EnableInteract()
    {
        isInteractable = true;
    }

    protected void DisableInteract()
    {
        isInteractable = false;

        interactingCreature.interactTarget = null;
        interactingCreature = null;
    }

    public virtual void OnInteract()
    {
        Debug.Log($"I, {name} have been interacted");
    }
}
