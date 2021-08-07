using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Agent : MonoBehaviour
{
    public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();

    public WorldStates beliefs = new WorldStates();

    [HideInInspector]
    public List<AgentAction> actions = new List<AgentAction>();

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public Health health;

    [HideInInspector]
    public AgentAction currentAction;

    [HideInInspector]
    public AgentAction nextAction;

    [HideInInspector]
    public AgentWeapon weaponController;

    [HideInInspector]
    public CharacterData character;

    [HideInInspector]
    public GameObject randomTarget;

    [HideInInspector]
    public GameObject currentTarget;

    private Planner planner;

    private Queue<AgentAction> actionQueue;

    private SubGoal currentGoal;

    public void Start()
    {
        Init();
        character = GetComponent<CharacterData>();
        character.Init();
        animator = character.GetAnimator();
        randomTarget = new GameObject("target");
    }

    private void FixedUpdate()
    {
        if (!currentAction) return;
        if (currentAction.navAgent) character.velocity = currentAction.navAgent.velocity.magnitude;
    }

    private void LateUpdate()
    {
        if (currentAction != null && currentAction.running) return;

        if (planner == null || actionQueue == null) CreatePlan();

        if (actionQueue != null && actionQueue.Count == 0) ErasePlan();

        if (actionQueue != null && actionQueue.Count > 0) PerformAction();
    }

    private void Init()
    {
        AgentAction[] agentActions = GetComponents<AgentAction>();
        foreach (AgentAction action in agentActions)
        {
            actions.Add(action);
        }
    }
    
    private void CreatePlan()
    {
        planner = new Planner();

        var sortedGoals = from entry in goals orderby entry.Value descending select entry; 
        foreach (KeyValuePair<SubGoal, int> goal in sortedGoals)
        {
            //actionQueue = planner.Plan(actions, goal.Key.subGoals, beliefs);
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
        if (actionQueue.Count > 0)
            nextAction = actionQueue.Peek();
        if (currentAction.PrePerform())
        {
            if (currentAction.target != null)
            {
                currentAction.running = true;
            }
        }
        else
        {
            actionQueue = null;
        }
    }

    public void StartAttack()
    {
        weaponController.StartAttack(currentAction.target);
    }

    public void StopAttack()
    {
        weaponController.StopAttack();
    }

    public void UpdateWeaponController()
    {
        weaponController = character.WeaponPlace
            .GetChild(character.CurrentWeaponId)
            .GetComponent<AgentWeapon>();
    }

    public void OnAttacked()
    {
        if (AvoidingDeath()) return;
        if (gameObject.tag == "Glopnop") return;
        StartCoroutine("ChangePlan");
    }

    private IEnumerator ChangePlan()
    {
        yield return new WaitForSeconds(Random.Range(2, 15));
        if (!currentAction.preconditionsMap.ContainsKey("safe"))
        {
            if (!beliefs.HasState("in danger"))
            {
                if (beliefs.HasState("weapon equipped") && 
                    currentAction.preconditionsMap.ContainsKey("weapon found"))
                {
                    currentAction.preconditionsMap.Remove("weapon found");
                }
                currentAction.preconditionsMap.Add("safe", 0);
                beliefs.ModifyState("in danger", 0);
                currentAction.PostPerform();
                CreatePlan();
            }
        }
    }

    public void OnEnemyKilled(Loot medKit)
    {
        if (!currentAction.preconditionsMap.ContainsKey("healthy"))
        {
            if (!beliefs.HasState("found medkit"))
            {
                currentTarget = medKit.gameObject;
                currentAction.preconditionsMap.Add("healthy", 0);
                beliefs.ModifyState("found medkit", 0);
                currentAction.PostPerform();
                CreatePlan();
            }
        }
    }

    public bool AvoidingDeath()
    {
        return currentAction.actionName == "Run away" || currentAction.name == "Hide" 
            || currentAction.name == "Grab medkit"; 
    }

}
