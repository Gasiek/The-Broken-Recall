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

        if (owner == null || attackOrigin == null)
            return;

        float distance = Vector3.Distance(owner.position, target.position);

        if (distance > Definition.AttackRange)
            return;

        Collider[] hits = Physics.OverlapSphere(attackOrigin.position, Definition.AttackRadius);

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.transform.root != target.root)
                continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            IPushable pushable = hit.GetComponentInParent<IPushable>();

            if (damageable != null)
            {
                damageable.TakeDamage(Definition.Damage);

                if (Definition.MeleeImpactSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayWorldSFX(
                        Definition.MeleeImpactSound,
                        hit.ClosestPoint(attackOrigin.position),
                        Definition.MeleeImpactSoundVolume
                    );
                }
            }

            if (Definition.PushesTarget && Definition.PushDistance > 0f && pushable != null)
            {
                Vector3 pushDirection = target.position - owner.position;

                pushDirection.y = 0f;

                if (pushDirection.sqrMagnitude > 0.001f)
                {
                    pushable.Push(pushDirection.normalized, Definition.PushDistance);
                }
            }

            break;
        }
    }
}
