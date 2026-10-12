using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAttack meleeAttack;
    private RangedEnemyAttack rangedAttack;
    private EnemyStateMachine stateMachine;

    private void Awake()
    {
        meleeAttack = GetComponentInParent<EnemyAttack>();
        rangedAttack = GetComponentInParent<RangedEnemyAttack>();
        stateMachine = GetComponentInParent<EnemyStateMachine>();
    }

    // Place at the impact/fire frame of the attack animation.
    public void AttackImpact()
    {
        if (meleeAttack != null && meleeAttack.IsAttacking)
        {
            meleeAttack.AnimationAttackImpact();
        }
        else if (rangedAttack != null && rangedAttack.IsAttacking)
        {
            rangedAttack.AnimationAttackImpact();
        }
    }

    // Place near the end of the attack animation.
    public void AttackFinished()
    {
        if (meleeAttack != null && meleeAttack.IsAttacking)
        {
            meleeAttack.AnimationAttackFinished();
        }

        if (rangedAttack != null && rangedAttack.IsAttacking)
        {
            rangedAttack.AnimationAttackFinished();
        }
    }

    // Place at the end of the death animation.
    public void DeathFinished()
    {
        if (stateMachine != null)
            stateMachine.FinishDeath();
    }
}
