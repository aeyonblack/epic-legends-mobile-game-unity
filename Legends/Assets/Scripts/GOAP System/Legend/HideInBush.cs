using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideInBush : AgentAction
{

    private float distanceToTarget;

    public override bool PrePerform()
    {
        if (!randomTarget)
        {
            randomTarget = GetComponent<Agent>().randomTarget;
        }
        navAgent.stoppingDistance = 1;
        StartHiding();
        return true;
    }

    private void Update()
    {
        if (running)
        {
            if (!target)
            {
                FindBush();
            }
            else
            {
                if (target.layer == LayerMask.NameToLayer("Bush"))
                    GoToBush();
                else
                    FindFurthestBush();
            }
            //Animate();
        }
    }

    private void FindBush()
    {
        Queue<GameObject> bushes = fov.FindNearestVisibleTargets(this, false);
        target = bushes.Count == 0 ? RandomTarget() : bushes.Dequeue();
    }

    private void FindFurthestBush()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= 5f)
        {
            FindBush();
        }
        else
        {
            Queue<GameObject> bushes = fov.FindNearestVisibleTargets(this, false);
            if (bushes.Count > 0)
            {
                target = bushes.Dequeue();
            }
        }
    }

    private void GoToBush()
    {
        Vector3 randomPosition = target.transform.position + Random.insideUnitSphere * 5f;
        randomPosition.y = target.transform.position.y;
        navAgent.SetDestination(randomPosition);
    }

    private void StartHiding()
    {
        StartCoroutine("EndHideSequence");
    }

    private IEnumerator EndHideSequence()
    {
        yield return new WaitForSeconds(30);
        PostPerform();
    }

    public override bool PostPerform()
    {
        running = false;
        return true;
    }

}
