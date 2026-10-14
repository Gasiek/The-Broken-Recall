using UnityEngine;

public class MeleeAttackRuntime : EnemyAttackRuntime
{
    public MeleeAttackRuntime(
        AttackDefinition definition,
        Transform owner,
        Transform attackOrigin,
        Transform projectileSpawnPoint,
        EnemyAnimation enemyAnimation
    )
        : base(definition, owner, attackOrigin, projectileSpawnPoint, enemyAnimation) { }

    public override void AnimationAttackImpact()
    {
        if (!isAttacking || target == null || Definition == null)
            return;

        if (attackOrigin == null)
            return;

        float distance = Vector3.Distance(owner.position, target.position);

        if (distance > Definition.AttackRange)
            return;

        Collider[] hits = Physics.OverlapSphere(attackOrigin.position, Definition.AttackRadius);

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null)
                continue;

            Transform hitRoot = hit.transform.root;
            Transform targetRoot = target.root;

            if (hitRoot != targetRoot)
                continue;

            damageable.TakeDamage(Definition.Damage);
            break;
        }
    }
}
