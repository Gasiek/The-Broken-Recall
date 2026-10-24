using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicHitAction", menuName = "RPG/Actions/Basic Hit")]
public class BasicHitAction : ActionDefinition
{
    [SerializeField]
    private int damage = 10;

    [Header("Hitbox")]
    [SerializeField]
    private float hitboxOffset = 0.8f;

    [SerializeField]
    private float hitboxRadius = 1.1f;

    [SerializeField]
    private LayerMask targetLayer;

    public override void OnAnimationImpact(ActionContext context)
    {
        DealDamage(context);
    }

    private void DealDamage(ActionContext context)
    {
        if (context == null || context.Player == null)
            return;

        Collider[] hits = ActionHitDetector.DetectForward(
            context.Player,
            hitboxOffset,
            hitboxRadius,
            targetLayer
        );

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null || !damagedTargets.Add(damageable))
                continue;

            Vector3 hitPoint = hit.ClosestPoint(context.Player.AimPosition);

            CombatResolver.ResolveAttackHit(
                context.Player.gameObject,
                context.WeaponBuffs,
                damageable,
                damage,
                hitPoint
            );
        }
    }

    public override string GetDescription() =>
        $"Delivers a quick slash dealing {damage} damage to an enemy in front.";
}
