using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Epic Legends/Create/Weapon")]
public class Weapon : Collectable
{
    public float damage;

    public float fireRate;

    public string controllerName;

    public override bool UsedBy(CharacterData user)
    {
        return user.Equip(this);
    }
}
