using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JumpSlamAction", menuName = "RPG/Actions/Attacks/Jump Slam")]
public class JumpSlamAction : AttackActionDefinition
{
    [Header("Slam")]
    [SerializeField]
    private float slamImpactForwardOffset = 0.7f;

    [SerializeField]
    private float knockbackDistance = 3.5f;

    [Header("Jump Physics")]
    [SerializeField, Min(0.01f)]
    private float jumpDuration = 0.72f;

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

    public override void OnStarted(ActionContext context)
    {
        if (context?.Player == null || context.Movement == null)
            return;

        context.Movement.SetMovementOverridden(true);

        if (jumpSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(
                jumpSound,
                context.Player.transform.position,
                soundVolume
            );
        }

        ActionRoutineRunner runner = context.Player.GetComponent<ActionRoutineRunner>();

        if (runner != null)
            runner.Run(JumpRoutine(context));
        else
            Cleanup(context);
    }

    private IEnumerator JumpRoutine(ActionContext context)
    {
        if (context?.Player == null || context.Movement == null)
            yield break;

        PlayerMovement movement = context.Movement;
        Transform playerTransform = context.Player.transform;

        Vector3 forwardDir = Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up);

        if (forwardDir.sqrMagnitude < 0.001f)
        {
            Cleanup(context);
            yield break;
        }

        forwardDir.Normalize();

        float totalJumpTime = jumpDuration;
        float apexTime = totalJumpTime * 0.625f;

        if (totalJumpTime <= 0f)
        {
            Cleanup(context);
            yield break;
        }

        float elapsed = 0f;
        float previousY = 0f;
        float previousForward = 0f;

        while (elapsed < totalJumpTime)
        {
            yield return null;

            if (context.Player == null || context.Movement == null)
                yield break;

            elapsed += Time.deltaTime;
            float currentTime = Mathf.Min(elapsed, totalJumpTime);

            float currentY;

            if (currentTime <= apexTime)
            {
                float p = currentTime / apexTime;
                currentY = jumpHeight * (1f - (1f - p) * (1f - p));
            }
            else
            {
                float p = (currentTime - apexTime) / (totalJumpTime - apexTime);

                currentY = jumpHeight * (1f - p * p);
            }

            float currentForward = forwardDistance * (currentTime / totalJumpTime);

            Vector3 displacement =
                forwardDir * (currentForward - previousForward)
                + Vector3.up * (currentY - previousY);

            previousY = currentY;
            previousForward = currentForward;

            CollisionFlags flags = movement.MoveActionDisplacement(displacement);

            if ((flags & CollisionFlags.Above) != 0 || (flags & CollisionFlags.Sides) != 0)
            {
                yield break;
            }

            if (currentTime > apexTime && movement.IsGrounded)
                yield break;
        }
    }

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context?.Player == null)
            return;

        Vector3 forward = Vector3
            .ProjectOnPlane(context.Player.transform.forward, Vector3.up)
            .normalized;

        Vector3 impactPosition =
            context.Player.transform.position + forward * slamImpactForwardOffset;

        if (slamSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(slamSound, impactPosition, soundVolume);
        }

        if (slamVfxPrefab != null)
        {
            VfxSpawner.SpawnOneShot(slamVfxPrefab, impactPosition, Quaternion.identity);
        }

        // Shared damage and effect application.
        PerformAreaHit(context, impactPosition, HitboxRadius);

        // Slam-specific knockback.
        Collider[] hits = ActionHitDetector.DetectRadial(
            impactPosition,
            HitboxRadius,
            TargetLayer,
            QueryTriggerInteraction.Ignore
        );

        HashSet<IPushable> pushedTargets = new HashSet<IPushable>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            IPushable pushable = hit.GetComponent<IPushable>();

            if (pushable == null || !pushedTargets.Add(pushable))
                continue;

            Vector3 pushDirection = hit.transform.position - impactPosition;

            pushDirection.y = 0f;

            if (pushDirection.sqrMagnitude < 0.001f)
                pushDirection = forward;
            else
                pushDirection.Normalize();

            pushable.Push(pushDirection, knockbackDistance);
        }
    }

    public override void OnAnimationFinished(ActionContext context)
    {
        Cleanup(context);
    }

    public override void OnCancelled(ActionContext context)
    {
        if (context?.Player != null)
        {
            ActionRoutineRunner runner = context.Player.GetComponent<ActionRoutineRunner>();

            if (runner != null)
                runner.StopAllActionRoutines();
        }

        Cleanup(context);
    }

    private void Cleanup(ActionContext context)
    {
        if (context?.Movement != null)
            context.Movement.SetMovementOverridden(false);
    }

    public override string GetDescription()
    {
        string damageDescription = DealDamage
            ? $"dealing {Damage} damage"
            : "applying configured effects";

        return $"Leaps and slams down, {damageDescription} within a "
            + $"{HitboxRadius:0.#}m radius and knocking enemies back "
            + $"{knockbackDistance:0.#}m.";
    }
}
