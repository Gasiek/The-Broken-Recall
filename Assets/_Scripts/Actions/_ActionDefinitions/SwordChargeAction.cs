using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SwordChargeAction", menuName = "RPG/Actions/Attacks/Sword Charge")]
public class SwordChargeAction : AttackActionDefinition
{
    [Header("Charge Motion & Timing")]
    [SerializeField]
    private float chargeMoveDuration = 1f;

    [SerializeField]
    private float chargeDistance = 4f;

    [Header("Audio & Effects")]
    [SerializeField]
    private AudioClip dashSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float soundVolume = 0.9f;

    [SerializeField]
    private ParticleSystem dashTrailVfxPrefab;

    public new int Damage => base.Damage;
    public float ChargeDistance => chargeDistance;
    public float ChargeMoveDuration => chargeMoveDuration;

    // Charge completion follows its existing animation/action lifecycle.
    public override void OnStarted(ActionContext context) { }

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context?.Player == null || context.Movement == null)
            return;

        if (context.Health == null || context.Health.IsDead)
            return;

        Vector3 direction = Vector3.ProjectOnPlane(context.Player.transform.forward, Vector3.up);

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        if (!context.Movement.BeginChargeMovement(direction, chargeDistance, chargeMoveDuration))
        {
            context.Movement.CancelChargeMovement();
            return;
        }

        PlayChargeEffects(context, direction);

        ActionRoutineRunner runner = context.Player.GetComponent<ActionRoutineRunner>();

        if (runner != null)
            runner.Run(ChargeRoutine(context));
        else
            context.Movement.CancelChargeMovement();
    }

    private IEnumerator ChargeRoutine(ActionContext context)
    {
        PlayerMovement movement = context.Movement;

        // Shared across all frames of this charge.
        HashSet<GameObject> alreadyHit = new HashSet<GameObject>();

        while (movement != null && movement.IsChargeMoving)
        {
            if (context.Health == null || context.Health.IsDead)
                break;

            if (context.Player == null)
                yield break;

            Collider[] hits = ActionHitDetector.DetectForward(
                context.Player,
                HitboxForwardOffset,
                HitboxRadius,
                TargetLayer,
                QueryTriggerInteraction.Ignore
            );

            ProcessHits(context, hits, alreadyHit);

            yield return null;
        }

        if (movement != null)
            movement.CancelChargeMovement();
    }

    private void PlayChargeEffects(ActionContext context, Vector3 direction)
    {
        if (dashSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(
                dashSound,
                context.Player.transform.position,
                soundVolume
            );
        }

        if (dashTrailVfxPrefab != null)
        {
            VfxSpawner.SpawnOneShot(
                dashTrailVfxPrefab,
                context.Player.transform.position + Vector3.up * 0.5f,
                Quaternion.LookRotation(direction)
            );
        }
    }

    public override void OnAnimationFinished(ActionContext context)
    {
        CleanupCharge(context);
    }

    public override void OnCancelled(ActionContext context)
    {
        if (context?.Player != null)
        {
            ActionRoutineRunner runner = context.Player.GetComponent<ActionRoutineRunner>();

            if (runner != null)
                runner.StopAllActionRoutines();
        }

        CleanupCharge(context);
    }

    private void CleanupCharge(ActionContext context)
    {
        if (context?.Movement != null)
            context.Movement.CancelChargeMovement();
    }

    public override string GetDescription()
    {
        return $"Charges forward {chargeDistance:0.#}m, dealing "
            + $"{Damage} damage and applying configured effects to enemies hit.";
    }
}
