using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BlockAction",
    menuName = "RPG/Actions/Block"
)]
public class BlockAction : ActionDefinition
{
    public override IEnumerator Execute(ActionContext context)
    {
        context.Visuals.Block(Duration);

        yield return new WaitForSeconds(Duration);
    }
}