using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "TornadoAction", menuName = "RPG/Actions/Tornado")]
public class TornadoAction : ActionDefinition
{
    [SerializeField]
    private int damage = 10;

    [SerializeField]
    private float radius = 1.5f;

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
        Vector3 center = context.Player.position;

        Collider[] hits = Physics.OverlapSphere(center, radius, targetLayer);

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null)
            {
                continue;
            }

            CombatResolver.ResolveAttackHit(
                context.Player.gameObject,
                context.WeaponBuffs,
                damageable,
                damage,
                hit.transform.position
            );
        }
    }

    public override string GetDescription() =>
        $"Spins rapidly, dealing {damage} damage to all enemies within a {radius:0.#}m radius.";
}
