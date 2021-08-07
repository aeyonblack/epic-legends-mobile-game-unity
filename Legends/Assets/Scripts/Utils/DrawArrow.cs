using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawArrow : AimingHelper
{
    public override void Disable()
    {
        gameObject.SetActive(false);
        isEnabled = false;
    }

    public override void Enable()
    {
        gameObject.SetActive(true);
        isEnabled = true;
    }

}
