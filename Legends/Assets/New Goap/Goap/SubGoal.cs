using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubGoal
{
    public Dictionary<string, int> subGoals;

    public bool shouldRemove;

    public string goal;

    public SubGoal(string goal, int priority, bool shouldRemove)
    {
        subGoals = new Dictionary<string, int>();
        subGoals.Add(goal, priority);
        this.shouldRemove = shouldRemove;
        this.goal = goal;
    }
}
