using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetHealthAction : GoapAction
{
    public override void DoReset()
    {
        StopAllCoroutines();
        running = false;
    }

    public override bool PrePerform()
    {
        bool medkitFound = FindMedkit();
        if (medkitFound)
        {
            // move towards it and try to use it
            StartCoroutine(GrabMedkit());
        }
        return medkitFound;
    }

    private IEnumerator GrabMedkit()
    {
        agent.navAgent.stoppingDistance = 1;
        while (true)
        {
            TryPickup();
            agent.Move(this);
            yield return null;
        }
    }

    private bool FindMedkit()
    {
        Queue<GameObject> collectables = fov.FindVisibleTargets(this, true);
        if (collectables.Count > 0)
        {
            foreach (var collectable in collectables)
            {
                if (collectable.tag == "Health")
                {
                    target = collectable;
                    return true;
                }
            }
        }
        return false;
    }

    private void TryPickup()
    {
        if (target == null)
        {
            DoReset();
            return;
        }

        distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        Loot medkit = target.GetComponentInParent<Loot>();
        if (medkit)
        {
            if (distanceToTarget < medkit.aggroRange)
            {
                medkit.InteractWith(agent.character);
                DoReset();
            }
        }
    }
}
