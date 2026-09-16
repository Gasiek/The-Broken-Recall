using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PushAction",
    menuName = "Actions/Push"
)]
public class PushAction : ActionDefinition
{
    public override IEnumerator Execute(ActionContext context)
    {
        context.Visuals.Push();

        yield return new WaitForSeconds(Duration);
    }
}