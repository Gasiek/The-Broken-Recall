using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JumpSlamAction", menuName = "RPG/Actions/Jump Slam")]
public class JumpSlamAction : ActionDefinition
{
    [Header("Combat")]
    [SerializeField]
    private int damage = 25;

    [SerializeField]
    private float slamRadius = 3.5f;

    [SerializeField]
    private float knockbackDistance = 3.5f;

    [SerializeField]
    private LayerMask targetLayer;

    [Header("Jump Physics")]
    [SerializeField]
    private float jumpHeight = 2.5f;

    [SerializeField]
    private float forwardDistance = 2f;

    [Header("Audio & Effects")]
    [SerializeField]
    private AudioClip jumpSound;

    [SerializeField]
    private AudioClip slamSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float soundVolume = 0.8f;

    [SerializeField]
    private ParticleSystem slamVfxPrefab;

    public override IEnumerator Execute(ActionContext context)
    {
        if (context == null || context.Player == null)
        {
            yield break;
        }

        PlayerMovement movement = context.Movement;
        CharacterController cc =
            movement != null
                ? movement.CharacterController
                : context.Player.GetComponent<CharacterController>();

        // 1. Visual animation for sword
        context.Visuals.JumpSlam(Duration);

        // Optional jump sound
        if (jumpSound != null)
        {
            AudioManager.Instance.PlaySFX(jumpSound, soundVolume);
        }

        // 2. Lock normal player movement and gravity
        if (movement != null)
        {
            movement.SetMovementOverridden(true);
        }

        Vector3 forwardDir = context.Player.forward;
        float elapsed = 0f;
        float apexRatio = 0.45f;
        float landRatio = 0.72f;
        float totalJumpTime = Duration * landRatio;
        float apexTime = totalJumpTime * (apexRatio / landRatio);

        float prevY = 0f;
        float prevForward = 0f;
        bool hasImpacted = false;

        try
        {
            while (elapsed < totalJumpTime)
            {
                yield return null;
                elapsed += Time.deltaTime;
                float currentT = Mathf.Min(elapsed, totalJumpTime);

                // Vertical trajectory calculation
                float currentY;
                if (currentT <= apexTime)
                {
                    // Ease-out quad to apex
                    float p = currentT / apexTime;
                    currentY = jumpHeight * (1f - (1f - p) * (1f - p));
                }
                else
                {
                    // Ease-in quad from apex to ground
                    float p = (currentT - apexTime) / (totalJumpTime - apexTime);
                    currentY = jumpHeight * (1f - p * p);
                }

                // Forward leap progress
                float currentForward = forwardDistance * (currentT / totalJumpTime);

                float deltaY = currentY - prevY;
                float deltaForward = currentForward - prevForward;

                prevY = currentY;
                prevForward = currentForward;

                Vector3 frameDisplacement = forwardDir * deltaForward + Vector3.up * deltaY;
                if (cc != null && cc.enabled)
                {
                    cc.Move(frameDisplacement);
                }
                else
                {
                    context.Player.position += frameDisplacement;
                }

                // If falling and touched ground early, trigger impact immediately
                if (currentT > apexTime && cc != null && cc.isGrounded)
                {
                    break;
                }
            }

            // 3. Landing impact & AoE damage
            hasImpacted = true;
            OnSlamImpact(context);

            // 4. Recovery phase before restoring movement
            float remainingTime = Mathf.Max(0f, Duration - elapsed);
            if (remainingTime > 0f)
            {
                yield return new WaitForSeconds(remainingTime);
            }
        }
        finally
        {
            if (!hasImpacted)
            {
                OnSlamImpact(context);
            }

            if (movement != null)
            {
                movement.SetMovementOverridden(false);
            }
        }
    }

    private void OnSlamImpact(ActionContext context)
    {
        // Sword impact point slightly in front of the player
        Vector3 impactPosition = context.Player.position + context.Player.forward * 0.7f;

        // Play Slam SFX
        if (slamSound != null)
        {
            AudioManager.Instance.PlaySFX(slamSound, soundVolume);
        }

        // Spawn slam VFX if available
        if (slamVfxPrefab != null)
        {
            ParticleSystem vfx = Instantiate(slamVfxPrefab, impactPosition, Quaternion.identity);
            float vfxLifetime = vfx.main.duration + vfx.main.startLifetime.constantMax;
            Destroy(vfx.gameObject, vfxLifetime);
        }

        // Deal damage and knockback to all targets in radius (deduplicated per entity)
        Collider[] hits = Physics.OverlapSphere(impactPosition, slamRadius, targetLayer);
        HashSet<IDamageable> damagedEntities = new HashSet<IDamageable>();
        HashSet<IPushable> pushedEntities = new HashSet<IPushable>();

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null && damagedEntities.Add(damageable))
            {
                damageable.TakeDamage(damage);
            }

            IPushable pushable = hit.GetComponentInParent<IPushable>();
            if (pushable != null && pushedEntities.Add(pushable))
            {
                Vector3 pushDir = hit.transform.position - impactPosition;
                pushDir.y = 0f;
                if (pushDir.sqrMagnitude < 0.001f)
                {
                    pushDir = context.Player.forward;
                }
                else
                {
                    pushDir.Normalize();
                }

                pushable.Push(pushDir, knockbackDistance);
            }
        }
    }
}
