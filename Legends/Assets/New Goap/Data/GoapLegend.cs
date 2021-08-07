using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapLegend : GoapAgent
{

    private bool evading = false;

    private SubGoal hasWeapon;
    private SubGoal enemyKilled;
    private SubGoal attackEvaded;
    private SubGoal hasGoodHealth;

    private const string LOW_HEALTH = "hasLowHealth";

    new void Start()
    {
        base.Start();

        CreateGoals();
        goals.Add(hasWeapon, 1);
        goals.Add(enemyKilled, 1);

        SubscribeToEvents();
    }

    private void CreateGoals()
    {
        hasWeapon = new SubGoal("hasWeapon", 1, true);
        enemyKilled = new SubGoal("enemyKilled", 1, false);
        attackEvaded = new SubGoal("attackEvaded", 1, true);
        hasGoodHealth = new SubGoal("hasGoodHealth", 1, true);
    }

    private void SubscribeToEvents()
    {
        // update the agent's weapon
        character.weaponUpdated += () =>
        {
            Weapon = character.WeaponPlace
            .GetChild(character.CurrentWeaponId)
            .GetComponent<GoapWeapon>();
        };

        // listen for changes to the agent's health
        character.health.LowHealth += () =>
        {
            if (!beliefs.HasState(LOW_HEALTH))
            {
                beliefs.ModifyState(LOW_HEALTH, 1);
            }
        };
    }

    public void OnTargetDead()
    {
        StopAllCoroutines();
        if (evading) evading = false;
        if (beliefs.HasState(LOW_HEALTH))
        {
            if (!goals.ContainsKey(hasGoodHealth))
            {
                goals.Add(hasGoodHealth, 2);
            }
        }
        currentAction.DoReset();
    }

    public void OnAttacked()
    {
        if (!(currentGoal.goal.Equals("attackEvaded") || evading ||
            currentGoal.goal.Equals("hasWeapon") || currentGoal.Equals("hasGoodHealth")))
        {
            StartCoroutine(TryEvade());
        }
    }

    private IEnumerator TryEvade()
    {
        evading = true;

        float delay = Random.Range(1, 4);
        if (!goals.ContainsKey(attackEvaded)) goals.Add(attackEvaded, 1);

        yield return new WaitForSeconds(delay);
        currentAction.DoReset();

        yield return new WaitForSeconds(1);
        evading = false;
    }
}
