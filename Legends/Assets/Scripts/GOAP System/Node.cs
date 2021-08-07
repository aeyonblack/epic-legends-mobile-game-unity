using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Node parent;
    public float cost;
    public GoapAction action;
    public Dictionary<string, int> states;

    public Node(Node parent, float cost, GoapAction action,
        Dictionary<string, int> states)
    {
        this.parent = parent;
        this.cost = cost;
        this.action = action;
        this.states = new Dictionary<string, int>(states);
    }

    public Node(Node parent, float cost, GoapAction action, 
        Dictionary<string, int> states, Dictionary<string, int> agentBeliefs)
    {
        this.parent = parent;
        this.cost = cost;
        this.action = action;
        this.states = new Dictionary<string, int>(states);
        ExtendStates(agentBeliefs);
    }

    private void ExtendStates(Dictionary<string,int> agentBeliefs)
    {
        foreach (KeyValuePair<string, int> belief in agentBeliefs)
        {
            if (!states.ContainsKey(belief.Key))
            {
                states.Add(belief.Key, belief.Value);
            }
        }
    }
}
