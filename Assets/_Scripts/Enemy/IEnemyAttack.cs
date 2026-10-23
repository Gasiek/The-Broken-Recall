using UnityEngine;

public interface IEnemyAttack
{
    AttackDefinition Definition { get; }

    float AttackRange { get; }
    float MinimumRange { get; }

    bool IsAttacking { get; }
    bool IsSuperAttack { get; }
    bool CanAttack { get; }

    bool TryStartAttack(Transform target);
    void AnimationAttackImpact();
    void AnimationAttackFinished();
    void CancelAttack();
}
