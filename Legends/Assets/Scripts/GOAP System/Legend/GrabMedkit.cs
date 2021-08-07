using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabMedkit : AgentAction
{
    private float distanceToTarget;

    public override bool PrePerform()
    {
        navAgent.stoppingDistance = 1;
        target = GetComponent<Agent>().currentTarget;
        if (target)
        {
            Debug.LogWarning("target name is " + target.transform.gameObject.name);
            navAgent.SetDestination(target.transform.position);
        }
        return true;
    }

    private void Update()
    {
        if (running)
        {
            if (!target)
            {
                PostPerform();
            }
            else
            {
                distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                TryPickup();
            }
        }
    }

    private void TryPickup()
    {
        var medKit = target.GetComponent<Loot>();
        if (medKit)
        {
            if (distanceToTarget <= medKit.aggroRange)
            {
                medKit.InteractWith(agent.character);
                Debug.LogWarning("Meds grabbed but not used!");
                PostPerform();
            }
        }
    }

    public override bool PostPerform()
    {
        var agent = GetComponent<Agent>();
        agent.beliefs.RemoveState("found medkit");
        if (agent.nextAction.preconditionsMap.ContainsKey("healthy"))
        {
            agent.nextAction.preconditionsMap.Remove("healthy");
        }
        running = false;
        return true;
    }
}
