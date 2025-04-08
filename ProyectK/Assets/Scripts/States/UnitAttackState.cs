using System;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    private NavMeshAgent agent;
    private AttackController attackController;

    public float stopAttackingDistante = 1.2f;

    public float attackRate = 2f;

    public float attackTimer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        attackController = animator.GetComponent<AttackController>();

        //Debuging material
        attackController.SetAttackMaterial();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackController.target != null && animator.transform.GetComponent<UnitMovementController>().isCommanded == false)
        {
            LookAtTarget();

            // Move towards enemy

            // agent.SetDestination(attackController.target.position);

            // Attack logic

            if (attackTimer <= 0)
            {
                Attack();
                attackTimer = 1f / attackRate;
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }

            // Should unit still attack

            float distanceFromTarget = Vector3.Distance(attackController.target.position, animator.transform.position);

            if (distanceFromTarget > stopAttackingDistante || attackController.target == null)
            {
                animator.SetBool("isAttack", false); //Move to Follow State
            }
        }
        else
        {
            // If target is null (there is not target) the unit change to follow state
            animator.SetBool("isAttack", false);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    private void LookAtTarget()
    {
        Vector3 direction = attackController.target.position - agent.transform.position;
        agent.transform.rotation = Quaternion.LookRotation(direction);

        var yRotation = agent.transform.eulerAngles.y;
        agent.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void Attack()
    {
        var damageToInflic = attackController.unitDamage;

        attackController.target.GetComponent<Unit>().TakeDamge(damageToInflic);
    }
}
