using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindWeapon : AgentAction
{

    private float distanceToTarget;

    public override bool PrePerform()
    {
        if (!randomTarget) randomTarget = GetComponent<Agent>().randomTarget;
        navAgent.stoppingDistance = 1f;
        FindTarget();
        return true;
    }

    private void Update()
    {
        if (running)
        {
            if (GetComponent<Agent>().character.CurrentWeapon != null) running = false;
            if (!target) FindTarget();
            else FollowTarget();
        }
    }

    private void FollowTarget()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (target.layer != LayerMask.NameToLayer("Lootable"))
        {
            SearchNearestTargets();
        }
        else
        {
            TryPickup();
        }
        navAgent.SetDestination(target.transform.position);
    }

    private void FindTarget()
    {
        Queue<GameObject> targetObjects = fov.FindNearestVisibleTargets(this,true);
        target = targetObjects.Count == 0 ? RandomTarget() 
            : targetObjects.Dequeue();
    } 

    private void SearchNearestTargets()
    {
        if (distanceToTarget <= 5f || !navAgent.hasPath)
        {
            FindTarget();
        }
        else
        {
            Queue<GameObject> targetObjects = fov.FindNearestVisibleTargets(this,true);
            if (targetObjects.Count != 0)
            {
                target = targetObjects.Dequeue();
            }
        }
    }

    private void TryPickup()
    {
        Loot weapon = target.GetComponentInParent<Loot>();
        if (distanceToTarget < weapon.aggroRange)
        {
            weapon.InteractWith(agent.character);
            PostPerform();
        }
    }

    public override bool PostPerform()
    {
        agent.beliefs.ModifyState("weapon equipped", 0);
        running = false;
        return true;
    }


}
