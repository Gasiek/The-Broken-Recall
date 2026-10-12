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

    [Range(0f, 1f)]
    [SerializeField]
    private float soundVolume = 0.9f;

    public float AttackRange => attackRange;
    public float StunDuration => stunStatusEffect.duration;

    public override IEnumerator Execute(ActionContext context)
    {
        context.Visuals.BasicHit();

        float halfDuration = Duration * 0.5f;
        yield return new WaitForSeconds(halfDuration);

        PerformStunStrike(context);

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

        StatusEffectManager statusManager = targetObj.GetComponent<StatusEffectManager>();
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
            Debug.LogWarning("No stun status effect applied.");
        }

        // Play Stunned sound on the stunned enemy
        AudioClip clipToPlay = stunStatusEffect != null ? stunStatusEffect.ApplySound : null;
        if (clipToPlay != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(clipToPlay, targetObj.transform.position, soundVolume);
        }
    }
}
