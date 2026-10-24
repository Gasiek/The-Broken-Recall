using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TornadoAction", menuName = "RPG/Actions/Tornado")]
public class TornadoAction : ActionDefinition
{
    [SerializeField]
    private int damage = 10;

    [Header("Hitbox")]
    [SerializeField]
    private float hitboxRadius = 1.1f;

    [SerializeField]
    private LayerMask targetLayer;

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context == null || context.Player == null)
            return;

        DealDamage(context);
    }

    private void DealDamage(ActionContext context)
    {
        if (context == null || context.Player == null)
            return;

        Collider[] hits = ActionHitDetector.DetectRadial(context.Player.AimPosition, hitboxRadius, targetLayer);

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null || !damagedTargets.Add(damageable))
                continue;

            CombatResolver.ResolveAttackHit(
                context.Player.gameObject,
                context.WeaponBuffs,
                damageable,
                damage,
                hit.ClosestPoint(context.Player.AimPosition)
            );
        }
    }

    public override string GetDescription() =>
        $"Spins rapidly, dealing {damage} damage to all enemies within a {hitboxRadius:0.#}m radius.";
}
