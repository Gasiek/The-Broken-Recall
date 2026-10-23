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
        Vector3 aimPoint = target.position;

        Player player = target.GetComponent<Player>();

        if (player != null)
            aimPoint = player.AimPosition;

        Vector3 direction;

        switch (Definition.AimMode)
        {
            case ProjectileAimMode.SpawnPointForward:
            {
                Vector3 forward = projectileSpawnPoint.forward;
                forward.y = 0f;

                if (forward.sqrMagnitude <= 0.001f)
                    return;

                forward.Normalize();

                Vector3 toTarget = aimPoint - spawnPosition;

                float horizontalDistance = Vector3.Dot(
                    new Vector3(toTarget.x, 0f, toTarget.z),
                    forward
                );

                horizontalDistance = Mathf.Max(horizontalDistance, 0.1f);

                float verticalAngle = Mathf.Atan2(toTarget.y, horizontalDistance);

                direction = (
                    forward * Mathf.Cos(verticalAngle) + Vector3.up * Mathf.Sin(verticalAngle)
                ).normalized;

                break;
            }

            case ProjectileAimMode.Target:
            default:
                direction = aimPoint - spawnPosition;
                break;
        }

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        GameObject projectileObject = Object.Instantiate(
            Definition.ProjectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        if (Definition.ProjectileLaunchSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(
                Definition.ProjectileLaunchSound,
                spawnPosition,
                Definition.ProjectileLaunchSoundVolume
            );
        }

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
