using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField]
    private EnemyStateMachine stateMachine;

    private void Awake()
    {
        if (stateMachine == null)
        {
            Debug.LogError($"{name}: No EnemyStateMachine found.", this);
        }
    }

    public void AttackImpact()
    {
        stateMachine.AnimationAttackImpact();
    }

    public void AttackFinished()
    {
        stateMachine.AnimationAttackFinished();
    }

    public void DeathFinished()
    {
        stateMachine.FinishDeath();
    }
}
