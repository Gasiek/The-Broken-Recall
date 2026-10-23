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
    private float chargeMoveDuration = 1.0f;

    [SerializeField]
    private float chargeDistance = 4.0f;

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

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context?.Player == null || context.Movement == null)
            return;

        if (context.Health == null || context.Health.IsDead)
            return;

        Vector3 direction = context.Player.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        if (!context.Movement.BeginChargeMovement(direction, chargeDistance, chargeMoveDuration))
        {
            context.Movement.CancelChargeMovement();
            return;
        }

        PlayChargeEffects(context, direction);

        GetRoutineRunner(context).Run(ChargeRoutine(context, direction));
    }

    private IEnumerator ChargeRoutine(ActionContext context, Vector3 direction)
    {
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        PlayerMovement movement = context.Movement;

        while (movement != null && movement.IsChargeMoving)
        {
            if (context.Health == null || context.Health.IsDead)
                break;

            PerformChargeDamage(context, direction, damagedTargets);

            yield return null;
        }

        if (movement != null)
            movement.CancelChargeMovement();
    }

    public override void OnAnimationFinished(ActionContext context)
    {
        CleanupCharge(context);
    }

    public override void OnCancelled(ActionContext context)
    {
        if (context?.Player != null)
            GetRoutineRunner(context).StopAllActionRoutines();

        CleanupCharge(context);
    }

    private void CleanupCharge(ActionContext context)
    {
        if (context?.Movement != null)
            context.Movement.CancelChargeMovement();
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

    private ActionRoutineRunner GetRoutineRunner(ActionContext context)
    {
        return context.Player.GetComponent<ActionRoutineRunner>();
    }
}
