using UnityEngine;

public static class EnemyAttackFactory
{
    public static IEnemyAttack Create(
        AttackDefinition definition,
        Transform owner,
        Transform attackOrigin,
        Transform projectileSpawnPoint,
        EnemyAnimation enemyAnimation
    )
    {
        if (definition == null)
            return null;

        switch (definition.AttackType)
        {
            case EnemyAttackType.Melee:
                return new MeleeAttackRuntime(
                    definition,
                    owner,
                    attackOrigin,
                    projectileSpawnPoint,
                    enemyAnimation
                );

            case EnemyAttackType.Ranged:
                if (definition.ProjectilePrefab == null)
                {
                    Debug.LogWarning(
                        $"{owner.name}: Ranged attack "
                            + $"'{definition.name}' has no projectile prefab."
                    );

                    return null;
                }

                return new RangedAttackRuntime(
                    definition,
                    owner,
                    attackOrigin,
                    projectileSpawnPoint,
                    enemyAnimation
                );

            default:
                Debug.LogWarning(
                    $"{owner.name}: Unsupported attack type " + $"on '{definition.name}'."
                );

                return null;
        }
    }
}
