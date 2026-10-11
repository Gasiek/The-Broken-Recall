using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "StunAction", menuName = "RPG/Actions/Stun")]
public class StunAction : ActionDefinition
{
    [Header("Attack Range & Target")]
    [SerializeField]
    private float attackRange = 1.6f;

    [Tooltip("Radius of the sphere cast for detecting the hit enemy in front.")]
    [SerializeField]
    private float attackRadius = 0.5f;

    [SerializeField]
    private LayerMask targetLayer;

    [Header("Stun Effect")]
    [Tooltip("Status effect definition for stun. If empty, a runtime stun effect with stunDuration is created.")]
    [SerializeField]
    private StatusEffectDefinition stunStatusEffect;

    [Tooltip("Fallback duration in seconds that the enemy is stunned (cannot move or attack).")]
    [SerializeField]
    private float stunDuration = 2.0f;

    [Header("Audio")]
    [Tooltip("Sound played on the enemy when they become stunned.")]
    [SerializeField]
    private AudioClip stunSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float soundVolume = 0.9f;

    public float AttackRange => attackRange;
    public float StunDuration => stunStatusEffect != null ? stunStatusEffect.Duration : stunDuration;

    public override IEnumerator Execute(ActionContext context)
    {
        if (context == null || context.Player == null)
        {
            yield break;
        }

        // 1. Play basic sword swing animation ("Animacja jak zwykly atak mieczem")
        if (context.Visuals != null)
        {
            context.Visuals.BasicHit();
        }

        // Wait until sword swing reaches contact point
        float halfDuration = Duration * 0.5f;
        yield return new WaitForSeconds(halfDuration);

        // 2. Perform hit detection in front of the player
        PerformStunStrike(context);

        // 3. Complete the rest of the swing
        yield return new WaitForSeconds(halfDuration);
    }

    private void PerformStunStrike(ActionContext context)
    {
        Vector3 origin = context.Player.position + Vector3.up * 0.8f;
        Vector3 direction = context.Player.forward;

        if (!Physics.SphereCast(
            origin,
            attackRadius,
            direction,
            out RaycastHit hit,
            attackRange,
            targetLayer,
            QueryTriggerInteraction.Ignore
        ))
        {
            return;
        }

        ApplyStun(hit.collider.gameObject);
    }

    private void ApplyStun(GameObject targetObj)
    {
        if (targetObj == null)
        {
            return;
        }

        StatusEffectManager statusManager = targetObj.GetComponentInParent<StatusEffectManager>();

        // Ensure target has a StatusEffectManager component
        if (statusManager == null)
        {
            Health targetHealth = targetObj.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                statusManager = targetHealth.gameObject.AddComponent<StatusEffectManager>();
            }
        }

        if (statusManager == null)
        {
            return;
        }

        if (stunStatusEffect != null)
        {
            statusManager.ApplyStatus(stunStatusEffect);
        }
        else
        {
            // Fallback runtime status effect definition (2s duration, speedMultiplier = 0)
            StatusEffectDefinition runtimeStun = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            // Note: runtime instance applied with 2s duration
            statusManager.ApplyStatus(runtimeStun);
        }

        // Play Stunned sound on the stunned enemy
        AudioClip clipToPlay = stunSound != null
            ? stunSound
            : (stunStatusEffect != null ? stunStatusEffect.ApplySound : null);

        if (clipToPlay != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(clipToPlay, targetObj.transform.position, soundVolume);
        }
    }
}
