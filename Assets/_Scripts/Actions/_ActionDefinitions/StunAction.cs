using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StunAction", menuName = "RPG/Actions/Stun")]
public class StunAction : ActionDefinition
{
    [Header("Hitbox")]
    [SerializeField]
    private float hitboxOffset = 0.8f;

    [SerializeField]
    private float hitboxRadius = 1.1f;

    [SerializeField]
    private LayerMask targetLayer;

    [Header("Stun Effect")]
    [SerializeField]
    private StatusEffectDefinition stunStatusEffect;

    [Header("Audio")]
    [Tooltip("Optional sound override. If empty, the status effect's apply sound is played.")]
    [SerializeField]
    private AudioClip stunSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float soundVolume = 0.9f;

    public float StunDuration => stunStatusEffect != null ? stunStatusEffect.Duration : 0f;

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context?.Player == null)
            return;

        PerformStunStrike(context);
    }

    private void PerformStunStrike(ActionContext context)
    {
        Collider[] hits = ActionHitDetector.DetectForward(
            context.Player,
            hitboxOffset,
            hitboxRadius,
            targetLayer,
            QueryTriggerInteraction.Ignore
        );

        HashSet<StatusEffectManager> stunnedTargets = new HashSet<StatusEffectManager>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            StatusEffectManager statusManager = hit.GetComponent<StatusEffectManager>();

            if (statusManager == null || !stunnedTargets.Add(statusManager))
                continue;

            ApplyStun(hit.gameObject, statusManager);
        }
    }

    private void ApplyStun(GameObject targetObj, StatusEffectManager statusManager)
    {
        if (targetObj == null || statusManager == null)
            return;

        if (stunStatusEffect != null)
        {
            statusManager.ApplyStatus(stunStatusEffect);
        }
        else
        {
            Debug.LogWarning("StunAction has no stun status effect assigned.", this);

            return;
        }

        if (stunSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(
                stunSound,
                targetObj.transform.position,
                soundVolume
            );
        }
    }
}
