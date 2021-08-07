using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glopnop : Agent
{
    new void Start()
    {
        base.Start();

        SubGoal goal = new SubGoal("enemy killed", 1, false);
        goals.Add(goal, 1);
    }

}
