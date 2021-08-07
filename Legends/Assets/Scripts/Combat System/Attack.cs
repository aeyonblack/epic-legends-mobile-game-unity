using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack 
{
    private readonly float damage;
    private readonly bool critical;

    public Attack(float damage, bool critical)
    {
        this.damage = damage;
        this.critical = critical;
    }

    public float Damage
    {
        get { return damage; }
    }
}
