using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SwordChargeAction", menuName = "RPG/Actions/Sword Charge")]
public class SwordChargeAction : ActionDefinition
{
    [Header("Charge Combat")]
    [SerializeField]
    private int damage = 25;

    [Tooltip("Radius around the sword/front of the player to detect and hit enemies.")]
    [SerializeField]
    private float hitRadius = 1.1f;

    [SerializeField]
    private LayerMask targetLayer;

    [Header("Charge Motion & Timing")]
    [Tooltip("Duration to point the sword forward.")]
    [SerializeField]
    private float prepDuration = 0.3f;

    [Tooltip("Duration of holding the sword pointed forward before the sudden burst.")]
    [SerializeField]
    private float holdDuration = 0.25f;

    [Tooltip("Duration of the rapid forward charge movement in seconds.")]
    [SerializeField]
    private float chargeMoveDuration = 1.0f;

    [Tooltip("Distance traveled during the charge.")]
    [SerializeField]
    private float chargeDistance = 4.0f;

    [Tooltip("Recovery duration after completing the charge.")]
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
        CharacterController cc = movement != null
            ? movement.CharacterController
            : context.Player.GetComponent<CharacterController>();

        if (movement != null)
        {
            movement.SetMovementOverridden(true);
        }

        try
        {
            context.Visuals.SwordCharge(
                prepDuration,
                holdDuration,
                chargeMoveDuration,
                recoveryDuration
            );

            float prepWaitTime = prepDuration + holdDuration;
            if (prepWaitTime > 0f)
            {
                yield return new WaitForSeconds(prepWaitTime);
            }

            yield return PerformCharge(context, cc);

            if (recoveryDuration > 0f)
            {
                yield return new WaitForSeconds(recoveryDuration);
            }
        }
        finally
        {
            if (movement != null)
            {
                movement.SetMovementOverridden(false);
            }
        }
    }

    private IEnumerator PerformCharge(ActionContext context, CharacterController cc)
    {
        Vector3 chargeDirection = context.Player.forward;
        chargeDirection.y = 0f;
        if (chargeDirection.sqrMagnitude > 0.001f)
        {
            chargeDirection.Normalize();
        }

        if (dashSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(dashSound, context.Player.position, soundVolume);
        }

        if (dashTrailVfxPrefab != null)
        {
            VfxSpawner.SpawnOneShot(
                dashTrailVfxPrefab,
                context.Player.position + Vector3.up * 0.5f,
                Quaternion.LookRotation(chargeDirection)
            );
        }

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        float elapsed = 0f;
        float speed = chargeDistance / Mathf.Max(0.01f, chargeMoveDuration);

        while (elapsed < chargeMoveDuration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            Vector3 moveDelta = chargeDirection * speed * dt;
            moveDelta += Vector3.down * (9.81f * dt);

            cc.Move(moveDelta);
            PerformChargeDamage(context, chargeDirection, damagedTargets);

            yield return null;
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

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCol = hits[i];
            IDamageable damageable = hitCol.GetComponentInParent<IDamageable>();

            if (damageable != null && damagedTargets.Add(damageable))
            {
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
}
