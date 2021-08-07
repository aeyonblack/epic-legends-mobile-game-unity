using UnityEngine;

public class AttackData : ScriptableObject
{
    public float MinDamage;
    public float MaxDamage;
    public float AttackDistance;
    public float CoolDownSeconds;
    public float DamageAmplifier;
    public float CriticalChance;

    private const float DOT_THRESHOLD = 0.5f;

    public Attack CreateAttack()
    {
        float coreDamage = Random.Range(MinDamage, MaxDamage);
        bool critical = Random.Range(0, 100) < CriticalChance;
        coreDamage = critical ? coreDamage * DamageAmplifier : coreDamage;
        return new Attack(coreDamage, critical);
    }

    protected bool LookingAtTarget(Transform attacker, Transform target)
    {
        var dirToTarget = target.position - attacker.position;
        dirToTarget.Normalize();
        float dot = Vector3.Dot(attacker.forward, dirToTarget);
        return dot >= DOT_THRESHOLD;
    }
}
