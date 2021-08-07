using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Legend : Agent
{
    new void Start()
    {
        base.Start();
        SubGoal enemyDead = new SubGoal("enemy dead", 1, false);
        goals.Add(enemyDead, 1);

        SubGoal safe = new SubGoal("safe", 1, false);
        goals.Add(safe, 1);

        SubGoal healthy = new SubGoal("healthy", 1, false);
        goals.Add(healthy, 1);
    }

}
