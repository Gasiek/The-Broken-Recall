using UnityEngine;

public class RangedEnemyAttack : MonoBehaviour
{
    [SerializeField]
    private Transform projectileSpawnPoint;

    private EnemyController enemy;

    private float cooldownTimer;

    public bool IsAttacking { get; private set; }

    public bool CanAttack => cooldownTimer <= 0f;

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

    public void TryAttack(Transform target)
    {
        if (!CanAttack || target == null)
        {
            return;
        }

        IsAttacking = true;

        cooldownTimer = enemy.Definition.AttackCooldown;

        FireProjectile(target);

        IsAttacking = false;
    }

    private void FireProjectile(Transform target)
    {
        if (projectileSpawnPoint == null)
        {
            Debug.LogError($"{gameObject.name} has no projectile spawn point.", gameObject);

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

        Vector3 direction = target.position - projectileSpawnPoint.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
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
