using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public float aggroRange;

    public abstract void InteractWith(CharacterData character);

    public abstract IEnumerator InteractWithDelay(CharacterData character);
}
