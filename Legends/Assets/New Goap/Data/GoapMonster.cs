using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapMonster : GoapAgent
{
    new void Start()
    {
        base.Start();

        goals.Add(new SubGoal("threatsAverted", 1, false), 1);
    }

}
