using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : AgentAction
{
    private float distanceToTarget;

    private bool attacking = false;

    private Attacker attacker;

    public override bool PrePerform()
    {
        if (!randomTarget) randomTarget = agent.randomTarget;
        attacker = GetComponent<Attacker>();  
        FindTarget();
        StartCoroutine("SearchNearestTarget");
        return true;
    }

    private void Update()
    {
        if (running)
        {
            if (!target)
            {
                FindTarget();
            }
            else
            {
                ChaseTarget();
            }
        }
    }

    private void FindTarget()
    {
        Queue<GameObject> targets = new Queue<GameObject>();
        foreach (var t in fov.FindNearestVisibleTargets(this, true))
        {
            if (t.tag != "Glopnop")
                targets.Enqueue(t);
        }
        target = targets.Count == 0 ? RandomTarget() : targets.Dequeue();
    }

    private void ChaseTarget()
    {
        distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
        if (target.layer != LayerMask.NameToLayer("Character"))
        {
            if (attacking) StopAttacking();
            if (distanceToTarget <= navAgent.stoppingDistance + 2 || !navAgent.hasPath)
                FindTarget();
            else
                GetTargetFromQueue();
        }
        else
        {
            PerformAttack();
        }
        navAgent.SetDestination(target.transform.position);
    }

    private void PerformAttack()
    {
        if (distanceToTarget <= navAgent.stoppingDistance)
        {
            LookAtTarget();
            if (distanceToTarget > navAgent.stoppingDistance + 1)
            {
                if (attacking) StopAttacking();
                return;
            }
            if (!attacking) StartAttacking();
        }
        else
        {
            if (attacking) StopAttacking();
        }
    }

    private void StartAttacking()
    {
        if (!attacking) attacking = true;
        StartCoroutine("DelayAttack");
    }

    private void StopAttacking()
    {
        attacking = false;
        StopCoroutine("DelayAttack");
    }

    private IEnumerator DelayAttack()
    {
        attacker.SetTarget(target.transform);
        while(true)
        {
            agent.character.GetAnimator().SetTrigger("attack");
            attacker.AlertTarget();
            yield return new WaitForSeconds(attacker.attack.CoolDownSeconds);
        }
    }

    private void LookAtTarget()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.identity;
        if (direction != Vector3.zero)
        {
            lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 
            Time.deltaTime * navAgent.angularSpeed);
    }

    private IEnumerator SearchNearestTarget()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            Queue<GameObject> targets = new Queue<GameObject>();
            foreach (var t in fov.FindNearestVisibleTargets(this, true))
            {
                if (t.tag != "Glopnop")
                    targets.Enqueue(t);
            }
            if (targets.Count > 0) target = targets.Dequeue();
        }
    }

    private void GetTargetFromQueue()
    {
        Queue<GameObject> targets = new Queue<GameObject>();
        foreach (var t in fov.FindNearestVisibleTargets(this, true))
        {
            if (t.tag != "Glopnop")
                targets.Enqueue(t);
        }
        if (targets.Count != 0)
            target = targets.Dequeue();
    }

    public override bool PostPerform()
    {
        // this will never happen
        return true;
    }
}
