using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TornadoAction",
    menuName = "Actions/Tornado"
)]
public class TornadoAction : ActionDefinition
{
    public override IEnumerator Execute(ActionContext context)
    {
        context.Visuals.Tornado();

        yield return new WaitForSeconds(Duration);
    }
}