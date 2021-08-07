using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AgentAction : MonoBehaviour
{
    [Header("Action Settings")]
    public string actionName = "Action";
    public int cost = 1;
    public LayerMask targetMask;

    [Header("Preconditions and Effects")]
    public WorldState[] preconditions;
    public WorldState[] effects;

    [Header("World Bounds")]
    public float xMin = -16;
    public float xMax = 144;
    public float zMin = -138;
    public float zMax = 25;

    [HideInInspector]
    public GameObject target;

    [HideInInspector]
    public Dictionary<string, int> preconditionsMap;

    [HideInInspector]
    public Dictionary<string, int> effectsMap;

    [HideInInspector]
    public FieldOfView fov;

    [HideInInspector]
    public NavMeshAgent navAgent;

    [HideInInspector]
    public bool running = false;

    protected GameObject randomTarget;

    protected Agent agent;

    public AgentAction()
    {
        preconditionsMap = new Dictionary<string, int>();
        effectsMap = new Dictionary<string, int>();
    }

    public abstract bool PrePerform();


    public void Awake()
    {
        agent = GetComponent<Agent>();
        navAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        MapStates();
    }

    public void FixedUpdate()
    {
        Animate();
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

    public bool IsAchievable()
    {
        return true;
    }

    public bool IsAchievableGiven(Dictionary<string,int> preconditions)
    {
        foreach (KeyValuePair<string, int> condition in preconditionsMap)
        {
            if (!preconditions.ContainsKey(condition.Key)) return false;
        }
        return true;
    }

    private GameObject GenerateRandomTarget()
    {
        float x = Random.Range(xMin, xMax);
        float z = Random.Range(zMin, zMax);
        randomTarget.transform.position = new Vector3(x, 2, z);
        return randomTarget;
    }

    protected GameObject RandomTarget()
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

    protected virtual void Animate()
    {
        agent.animator.SetBool("moving", navAgent.velocity.magnitude != 0);
    }

    public abstract bool PostPerform();
}
