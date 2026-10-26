using UnityEngine;

[CreateAssetMenu(fileName = "TornadoAction", menuName = "RPG/Actions/Attacks/Tornado")]
public class TornadoAction : AttackActionDefinition
{
    public override void OnAnimationImpact(ActionContext context)
    {
        if (context?.Player == null)
            return;

        PerformAreaHit(context, context.Player.AimPosition, HitboxRadius);
    }

    public override string GetDescription()
    {
        if (!DealDamage)
            return $"Applies configured effects within a " + $"{HitboxRadius:0.#}m radius.";

        return $"Spins rapidly, dealing {Damage} damage within a "
            + $"{HitboxRadius:0.#}m radius and applying configured effects.";
    }
}
