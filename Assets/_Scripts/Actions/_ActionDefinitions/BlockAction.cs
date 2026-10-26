using UnityEngine;

[CreateAssetMenu(fileName = "BlockAction", menuName = "RPG/Actions/Block")]
public class BlockAction : ActionDefinition
{
    public override void OnStarted(ActionContext context)
    {
        context.Health.StartInvincibility();
    }

    public override void OnAnimationFinished(ActionContext context)
    {
        context.Health.StopInvincibility();
    }

    public override void OnCancelled(ActionContext context)
    {
        context.Health.StopInvincibility();
    }

    public override string GetDescription() =>
        $"Raises a guard, granting complete invincibility against damage.";
}
