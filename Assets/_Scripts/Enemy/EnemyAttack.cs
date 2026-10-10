using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyAttack : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    protected EnemyController enemy;
    private EnemyAttackVisuals visuals;

    protected float cooldownTimer;

    public float AttackRange => enemy.Definition.AttackRange;

    public bool CanAttack => cooldownTimer <= 0f && !IsAttacking;

    public bool IsAttacking { get; protected set; }

    protected virtual void Awake()
    {
        enemy = GetComponent<EnemyController>();
        visuals = GetComponent<EnemyAttackVisuals>();
    }

    protected virtual void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public virtual bool TryAttack(Transform target)
    {
        if (!CanAttack || target == null)
        {
            return false;
        }

        StartCoroutine(AttackRoutine(target));

        return true;
    }

    protected virtual IEnumerator AttackRoutine(Transform target)
    {
        IsAttacking = true;

        // Start the cooldown immediately when the attack is committed.
        cooldownTimer = enemy.Definition.AttackCooldown;

        // Lock the attack direction at the moment the attack starts.
        FaceTarget(target);

        // Wait for the attack wind-up.
        yield return new WaitForSeconds(enemy.Definition.AttackDelay);

        // Play the attack animation/effect.
        if (visuals != null)
        {
            visuals.BasicAttack();
        }

        // Check what is actually in front of the enemy NOW.
        DealDamage();

        IsAttacking = false;
    }

    protected void FaceTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction);
    }

    protected void DealDamage()
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

    protected virtual void OnDrawGizmosSelected()
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
