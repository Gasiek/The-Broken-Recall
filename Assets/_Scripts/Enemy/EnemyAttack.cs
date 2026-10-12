using UnityEngine;

[RequireComponent(typeof(EnemyAnimation))]
[RequireComponent(typeof(EnemyController))]
public class EnemyAttack : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    private EnemyController enemy;
    private EnemyAnimation enemyAnimation;

    private float cooldownTimer;
    private bool hasAppliedDamage;

    public float AttackRange => enemy.Definition.AttackRange;

    public bool CanAttack => cooldownTimer <= 0f && !IsAttacking;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        enemyAnimation = GetComponent<EnemyAnimation>();
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
            return false;

        IsAttacking = true;
        hasAppliedDamage = false;

        cooldownTimer = enemy.Definition.AttackCooldown;

        FaceTarget(target);

        enemyAnimation.PlayAttack();

        return true;
    }

    // Called by the AttackImpact Animation Event.
    public void AnimationAttackImpact()
    {
        if (!IsAttacking || hasAppliedDamage)
            return;

        hasAppliedDamage = true;
        DealDamage();
    }

    // Called by the AttackFinished Animation Event.
    public void AnimationAttackFinished()
    {
        IsAttacking = false;
    }

    public void CancelAttack()
    {
        IsAttacking = false;
        hasAppliedDamage = false;
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void DealDamage()
    {
        Vector3 origin = transform.position + 0.5f * transform.up;
        Vector3 direction = transform.forward;

        if (direction.sqrMagnitude <= 0.001f)
            return;

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
            return;

        damageable.TakeDamage(enemy.Definition.AttackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (enemy == null || enemy.Definition == null)
            return;

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
