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

    [Tooltip("Duration of the rapid forward charge movement in seconds (as requested: 1 sec).")]
    [SerializeField]
    private float chargeMoveDuration = 1.0f;

    [Tooltip("Distance traveled during the charge (as requested: 4 units).")]
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
        if (context == null || context.Player == null)
        {
            yield break;
        }

        PlayerMovement movement = context.Movement;
        CharacterController cc = movement != null
            ? movement.CharacterController
            : context.Player.GetComponent<CharacterController>();

        // Lock regular movement controls during the charge action
        if (movement != null)
        {
            movement.SetMovementOverridden(true);
        }

        try
        {
            // 1. Preparation Phase:
            // "Player wyciaga miecz przed siebie. trzyma go tak"
            if (context.Visuals != null)
            {
                context.Visuals.SwordCharge(
                    prepDuration,
                    holdDuration,
                    chargeMoveDuration,
                    recoveryDuration
                );
            }

            float prepWaitTime = prepDuration + holdDuration;
            if (prepWaitTime > 0f)
            {
                yield return new WaitForSeconds(prepWaitTime);
            }

            // 2. Burst Charge Phase:
            // "i nagle szybko (w 1 sek) porusza sie do przodu na odleglosc 4 zadajac obrazenia wszystkiemu na swojej sciezce"
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

                // Move forward via CharacterController for obstacle / terrain collision
                Vector3 moveDelta = chargeDirection * speed * dt;
                // Add grounding gravity to prevent floating off steps/slopes
                moveDelta += Vector3.down * (9.81f * dt);

                if (cc != null && cc.enabled)
                {
                    cc.Move(moveDelta);
                }
                else
                {
                    context.Player.position += chargeDirection * speed * dt;
                }

                // Check for enemies along the path in front of the thrusting sword
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

                yield return null;
            }

            // 3. Recovery Phase:
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
}
