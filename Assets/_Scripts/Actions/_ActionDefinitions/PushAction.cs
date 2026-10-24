using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PushAction", menuName = "RPG/Actions/Push")]
public class PushAction : ActionDefinition
{
    [SerializeField]
    private int damage = 10;

    [Header("Hitbox")]
    [SerializeField]
    private float hitboxOffset = 0.8f;

    [SerializeField]
    private float hitboxRadius = 1.1f;

    [SerializeField]
    private float knockbackDistance = 2f;

    [SerializeField]
    private LayerMask targetLayer;

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context == null || context.Player == null)
            return;

        DealDamageAndPush(context);
    }

    private void DealDamageAndPush(ActionContext context)
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
        HashSet<IPushable> pushedTargets = new HashSet<IPushable>();

        Vector3 pushDirection = Vector3
            .ProjectOnPlane(context.Player.transform.forward, Vector3.up)
            .normalized;

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable != null && damagedTargets.Add(damageable))
            {
                CombatResolver.ResolveAttackHit(
                    context.Player.gameObject,
                    context.WeaponBuffs,
                    damageable,
                    damage,
                    hit.ClosestPoint(context.Player.transform.position)
                );
            }

            IPushable pushable = hit.GetComponent<IPushable>();

            if (pushable != null && pushedTargets.Add(pushable))
            {
                pushable.Push(pushDirection, knockbackDistance);
            }
        }
    }

    public override string GetDescription() =>
        $"Thrusts forward, dealing {damage} damage and pushing the target back {knockbackDistance:0.#}m.";
}
