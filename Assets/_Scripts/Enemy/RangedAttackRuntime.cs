using UnityEngine;

public class RangedAttackRuntime : EnemyAttackRuntime
{
    public RangedAttackRuntime(
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

        if (Definition.ProjectilePrefab == null)
        {
            Debug.LogWarning(
                $"{owner.name}: Attack '{Definition.name}' " + "has no projectile prefab."
            );
            return;
        }

        Vector3 spawnPosition = projectileSpawnPoint.position;
        Vector3 direction = target.position - spawnPosition;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        GameObject projectileObject = Object.Instantiate(
            Definition.ProjectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError(
                $"Projectile prefab '{Definition.ProjectilePrefab.name}' "
                    + "does not have a Projectile component."
            );

            Object.Destroy(projectileObject);
            return;
        }

        projectile.Initialize(
            direction,
            Definition.ProjectileSpeed,
            Definition.Damage,
            Definition.ProjectileLifetime
        );
    }
}
