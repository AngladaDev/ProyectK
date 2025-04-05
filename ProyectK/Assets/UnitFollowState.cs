using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{
    AttackController attackController;
    NavMeshAgent agent;
    public float attackingDistance = 1f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackController = animator.transform.GetComponent<AttackController>();
        agent = animator.transform.GetComponent<NavMeshAgent>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Should Unit transition to Idle State?
        if (attackController.target == null)
        {
            animator.SetBool("isFollowing", false);
        }
        else
        {
            // If there is no direct order to move from the player
            if (animator.transform.GetComponent<UnitMovementController>().isCommanded == false)
            {
                // Set stopping distance to stay at a safe attack range
                agent.stoppingDistance = 1.5f;

                // Moving Unit towards Enemy
                agent.SetDestination(attackController.target.position);
                animator.transform.LookAt(attackController.target);

                // Should Unit transition to Attack State?
                float distanceFromTarget = Vector3.Distance(attackController.target.position, animator.transform.position);

                if (distanceFromTarget < attackingDistance)
                {
                    // Stop and prepare to attack
                    agent.SetDestination(animator.transform.position);
                    animator.SetBool("isAttacking", true); //Move to Attacking State
                }
            }
        }
    }
}
