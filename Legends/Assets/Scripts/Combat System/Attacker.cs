using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    public AttackData attack;

    private Transform target;

    public void AttackTheat(GameObject target)
    {
        this.target = target.transform;
        StartCoroutine(DoAttack());
    }

    public void HoldAttack()
    {
        StopAllCoroutines();
    }

    private IEnumerator DoAttack()
    {
        while (true)
        {
            GetComponent<CharacterData>().GetAnimator().SetTrigger("attack");
            OnAttack();
            yield return new WaitForSeconds(attack.CoolDownSeconds);
        }
    }

    public void Attack()
    {
        if (attack is HandCombat) (attack as HandCombat).Execute(transform, target);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void OnAttack()
    {
        if (target)
        {
            var legend = target.GetComponent<GoapLegend>();
            if (legend) legend.OnAttacked();
        }
    }

    public void AlertTarget()
    {
        if (target)
        {
            var agent = target.GetComponent<Agent>();
            if (agent) agent.OnAttacked();
        }
    }
}
