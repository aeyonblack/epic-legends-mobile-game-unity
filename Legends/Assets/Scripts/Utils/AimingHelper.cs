using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AimingHelper : MonoBehaviour
{
    public bool isProjection = false;

    public bool isEnabled = false;

    public virtual void Enable() { }

    public virtual void Disable() { }

}
