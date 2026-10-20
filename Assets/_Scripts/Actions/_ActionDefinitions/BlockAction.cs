using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockAction", menuName = "RPG/Actions/Block")]
public class BlockAction : ActionDefinition
{
    public override IEnumerator Execute(ActionContext context)
    {
        context.Health.StartInvincibility();

        yield return new WaitForSeconds(Duration);

        context.Health.StopInvincibility();
    }

    public override string GetDescription() =>
        $"Raises a guard, granting complete invincibility against damage for {Duration:0.#}s.";
}
