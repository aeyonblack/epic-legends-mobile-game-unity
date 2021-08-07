using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeAttackAction : GoapAction
{
    public override void DoReset()
    {
        StopAllCoroutines();
        running = false;
    }

    public override bool PrePerform()
    {
        target = GetRandomTarget();
        StartCoroutine(PerformAction());
        return true;
    }

    private IEnumerator PerformAction()
    {
        while (true)
        {
            MoveToRandomLocation();
            float delay = Random.Range(2, 8);
            yield return new WaitForSeconds(delay);
            EndAction();
        }
    }

    private void MoveToRandomLocation()
    {
        agent.Move(this);
    }

    private void EndAction()
    {
        StopAllCoroutines();
        running = false;
    }

}
