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
        context.Movement.SetSpeedMultiplier(speedMultiplier);

        yield return new WaitForSeconds(Duration);

        context.Movement.ResetSpeedMultiplier();
    }

    public override string GetDescription() =>
        $"Increases movement speed by {Mathf.RoundToInt((speedMultiplier - 1f) * 100)}% for {Duration:0.#}s.";
}