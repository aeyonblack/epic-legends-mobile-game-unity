using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class GoapAgent : MonoBehaviour
{
    [Header("World Bounds")]
    public float xmin = 70;
    public float xmax = 160;
    public float zmin = -70;
    public float zmax = 20;

    public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();

    public WorldStates beliefs = new WorldStates();

    public GoapWeapon Weapon { get; protected set; }

    [HideInInspector]
    public List<GoapAction> actions = new List<GoapAction>();

    [HideInInspector]
    public GoapAction currentAction;

    [HideInInspector]
    public GameObject randomTarget;

    [HideInInspector]
    public CharacterData character;

    [HideInInspector]
    public NavMeshAgent navAgent;

    private Planner planner;

    private Queue<GoapAction> actionQueue;

    protected SubGoal currentGoal;

    public void Start()
    {
        GetActions();
        SetupAgent();
    }

    private void GetActions()
    {
        GoapAction[] agentActions = GetComponents<GoapAction>();
        foreach (GoapAction action in agentActions)
        {
            actions.Add(action);
        }
    }

    private void SetupAgent()
    {
        character = GetComponent<CharacterData>();
        character.Init();
        navAgent = character.GetNavAgent();
        randomTarget = new GameObject("TARGET");
    }

    private void LateUpdate()
    {
        if (currentAction != null && currentAction.running) return;

        if (planner == null || actionQueue == null) CreatePlan();

        if (actionQueue != null && actionQueue.Count == 0) ErasePlan();

        if (actionQueue != null && actionQueue.Count > 0) PerformAction();
    }

    public void Move(GoapAction action)
    {
        if (action.target)
        {
            navAgent.SetDestination(action.target.transform.position);
        }
    } 

    private void CreatePlan()
    {
        planner = new Planner();

        var sortedGoals = from entry in goals orderby entry.Value descending select entry;
        foreach (KeyValuePair<SubGoal, int> goal in sortedGoals)
        {
            actionQueue = planner.Plan(actions, goal.Key.subGoals, beliefs);
            if (actionQueue != null)
            {
                currentGoal = goal.Key;
                break;
            }
        }
    }

    private void ErasePlan()
    {
        if (currentGoal.shouldRemove)
        {
            goals.Remove(currentGoal);
        }
        planner = null;
    }

    private void PerformAction()
    {
        currentAction = actionQueue.Dequeue();
        if (currentAction.PrePerform())
        {
            if (currentAction.target && currentAction.target.activeInHierarchy)
            {
                currentAction.running = true;
            }
        }
        else
        {
            actionQueue = null;
        }
    }

    private void OnDestroy()
    {
        Destroy(randomTarget);
    }

}
