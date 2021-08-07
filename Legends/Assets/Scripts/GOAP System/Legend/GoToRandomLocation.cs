using System.Collections;
using UnityEngine;

public class GoToRandomLocation : AgentAction
{

    public override bool PrePerform()
    {
        if (!randomTarget) randomTarget = agent.randomTarget;
        float delay = Random.Range(1f, 3f);
        StartCoroutine("CompleteAction", delay);
        GoToRandomTarget();
        return true;
    }

    private void Update()
    {
        if (running)
        {
            
            if (!target)
            {
                GoToRandomTarget();
            }
            else
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance <= 8)
                {
                    GoToRandomTarget();
                }
                if (!navAgent.hasPath) GoToRandomTarget();
                navAgent.SetDestination(target.transform.position);
            }
        }
    }

    private void GoToRandomTarget()
    {
        target = RandomTarget();
    }

    public IEnumerator CompleteAction(float delay)
    {
        yield return new WaitForSeconds(delay);
        PostPerform();
    }

    public override bool PostPerform()
    {
        if (agent.nextAction != null)
        {
            if (agent.nextAction.preconditionsMap.ContainsKey("safe"))
            {
                agent.nextAction.preconditionsMap.Remove("safe");
            }
        }
        agent.beliefs.RemoveState("in danger");
        running = false;
        return true;
    }

}
