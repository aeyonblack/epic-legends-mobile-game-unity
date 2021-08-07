using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipWeaponAction : GoapAction
{
    private bool looting = false;

    public override void DoReset()
    {
        running = false;
        looting = false;
        StopAllCoroutines();
    }

    public override bool PrePerform()
    {
        agent.character.GetNavAgent().stoppingDistance = 1f;
        FindWeapon();
        StartCoroutine(PerformAction());
        return true;
    }

    private IEnumerator PerformAction()
    {
        while (true)
        {
            if (target == null) FindWeapon();
            else GetWeapon();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void FindWeapon()
    {
        Queue<GameObject> weapons = fov.FindVisibleTargets(this, true);
        target = weapons.Count == 0 ? GetRandomTarget() : weapons.Dequeue();
    }

    private void GetWeapon()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if (target.layer != LayerMask.NameToLayer("Collectable"))
        {
            if (looting)
            {
                StopAllCoroutines();
                looting = false;
            }
            SearchArea();
        }
        else
        {
            TryPickup();
        }
   
        agent.Move(this);
    }

    private void SearchArea()
    {
        if (distanceToTarget < 6f) FindWeapon();
        else
        {
            Queue<GameObject> weapons = fov.FindVisibleTargets(this, true);
            if (weapons.Count > 0) target = weapons.Dequeue();
        }
    }

    private IEnumerator Pickup(Loot weapon )
    {
        looting = true;
        yield return weapon.InteractWithDelay(agent.character);
        agent.beliefs.ModifyState("weaponEquipped", 1);
        DoReset();
    }

    private void TryPickup()
    {
        Loot weapon = target.GetComponentInParent<Loot>();
        if (weapon != null)
        {
            if (!(weapon.legendLooting || weapon.playerLooting))
            {
                if (distanceToTarget < weapon.aggroRange)
                {
                    if (!looting)
                    {
                        StartCoroutine(Pickup(weapon));
                    }
                }
            }
        }
        else
        {
            if (looting)
            {
                StopAllCoroutines();
                looting = false;
            }
            FindWeapon();
        }
    }

}
