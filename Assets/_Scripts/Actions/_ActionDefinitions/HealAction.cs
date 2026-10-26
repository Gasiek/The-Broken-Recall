using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "HealAction", menuName = "RPG/Actions/Heal")]
public class HealAction : ActionDefinition
{
    [Header("Healing")]
    [SerializeField, Min(1)]
    private int healAmount = 5;

    [SerializeField, Min(0.01f)]
    private float healInterval = 1f;

    [SerializeField, Min(0.01f)]
    private float healDuration = 5f;

    [Header("Visual Effects")]
    [SerializeField]
    private ParticleSystem healVfxPrefab;

    [SerializeField]
    private Vector3 vfxOffset = Vector3.zero;

    public override void OnStarted(ActionContext context)
    {
        if (context?.Movement == null)
            return;

        // Prevent walking throughout sit-down, healing, and stand-up.
        context.Movement.SetMovementOverridden(true);
    }

    public override void OnHealSittingStarted(ActionContext context)
    {
        if (context == null || context.Health == null || context.RoutineRunner == null)
            return;

        // Prevent accidentally starting this routine twice.
        if (context.ActionRoutine != null)
            return;

        context.ActionRoutine = context.RoutineRunner.Run(HealOverTime(context));
    }

    private IEnumerator HealOverTime(ActionContext context)
    {
        float elapsed = 0f;

        // Heal immediately once the character is seated.
        ApplyHealing(context);

        while (elapsed < healDuration)
        {
            float waitTime = Mathf.Min(healInterval, healDuration - elapsed);
            yield return new WaitForSeconds(waitTime);

            elapsed += waitTime;

            // Don't heal at the exact end of the duration.
            if (elapsed < healDuration)
                ApplyHealing(context);
        }

        // The healing timer has ended; now stand up.
        context.ActionRoutine = null;

        PlayerAnimation playerAnimation =
            context.Player != null ? context.Player.GetComponent<PlayerAnimation>() : null;

        if (playerAnimation != null)
            playerAnimation.BeginHealStandUp();
        else
            context.ActionController?.CompleteAction(context);
    }

    private void ApplyHealing(ActionContext context)
    {
        if (context?.Health == null)
            return;

        context.Health.Heal(healAmount);

        if (healVfxPrefab == null || context.Player == null)
            return;

        Transform playerTransform = context.Player.transform;
        Vector3 spawnPosition =
            playerTransform.position + playerTransform.TransformVector(vfxOffset);

        VfxSpawner.SpawnOneShot(
            healVfxPrefab,
            spawnPosition,
            playerTransform.rotation,
            playerTransform
        );
    }

    public override void OnCancelled(ActionContext context)
    {
        StopHealingRoutine(context);
        ReleaseMovementLock(context);
    }

    public override void OnAnimationFinished(ActionContext context)
    {
        // Called after the stand-up animation finishes.
        StopHealingRoutine(context);
        ReleaseMovementLock(context);
    }

    private void StopHealingRoutine(ActionContext context)
    {
        if (context == null)
            return;

        if (context.ActionRoutine != null)
        {
            context.RoutineRunner?.Stop(context.ActionRoutine);
            context.ActionRoutine = null;
        }
    }

    private void ReleaseMovementLock(ActionContext context)
    {
        context?.Movement?.SetMovementOverridden(false);
    }

    public override string GetDescription()
    {
        return $"Restores {healAmount} health every {healInterval:0.##} seconds for {healDuration:0.##} seconds.";
    }
}
