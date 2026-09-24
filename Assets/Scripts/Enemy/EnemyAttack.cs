using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyAttackVisuals))]
public class EnemyAttack : MonoBehaviour
{
    [SerializeField]
    private int damage = 10;

    [SerializeField]
    private float attackRange = 1.5f;

    [Tooltip("The radius of the sphere cast for the attack.")]
    [SerializeField]
    private float attackRadius = 0.35f;

    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField]
    private float attackDuration = 0.4f;

    [SerializeField]
    private float attackCooldown = 1f;

    private EnemyAttackVisuals visuals;

    private float cooldownTimer;

    public float AttackRange => attackRange;

    public bool CanAttack =>
        cooldownTimer <= 0f && !IsAttacking;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
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
        cooldownTimer = attackCooldown;

        FaceTarget(target);

        visuals.BasicAttack();

        yield return new WaitForSeconds(
            attackDuration * 0.5f
        );

        DealDamage(target);

        yield return new WaitForSeconds(
            attackDuration * 0.5f
        );

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

        transform.rotation = Quaternion.LookRotation(
            direction
        );
    }

    private void DealDamage(Transform target)
    {
        if (target == null)
        {
            return;
        }

        Vector3 origin = transform.position;

        Vector3 direction = target.position - origin;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();

        if (!Physics.SphereCast(
                origin,
                attackRadius,
                direction,
                out RaycastHit hit,
                attackRange,
                targetLayer))
        {
            return;
        }

        IDamageable damageable =
            hit.collider.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(damage);
    }
}