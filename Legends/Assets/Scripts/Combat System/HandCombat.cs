using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="handcombat", menuName ="Epic Legends/Create/HandCombat Attack")]
public class HandCombat : AttackData
{
    public void Execute(Transform attacker, Transform target)
    {
        if (target == null) return;
        if (Vector3.Distance(attacker.position, target.position) > AttackDistance) return;

        var attack = CreateAttack();
        var childHealth = target.GetComponent<Health>();
        var health = childHealth ? childHealth : target.GetComponentInParent<Health>();
        health.TakeDamage(attack.Damage);
    }
}
