using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField]
    private EnemyStateMachine stateMachine;

    private void Awake()
    {
        if (stateMachine == null)
            Debug.LogError($"{name}: No EnemyStateMachine found.", this);
    }

    public void AttackImpact()
    {
        if (stateMachine == null)
            return;

        stateMachine.AnimationAttackImpact();
    }

    public void AttackFinished()
    {
        if (stateMachine == null)
            return;

        stateMachine.AnimationAttackFinished();
    }

    public void DeathFinished()
    {
        if (stateMachine == null)
            return;

        stateMachine.FinishDeath();
    }
}
