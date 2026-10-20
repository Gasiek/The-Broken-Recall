using UnityEngine;

public abstract class EnemyAttackRuntime : IEnemyAttack
{
    protected readonly Transform owner;
    protected readonly Transform attackOrigin;
    protected readonly Transform projectileSpawnPoint;
    protected readonly EnemyAnimation enemyAnimation;

    protected Transform target;
    protected float nextAttackTime;
    protected bool isAttacking;

    public AttackDefinition Definition { get; }

    public float AttackRange => Definition != null ? Definition.AttackRange : 0f;

    public float MinimumRange => Definition != null ? Definition.MinimumRange : 0f;

    public bool IsAttacking => isAttacking;

    public bool CanAttack => Definition != null && !isAttacking && Time.time >= nextAttackTime;

    public bool IsSuperAttack => Definition != null && Definition.IsSuperAttack;

    protected EnemyAttackRuntime(
        AttackDefinition definition,
        Transform owner,
        Transform attackOrigin,
        Transform projectileSpawnPoint,
        EnemyAnimation enemyAnimation
    )
    {
        Definition = definition;
        this.owner = owner;
        this.attackOrigin = attackOrigin != null ? attackOrigin : owner;
        this.projectileSpawnPoint =
            projectileSpawnPoint != null ? projectileSpawnPoint : this.attackOrigin;
        this.enemyAnimation = enemyAnimation;
    }

    public bool TryStartAttack(Transform newTarget)
    {
        if (!CanAttack || newTarget == null)
            return false;

        target = newTarget;
        isAttacking = true;
        if (Definition.IsSuperAttack)
        {
            nextAttackTime = Time.time + Definition.SuperAttackCooldown;
        }

        if (enemyAnimation != null)
            enemyAnimation.PlayAttack(Definition.AnimationTrigger);

        return true;
    }

    public abstract void AnimationAttackImpact();

    public virtual void AnimationAttackFinished()
    {
        isAttacking = false;
        target = null;
    }

    public virtual void CancelAttack()
    {
        isAttacking = false;
        target = null;
    }
}
