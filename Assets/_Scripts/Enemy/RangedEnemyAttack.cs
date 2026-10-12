using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyAnimation))]
public class RangedEnemyAttack : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField]
    private Transform projectileSpawnPoint;

    private EnemyController enemy;
    private EnemyAnimation enemyAnimation;
    private Transform currentTarget;
    private float cooldownTimer;
    private bool hasFiredProjectile;

    public bool IsAttacking { get; private set; }
    public bool CanAttack => cooldownTimer <= 0f && !IsAttacking;
    public float AttackRange => enemy.Definition.RangedAttackRange;

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        enemyAnimation = GetComponent<EnemyAnimation>();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
    }

    public bool TryAttack(Transform target)
    {
        if (!CanAttack || target == null)
            return false;

        currentTarget = target;
        IsAttacking = true;
        hasFiredProjectile = false;

        // Start cooldown when the attack is committed.
        cooldownTimer = enemy.Definition.AttackCooldown;

        FaceTarget(currentTarget);
        enemyAnimation.PlayAttack();

        return true;
    }

    // Called by the AttackImpact animation event.
    public void AnimationAttackImpact()
    {
        if (!IsAttacking || hasFiredProjectile)
            return;

        // Set this first to prevent duplicate shots from repeated events.
        hasFiredProjectile = true;

        if (currentTarget == null)
            return;

        FireProjectile(currentTarget);
    }

    // Called by the AttackFinished animation event.
    public void AnimationAttackFinished()
    {
        if (!IsAttacking)
            return;

        IsAttacking = false;
        currentTarget = null;
    }

    public void CancelAttack()
    {
        IsAttacking = false;
        hasFiredProjectile = false;
        currentTarget = null;
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
            return;

        GameObject projectileObject = Instantiate(
            enemy.Definition.ProjectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.LookRotation(direction.normalized)
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

    private void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}
