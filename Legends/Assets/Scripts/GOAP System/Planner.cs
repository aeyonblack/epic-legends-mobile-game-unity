using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Develops a plan for achieving a specified goal
/// </summary>
public class Planner
{
    public Queue<GoapAction> Plan(List<GoapAction> actions, 
        Dictionary<string, int> goal, WorldStates agentBeliefs)
    {
        foreach (GoapAction a in actions)
        {
            a.DoReset();
        }

        List<GoapAction> usableActions = new List<GoapAction>();
        foreach (GoapAction action in actions)
        {
            if (action.IsAchievable())
            {
                usableActions.Add(action);
            }
        }

        List<Node> leaves = new List<Node>();
        Node start = new Node(null, 0, null, World.Instance
            .GetWorld()
            .GetStates(), agentBeliefs.GetStates());

        bool success = BuildGraph(start, leaves, usableActions, goal);

        if (!success)
        {
            Debug.Log("NO PLAN");
            return null;
        }

        Node cheapest = null;
        foreach (Node leaf in leaves)
        {
            if (cheapest == null) cheapest = leaf;
            else
            {
                if (leaf.cost < cheapest.cost) cheapest = leaf;
            }
        }

        List<GoapAction> result = new List<GoapAction>();
        Node node = cheapest;
        while (node != null)
        {
            if (node.action != null)
            {
                result.Insert(0, node.action);
            }
            node = node.parent;
        }

        Queue<GoapAction> queue = new Queue<GoapAction>();
        foreach (GoapAction action in result)
        {
            queue.Enqueue(action);
        }

        Debug.Log("THE PLAN IS\n--------------------");
        foreach (GoapAction action in queue)
        {
            Debug.Log("Q " + action.actionName);
        }

        return queue;
    }

    private bool BuildGraph(Node parent, List<Node> leaves, 
        List<GoapAction> usableActions, Dictionary<string, int> goal)
    {
        bool foundPath = false;
        foreach (GoapAction action in usableActions)
        {
            if (action.IsAchievableGiven(parent.states))
            {
                Dictionary<string, int> currentState = new Dictionary<string, int>(parent.states);
                foreach (KeyValuePair<string, int> effect in action.effectsMap)
                {
                    if (!currentState.ContainsKey(effect.Key))
                    {
                        currentState.Add(effect.Key, effect.Value);
                    }
                }

                Node node = new Node(parent, parent.cost + action.cost, action, currentState);

                if (GoalAchieved(goal,currentState))
                {
                    leaves.Add(node);
                    foundPath = true;
                }
                else
                {
                    List<GoapAction> subset = ActionSubset(usableActions, action);
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found) foundPath = true;
                }
            }
        }
        return foundPath;
    }

    private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> state)
    {
        foreach (KeyValuePair<string, int> g in goal)
        {
            if (!state.ContainsKey(g.Key)) return false;
        }
        return true;
    }

    private List<GoapAction> ActionSubset(List<GoapAction> actions, GoapAction actionToRemove)
    {
        List<GoapAction> subset = new List<GoapAction>();
        foreach (GoapAction action in actions)
        {
            if (!action.Equals(actionToRemove)) subset.Add(action);
        }
        return subset;
    }
}
