using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossAttackVisuals))]
[RequireComponent(typeof(EnemyController))]
public class BossAttack : EnemyAttack
{
    private BossAttackVisuals bossVisuals;

    protected override void Awake()
    {
        base.Awake();
        bossVisuals = GetComponent<BossAttackVisuals>();
    }

    protected override IEnumerator AttackRoutine(Transform target)
    {
        IsAttacking = true;

        // Start cooldown immediately
        cooldownTimer = enemy.Definition.AttackCooldown;

        FaceTarget(target);

        // Wind-up: raise weapon and prepare body for strike
        if (bossVisuals != null)
        {
            bossVisuals.StartWindUp(enemy.Definition.AttackDelay);
        }

        // Wait for wind-up duration
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

        // Apply damage at the exact impact moment
        DealDamage();

        // Recovery: wait for weapon/body to return to idle pose before allowing movement
        if (bossVisuals != null && bossVisuals.RecoveryDuration > 0f)
        {
            yield return new WaitForSeconds(bossVisuals.RecoveryDuration);
        }

        IsAttacking = false;
    }
}
