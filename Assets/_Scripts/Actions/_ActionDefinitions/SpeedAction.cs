using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedAction", menuName = "RPG/Actions/Speed")]
public class SpeedAction : ActionDefinition
{
    [SerializeField, Min(1f)]
    private float speedMultiplier = 2f;

    public override void OnStarted(ActionContext context)
    {
        if (context?.Player == null || context.Movement == null)
            return;

        context.Movement.SetSpeedMultiplier(speedMultiplier);

        GetRoutineRunner(context).Run(SpeedRoutine(context));
    }

    private IEnumerator SpeedRoutine(ActionContext context)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, Duration));

        if (context?.Player == null)
            yield break;

        ActionController controller = context.Player.GetComponent<ActionController>();

        if (controller != null)
            controller.CompleteTimedAction(context);
    }

    public override void OnAnimationFinished(ActionContext context)
    {
        ResetSpeed(context);
    }

    public override void OnCancelled(ActionContext context)
    {
        if (context?.Player != null)
            GetRoutineRunner(context).StopAllActionRoutines();

        ResetSpeed(context);
    }

    private void ResetSpeed(ActionContext context)
    {
        if (context?.Movement != null)
            context.Movement.ResetSpeedMultiplier();
    }

    private ActionRoutineRunner GetRoutineRunner(ActionContext context)
    {
        return context.Player.GetComponent<ActionRoutineRunner>();
    }

    public override string GetDescription() =>
        $"Increases movement speed by "
        + $"{Mathf.RoundToInt((speedMultiplier - 1f) * 100)}% "
        + $"for {Duration:0.#}s.";
}
