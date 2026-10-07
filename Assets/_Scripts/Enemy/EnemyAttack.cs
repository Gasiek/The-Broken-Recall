using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyAttackVisuals))]
[RequireComponent(typeof(EnemyController))]
public class EnemyAttack : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    private EnemyController enemy;
    private EnemyAttackVisuals visuals;

    private float cooldownTimer;

    public float AttackRange => enemy.Definition.AttackRange;

    public bool CanAttack => cooldownTimer <= 0f && !IsAttacking;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        visuals = GetComponent<EnemyAttackVisuals>();
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

        // Start the cooldown immediately when the attack is committed.
        cooldownTimer = enemy.Definition.AttackCooldown;

        // Lock the attack direction at the moment the attack starts.
        FaceTarget(target);

        // Wait for the attack wind-up.
        yield return new WaitForSeconds(enemy.Definition.AttackDelay);

        // Play the attack animation/effect. TODO: when proper animations are implemented, we need to change the timing of when the damage id dealt
        visuals.BasicAttack();

        // Check what is actually in front of the enemy NOW.
        // We do not use the target Transform here.
        DealDamage();

        IsAttacking = false;
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void DealDamage()
    {
        Vector3 origin = transform.position + 0.5f * transform.up;
        Vector3 direction = transform.forward;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        if (
            !Physics.SphereCast(
                origin,
                enemy.Definition.AttackRadius,
                direction,
                out RaycastHit hit,
                enemy.Definition.AttackRange,
                targetLayer,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            return;
        }

        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(enemy.Definition.AttackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (enemy == null || enemy.Definition == null)
        {
            return;
        }

        float radius = enemy.Definition.AttackRadius;
        float distance = enemy.Definition.AttackRange;

        Vector3 start = transform.position + 0.5f * transform.up;
        Vector3 end = start + transform.forward * distance;

        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);

        Gizmos.DrawLine(start + transform.right * radius, end + transform.right * radius);

        Gizmos.DrawLine(start - transform.right * radius, end - transform.right * radius);

        Gizmos.DrawLine(start + transform.up * radius, end + transform.up * radius);

        Gizmos.DrawLine(start - transform.up * radius, end - transform.up * radius);
    }
}
