using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GoapAction : MonoBehaviour
{
    [Header("Configuration")]
    public string actionName = "Action";
    public int cost = 1;
    public LayerMask targetMask;
    public WorldState[] preconditions;
    public WorldState[] effects;

    [HideInInspector]
    public GameObject target;

    [HideInInspector]
    public Dictionary<string, int> preconditionsMap;

    [HideInInspector]
    public Dictionary<string, int> effectsMap;

    [HideInInspector]
    public FieldOfView fov;

    [HideInInspector]
    public bool running = false;

    protected GoapAgent agent;

    protected float distanceToTarget;

    public GoapAction()
    {
        preconditionsMap = new Dictionary<string, int>();
        effectsMap = new Dictionary<string, int>();
    }

    public abstract bool PrePerform();

    public abstract void DoReset();

    private void Awake()
    {
        agent = GetComponent<GoapAgent>();
        fov = GetComponent<FieldOfView>();
    }

    private void Start()
    {
        MapStates();
    }

    private void MapStates()
    {
        if (preconditions != null)
        {
            foreach (WorldState condition in preconditions)
            {
                preconditionsMap.Add(condition.key, condition.value);
            }
        }

        if (effects != null)
        {
            foreach (WorldState effect in effects)
            {
                effectsMap.Add(effect.key, effect.value);
            }
        }
    }

    public void RemapStates()
    {
        preconditionsMap = new Dictionary<string, int>();
        effectsMap = new Dictionary<string, int>();
        MapStates();
    }

    public bool IsAchievable()
    {
        return true;
    }

    public bool IsAchievableGiven(Dictionary<string, int> preconditions)
    {
        foreach (KeyValuePair<string, int> condition in preconditionsMap)
        {
            if (!preconditions.ContainsKey(condition.Key)) return false;
        }
        return true;
    }

    protected GameObject GetRandomTarget()
    {
        while (true)
        {
            GameObject randomTarget = GenerateRandomTarget();
            Collider[] c = Physics.OverlapSphere(randomTarget.transform.position, 1.5f);
            if (c.Length == 0)
            {
                return randomTarget;
            }
        }
    }

    private GameObject GenerateRandomTarget()
    {
        float x = Random.Range(agent.xmin, agent.xmax);
        float z = Random.Range(agent.zmin, agent.zmax);
        agent.randomTarget.transform.position = new Vector3(x, 2, z);
        return agent.randomTarget;
    }

}
