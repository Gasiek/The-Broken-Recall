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

    public override void OnStarted(ActionContext context)
    {
        if (context?.Player == null || context.Movement == null)
            return;

        context.Movement.SetMovementOverridden(true);

        if (jumpSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(jumpSound, context.Player.position, soundVolume);
        }

        GetRoutineRunner(context).Run(JumpRoutine(context));
    }

    private IEnumerator JumpRoutine(ActionContext context)
    {
        if (context?.Player == null || context.Movement == null)
            yield break;

        PlayerMovement movement = context.Movement;
        Transform playerTransform = context.Player;

        Vector3 forwardDir = playerTransform.forward;
        forwardDir.y = 0f;

        if (forwardDir.sqrMagnitude < 0.001f)
            yield break;

        forwardDir.Normalize();

        float apexRatio = 0.45f;
        float landRatio = 0.72f;
        float totalJumpTime = Duration * landRatio;
        float apexTime = totalJumpTime * (apexRatio / landRatio);

        float elapsed = 0f;
        float prevY = 0f;
        float prevForward = 0f;

        while (elapsed < totalJumpTime)
        {
            yield return null;

            if (context.Player == null || context.Movement == null)
                yield break;

            elapsed += Time.deltaTime;
            float currentT = Mathf.Min(elapsed, totalJumpTime);

            float currentY;

            if (currentT <= apexTime)
            {
                float p = currentT / apexTime;
                currentY = jumpHeight * (1f - (1f - p) * (1f - p));
            }
            else
            {
                float p = (currentT - apexTime) / (totalJumpTime - apexTime);

                currentY = jumpHeight * (1f - p * p);
            }

            float currentForward = forwardDistance * (currentT / totalJumpTime);

            Vector3 displacement =
                forwardDir * (currentForward - prevForward) + Vector3.up * (currentY - prevY);

            prevY = currentY;
            prevForward = currentForward;

            CollisionFlags flags = movement.MoveActionDisplacement(displacement);

            // Stop the scripted leap if a ceiling or wall blocks it.
            if ((flags & CollisionFlags.Above) != 0 || (flags & CollisionFlags.Sides) != 0)
            {
                yield break;
            }

            if (currentT > apexTime && movement.IsGrounded)
                yield break;
        }
    }

    private void OnSlamImpact(ActionContext context)
    {
        Vector3 impactPosition = context.Player.position + context.Player.forward * 0.7f;

        if (slamSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(slamSound, impactPosition, soundVolume);
        }

        VfxSpawner.SpawnOneShot(slamVfxPrefab, impactPosition, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(impactPosition, slamRadius, targetLayer);

        HashSet<IDamageable> damagedEntities = new HashSet<IDamageable>();

        HashSet<IPushable> pushedEntities = new HashSet<IPushable>();

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable != null && damagedEntities.Add(damageable))
            {
                CombatResolver.ResolveAttackHit(
                    context.Player.gameObject,
                    context.WeaponBuffs,
                    damageable,
                    damage,
                    impactPosition
                );
            }

            IPushable pushable = hit.GetComponent<IPushable>();

            if (pushable != null && pushedEntities.Add(pushable))
            {
                Vector3 pushDir = hit.transform.position - impactPosition;

                pushDir.y = 0f;

                if (pushDir.sqrMagnitude < 0.001f)
                    pushDir = context.Player.forward;
                else
                    pushDir.Normalize();

                pushable.Push(pushDir, knockbackDistance);
            }
        }
    }

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context == null || context.Player == null)
            return;

        OnSlamImpact(context);
    }

    public override void OnAnimationFinished(ActionContext context)
    {
        Cleanup(context);
    }

    public override void OnCancelled(ActionContext context)
    {
        if (context?.Player != null)
            GetRoutineRunner(context).StopAllActionRoutines();

        Cleanup(context);
    }

    private void Cleanup(ActionContext context)
    {
        if (context?.Movement != null)
            context.Movement.SetMovementOverridden(false);
    }

    public override string GetDescription() =>
        $"Leaps into the air and slams down, dealing {damage} damage within a {slamRadius:0.#}m radius and knocking enemies back {knockbackDistance:0.#}m.";

    private ActionRoutineRunner GetRoutineRunner(ActionContext context)
    {
        return context.Player.GetComponent<ActionRoutineRunner>();
    }
}
