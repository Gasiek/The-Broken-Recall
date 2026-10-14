using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SwordChargeAction", menuName = "RPG/Actions/Sword Charge")]
public class SwordChargeAction : ActionDefinition
{
    [Header("Charge Combat")]
    [SerializeField]
    private int damage = 25;

    [Tooltip("Radius around the front of the player used to detect targets.")]
    [SerializeField]
    private float hitRadius = 1.1f;

    [SerializeField]
    private LayerMask targetLayer;

    [Header("Charge Motion & Timing")]
    [SerializeField]
    private float prepDuration = 0.3f;

    [SerializeField]
    private float holdDuration = 0.25f;

    [SerializeField]
    private float chargeMoveDuration = 1.0f;

    [SerializeField]
    private float chargeDistance = 4.0f;

    [SerializeField]
    private float recoveryDuration = 0.2f;

    [Header("Audio & Effects")]
    [SerializeField]
    private AudioClip dashSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float soundVolume = 0.9f;

    [SerializeField]
    private ParticleSystem dashTrailVfxPrefab;

    public int Damage => damage;
    public float ChargeDistance => chargeDistance;
    public float ChargeMoveDuration => chargeMoveDuration;

    public override IEnumerator Execute(ActionContext context)
    {
        PlayerMovement movement = context.Movement;

        context.Visuals.SwordCharge(
            prepDuration,
            holdDuration,
            chargeMoveDuration,
            recoveryDuration
        );

        float prepTime = Mathf.Max(0f, prepDuration) + Mathf.Max(0f, holdDuration);

        if (prepTime > 0f)
            yield return new WaitForSeconds(prepTime);

        if (context.Health == null || context.Health.IsDead)
        {
            movement.CancelChargeMovement();
            yield break;
        }

        Vector3 direction = context.Player.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            movement.CancelChargeMovement();
            yield break;
        }

        direction.Normalize();

        if (!movement.BeginChargeMovement(direction, chargeDistance, chargeMoveDuration))
        {
            movement.CancelChargeMovement();
            yield break;
        }

        PlayChargeEffects(context, direction);

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        while (movement.IsChargeMoving)
        {
            if (context.Health == null || context.Health.IsDead)
                break;

            PerformChargeDamage(context, direction, damagedTargets);

            yield return null;
        }

        // Release charge movement before recovery.
        movement.CancelChargeMovement();

        if (context.Health != null && !context.Health.IsDead)
            PerformChargeDamage(context, direction, damagedTargets);

        if (recoveryDuration > 0f)
            yield return new WaitForSeconds(recoveryDuration);
    }

    private void PlayChargeEffects(ActionContext context, Vector3 direction)
    {
        if (dashSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(dashSound, context.Player.position, soundVolume);
        }

        if (dashTrailVfxPrefab != null)
        {
            VfxSpawner.SpawnOneShot(
                dashTrailVfxPrefab,
                context.Player.position + Vector3.up * 0.5f,
                Quaternion.LookRotation(direction)
            );
        }
    }

    private void PerformChargeDamage(
        ActionContext context,
        Vector3 chargeDirection,
        HashSet<IDamageable> damagedTargets
    )
    {
        Vector3 checkOrigin = context.Player.position + chargeDirection * 0.8f + Vector3.up * 0.8f;

        Collider[] hits = Physics.OverlapSphere(
            checkOrigin,
            hitRadius,
            targetLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hitCol in hits)
        {
            if (hitCol == null)
                continue;

            IDamageable damageable = hitCol.GetComponent<IDamageable>();

            if (damageable == null || !damagedTargets.Add(damageable))
                continue;

            Vector3 hitPoint = hitCol.ClosestPoint(checkOrigin);

            CombatResolver.ResolveAttackHit(
                context.Player.gameObject,
                context.WeaponBuffs,
                damageable,
                damage,
                hitPoint
            );
        }
    }
}
