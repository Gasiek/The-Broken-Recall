using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossAttackVisuals))]
[RequireComponent(typeof(EnemyController))]
public class BossAttack : EnemyAttack
{
    [Header("Spin Attack AI Settings")]
    [Tooltip("Probability (0 to 1) that the boss will perform the 360-degree spin attack instead of the normal ground slam.")]
    [SerializeField, Range(0f, 1f)]
    private float spinAttackChance = 0.4f;

    [Tooltip("Radius around the boss for the 360-degree spin attack damage (in meters).")]
    [SerializeField]
    private float spinAttackRadius = 2.5f;

    [Header("Smooth Turning")]
    [Tooltip("Turning speed in degrees per second when rotating towards the player before attacking. Lower value = slower, heavier rotation (e.g. 180-240).")]
    [SerializeField]
    private float smoothTurnSpeed = 50f;

    private BossAttackVisuals bossVisuals;

    protected override void Awake()
    {
        base.Awake();
        bossVisuals = GetComponent<BossAttackVisuals>();
    }

    protected override IEnumerator AttackRoutine(Transform target)
    {
        if (target == null)
        {
            IsAttacking = false;
            yield break;
        }

        IsAttacking = true;
        cooldownTimer = enemy.Definition.AttackCooldown;

        // 40% chance to perform 360-degree spin attack (independent of player position)
        bool shouldSpin = Random.value <= spinAttackChance;

        if (shouldSpin)
        {
            yield return StartCoroutine(SpinAttackRoutine());
        }
        else
        {
            yield return StartCoroutine(GroundSlamRoutine(target));
        }

        IsAttacking = false;
    }

    private IEnumerator SpinAttackRoutine()
    {
        // Boss attacks 360 degrees all around him without snapping rotation.
        // He crouches with weapon drawn, spins 360 degrees, and recovers.
        bool isFinished = false;

        if (bossVisuals != null)
        {
            bossVisuals.PlaySpinAttack(
                onSpinStrike: DealSpinDamage,
                onComplete: () => isFinished = true
            );

            yield return new WaitUntil(() => isFinished);
        }
        else
        {
            DealSpinDamage();
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator GroundSlamRoutine(Transform target)
    {
        // Smoothly rotate to face the target before executing front slam (no instant snapping!)
        yield return StartCoroutine(SmoothRotateTowardsTarget(target));

        // Wind-up: raise weapon and prepare body for strike
        if (bossVisuals != null)
        {
            bossVisuals.StartWindUp(enemy.Definition.AttackDelay);
        }

        yield return new WaitForSeconds(enemy.Definition.AttackDelay);

        // Execute slam swing
        if (bossVisuals != null)
        {
            bossVisuals.PlaySlamAttack();
        }

        // Wait for downward swing to reach impact point
        if (bossVisuals != null && bossVisuals.SlamDuration > 0f)
        {
            yield return new WaitForSeconds(bossVisuals.SlamDuration);
        }

        // Apply damage in front
        DealDamage();

        // Recovery: wait for weapon/body to return to idle pose before allowing movement
        if (bossVisuals != null && bossVisuals.RecoveryDuration > 0f)
        {
            yield return new WaitForSeconds(bossVisuals.RecoveryDuration);
        }
    }

    private IEnumerator SmoothRotateTowardsTarget(Transform target, float maxDuration = 1.5f)
    {
        if (target == null) yield break;

        float turnSpeed = smoothTurnSpeed;

        float elapsed = 0f;

        while (elapsed < maxDuration)
        {
            if (target == null) yield break;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f) yield break;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float angleRemaining = Quaternion.Angle(transform.rotation, targetRotation);

            if (angleRemaining <= 3f)
            {
                transform.rotation = targetRotation;
                yield break;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void DealSpinDamage()
    {
        Vector3 origin = transform.position + Vector3.up * 1f;
        float radius = spinAttackRadius > 0f ? spinAttackRadius : enemy.Definition.AttackRange;

        Collider[] hits = Physics.OverlapSphere(origin, radius, targetLayer, QueryTriggerInteraction.Ignore);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider col in hits)
        {
            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null && damagedTargets.Add(damageable))
            {
                damageable.TakeDamage(enemy.Definition.AttackDamage);
            }
        }
    }

    protected override void FaceTarget(Transform target)
    {
        // Override to prevent instant snap rotation from base class.
        // Smooth rotation is handled explicitly via SmoothRotateTowardsTarget or EnemyStateMachine.
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, spinAttackRadius);
    }
}
