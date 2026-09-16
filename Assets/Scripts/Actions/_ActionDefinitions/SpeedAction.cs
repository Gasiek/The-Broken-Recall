using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SpeedAction",
    menuName = "RPG/Actions/Speed"
)]
public class SpeedAction : ActionDefinition
{
    [SerializeField] private float speedMultiplier = 2f;

    public override IEnumerator Execute(ActionContext context)
    {
        context.Player.SetSpeedMultiplier(speedMultiplier);

        yield return new WaitForSeconds(Duration);

        context.Player.ResetSpeedMultiplier();
    }
}