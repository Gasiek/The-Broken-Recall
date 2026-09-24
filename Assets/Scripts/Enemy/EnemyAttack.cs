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

        cooldownTimer = enemy.Definition.AttackCooldown;

        FaceTarget(target);

        visuals.BasicAttack();

        yield return new WaitForSeconds(enemy.Definition.AttackDuration * 0.5f);

        DealDamage(target);

        yield return new WaitForSeconds(enemy.Definition.AttackDuration * 0.5f);

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

        if (
            !Physics.SphereCast(
                origin,
                enemy.Definition.AttackRadius,
                direction,
                out RaycastHit hit,
                enemy.Definition.AttackRange,
                targetLayer
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
}
