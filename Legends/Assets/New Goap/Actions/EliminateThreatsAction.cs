using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliminateThreatsAction : GoapAction
{
    private bool attacking = false;

    private Attacker attacker;

    private float range;

    public override void DoReset()
    {
        StopAllCoroutines();
        attacker = null;
        attacking = false;
        running = false;
    }

    public override bool PrePerform()
    {
        attacker = GetComponent<Attacker>();
        range = attacker.attack.AttackDistance;
        agent.navAgent.stoppingDistance = range - 1;
        FindTarget();
        StartCoroutine(PerformAction());
        StartCoroutine(GetClosestEnemy());
        return true;
    }

    private IEnumerator PerformAction()
    {
        while (true)
        {
            if (target == null)
            {
                if (attacking) AbortAttack();
                FindTarget();
            }
            else ChaseTarget(); 
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator GetClosestEnemy()
    {
        while (true)
        {
            float delay = Random.Range(1, 5);

            yield return new WaitForSeconds(delay);

            Queue<GameObject> threats = GetTargetsFromQueue();
            if (threats.Count > 0) target = threats.Dequeue();
        }
    }

    private void FindTarget()
    {
        Queue<GameObject> threats = GetTargetsFromQueue();
        target = threats.Count == 0 ? GetRandomTarget() : threats.Dequeue();
    }

    private void SearchArea()
    {
        if (distanceToTarget < 5) FindTarget();
        else
        {
            Queue<GameObject> threats = GetTargetsFromQueue();
            if (threats.Count > 0) target = threats.Dequeue();
        }
    }

    private Queue<GameObject> GetTargetsFromQueue()
    {
        Queue<GameObject> threats = new Queue<GameObject>();
        foreach (var t in fov.FindVisibleTargets(this, true))
        {
            if (t.tag != "Glopnop")
            {
                threats.Enqueue(t);
            }
        }
        return threats;
    }

    private void ChaseTarget()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (target.layer != LayerMask.NameToLayer("Character"))
        {
            if (attacking) AbortAttack();
            SearchArea();
        }
        else
        {
            Attack();
        }
        agent.Move(this);
    }

    private void Attack()
    {
        if (distanceToTarget <= range)
        {
            LookAtTarget();
            if (!attacking)
            {
                attacking = true;
                attacker.AttackTheat(target);
            }
        }
        else
        {
            if (attacking) AbortAttack();
        }
    }

    private void AbortAttack()
    {
        attacker.HoldAttack();
        attacking = false;
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
            Time.deltaTime * agent.navAgent.angularSpeed);
    }

}
