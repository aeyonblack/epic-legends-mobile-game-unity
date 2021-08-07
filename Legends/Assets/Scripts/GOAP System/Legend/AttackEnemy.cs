using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modify this whole script to work properly
/// make sure actions are proper
/// 
/// </summary>
public class AttackEnemy : AgentAction
{

    private bool attacking = false;

    private float distanceToTarget;

    private float stoppingDistance;

    public override bool PrePerform()
    {
        if (!randomTarget) randomTarget = agent.randomTarget;
        stoppingDistance = Random.Range(8, 12);
        navAgent.stoppingDistance = stoppingDistance;
        FindEnemy();
        StartCoroutine("SearchNearestTarget", 5);
        return true;
    }

    private void Update()
    {
        if (running)
        {
            if (!target)
            {
                if (attacking) StopAttacking();
                FindEnemy();
            }
            else
            {
                ChaseEnemy();
            }
        }
    }

    private void FindEnemy()
    {
        Queue<GameObject> targets = fov.FindNearestVisibleTargets(this, true);
        target = targets.Count == 0 ? RandomTarget() : targets.Dequeue();
    }

    private void ChaseEnemy()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (target.layer != LayerMask.NameToLayer("Character"))
        {
            if (attacking) StopAttacking();
            if (distanceToTarget <= 13f || !navAgent.hasPath) FindEnemy();
            else GetTargetFromQueue();
        }
        else
        {
            DoGunAttack();
        }
        navAgent.SetDestination(target.transform.position);
    }

    private void Attack()
    {
        if (!attacking)
        {
            attacking = true;
        }
        agent.StartAttack();
    }

    private void DoGunAttack()
    {
        if (distanceToTarget <= stoppingDistance + 2)
        {
            LookAtTarget();
            if (distanceToTarget > 12)
            {
                if (attacking) StopAttacking();
                return;
            }

            if (!attacking) Attack();
        }
        else
        {
            StopAttacking();
        }
    }

    private void StopAttacking()
    {
        if (attacking)
        {
            agent.StopAttack();
            attacking = false;
        }
    }

    private IEnumerator SearchNearestTarget(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            if (fov.FindNearestVisibleTargets(this,true).Count > 0)
            {
                target = fov.FindNearestVisibleTargets(this,true).Dequeue();
            }
        }
    }

    private void StopSearching()
    {
        StopCoroutine("SearchNearestTarget");
    }

    private void LookAtTarget()
    {
        CharacterData targetCharacter = target.GetComponent<CharacterData>() != null ? 
            target.GetComponent<CharacterData>() : target.GetComponentInParent<CharacterData>();

        Vector3 targetPosition = targetCharacter.velocity > 1f && targetCharacter.OffsetTarget ? 
            targetCharacter.OffsetTarget.position : target.transform.position;

        Vector3 direction = (targetPosition - transform.position).normalized;

        Quaternion lookRotation = Quaternion.identity;

        if (direction != Vector3.zero)
        {
            lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 
            Time.deltaTime * navAgent.angularSpeed);
    }

    private void GetTargetFromQueue()
    {
        Queue<GameObject> targets = fov.FindNearestVisibleTargets(this,true);
        if (targets.Count != 0)
            target = targets.Dequeue();
    }

    public override bool PostPerform()
    {
        StopAttacking();
        StopSearching();
        running = false;
        return true;
    }

    
}
