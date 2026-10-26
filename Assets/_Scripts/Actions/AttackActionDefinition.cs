using System.Collections.Generic;
using UnityEngine;

public abstract class AttackActionDefinition : ActionDefinition
{
    [Header("Target Detection")]
    [Min(0f)]
    [SerializeField]
    private float hitboxForwardOffset = 1f;

    [Min(0.1f)]
    [SerializeField]
    private float hitboxRadius = 1f;

    [SerializeField]
    private LayerMask targetLayer;

    [Header("Damage")]
    [SerializeField]
    private bool dealDamage = true;

    [Min(0)]
    [SerializeField]
    private int damage = 10;

    [Header("Effects Applied To Targets")]
    [SerializeField]
    private List<EffectDefinition> effectsToApply = new List<EffectDefinition>();

    protected int Damage => damage;
    protected float HitboxRadius => hitboxRadius;
    protected float HitboxForwardOffset => hitboxForwardOffset;
    protected LayerMask TargetLayer => targetLayer;
    protected bool DealDamage => dealDamage;

    public override void OnAnimationImpact(ActionContext context)
    {
        PerformForwardHit(context);
    }

    protected void PerformForwardHit(ActionContext context)
    {
        if (context == null || context.Player == null)
            return;

        Collider[] hits = ActionHitDetector.DetectForward(
            context.Player,
            hitboxForwardOffset,
            hitboxRadius,
            targetLayer,
            QueryTriggerInteraction.Ignore
        );

        ProcessHits(context, hits);
    }

    protected void PerformAreaHit(ActionContext context, Vector3 center, float radius)
    {
        if (context?.Player == null || radius <= 0f)
            return;

        Collider[] hits = ActionHitDetector.DetectRadial(
            center,
            radius,
            targetLayer,
            QueryTriggerInteraction.Ignore
        );

        ProcessHits(context, hits);
    }

    protected void ProcessHits(
        ActionContext context,
        Collider[] hits,
        HashSet<GameObject> alreadyProcessed = null
    )
    {
        if (context == null || context.Player == null || hits == null)
            return;

        HashSet<GameObject> processedThisImpact = alreadyProcessed ?? new HashSet<GameObject>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            GameObject target = hit.gameObject;

            if (target == context.Player || target.transform.IsChildOf(context.Player.transform))
            {
                continue;
            }

            if (!processedThisImpact.Add(target))
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();

            CharacterEffectManager targetEffects = hit.GetComponent<CharacterEffectManager>();

            if (DealDamage && damageable != null)
            {
                Vector3 hitPoint = hit.ClosestPoint(context.Player.transform.position);

                CombatResolver.ResolveAttackHit(
                    context.Player.gameObject,
                    damageable,
                    Damage,
                    hitPoint
                );
            }

            if (targetEffects == null)
                continue;

            foreach (EffectDefinition effect in effectsToApply)
            {
                if (effect == null || effect.Type == EffectType.NextHitBonus)
                {
                    continue;
                }

                targetEffects.ApplyEffect(effect);
            }
        }
    }

    public override string GetDescription()
    {
        if (!dealDamage && effectsToApply.Count == 0)
            return "Attack has no damage or effects configured.";

        if (!dealDamage)
            return "Applies effects to enemies hit.";

        if (effectsToApply.Count == 0)
            return $"Deals {damage} damage to enemies hit.";

        return $"Deals {damage} damage and applies effects to enemies hit.";
    }
}
