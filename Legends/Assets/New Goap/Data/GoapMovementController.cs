using UnityEngine;
using UnityEngine.AI;

public class GoapMovementController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        bool moving = agent.velocity.magnitude != 0;
        int weight = moving ? 1 : 0;
        animator.SetBool("moving", moving);
        animator.SetLayerWeight(1, weight);
    }

}
