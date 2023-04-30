using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Interactable_Base
{
    public float lifeTimeGain = 3;

    int foodCount = 1;

    protected override void CycleStart(int cycle)
    {
        base.CycleStart(cycle);

        foodCount = 1;

        EnableInteract();
        this.GetComponent<MeshRenderer>().enabled = true;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        foodCount--;

        interactingCreature.Feed(lifeTimeGain);

        if (foodCount <= 0)
        {
            DisableInteract();
            this.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
