using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicHitAction", menuName = "Actions/Basic Hit")]
public class BasicHitAction : ActionDefinition
{
    public override IEnumerator Execute(ActionContext context)
    {
        context.Visuals.BasicHit();

        yield return new WaitForSeconds(Duration);
    }
}
