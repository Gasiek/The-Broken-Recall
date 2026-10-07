using System.Collections;
using UnityEngine;

public class RangedEnemyAttack : MonoBehaviour
{
    [SerializeField]
    private Transform projectileSpawnPoint;

    private EnemyController enemy;

    private float cooldownTimer;

    public bool IsAttacking { get; private set; }

    public bool CanAttack => cooldownTimer <= 0f && !IsAttacking;

    public float AttackRange => enemy.Definition.RangedAttackRange;

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public bool TryAttack(Transform target)
    {
        if (!CanAttack || target == null)
        {
            return false;
        }

        StartCoroutine(AttackRoutine(target));

        return true;
    }

    private IEnumerator AttackRoutine(Transform target)
    {
        IsAttacking = true;

        // Start cooldown when the attack is committed.
        cooldownTimer = enemy.Definition.AttackCooldown;

        // Wait for the attack delay.
        // For ranged enemies this can be 0.
        yield return new WaitForSeconds(enemy.Definition.AttackDelay);

        // Check that the target still exists before firing.
        if (target != null)
        {
            FireProjectile(target);
        }

        IsAttacking = false;
    }

    private void FireProjectile(Transform target)
    {
        if (projectileSpawnPoint == null)
        {
            Debug.LogError($"{gameObject.name} has no projectile spawn point.", gameObject);

            return;
        }

        if (enemy.Definition.ProjectilePrefab == null)
        {
            Debug.LogError($"{gameObject.name} has no projectile prefab.", gameObject);

            return;
        }

        Vector3 direction = target.position - projectileSpawnPoint.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        GameObject projectileObject = Instantiate(
            enemy.Definition.ProjectilePrefab,
            projectileSpawnPoint.position,
            projectileSpawnPoint.rotation
        );

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError("Projectile prefab is missing Projectile component.", projectileObject);

            Destroy(projectileObject);
            return;
        }

        projectile.Initialize(
            direction.normalized,
            enemy.Definition.ProjectileSpeed,
            enemy.Definition.AttackDamage,
            enemy.Definition.ProjectileLifetime
        );
    }
}
