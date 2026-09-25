using System;
using System.Collections;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ActionMemory actionMemory;
    [SerializeField] private PlayerActionVisuals actionVisuals;

    public event Action<float> ActionStarted;

    public void ExecuteAction(int slot)
    {
        if (!playerController.CanAct)
        {
            return;
        }

        ActionDefinition action = actionMemory.GetAction(slot);

        if (action == null)
        {
            return;
        }

        StartCoroutine(ExecuteActionRoutine(slot, action));
    }

    private IEnumerator ExecuteActionRoutine(
        int slot,
        ActionDefinition action)
    {
        playerController.StartActing();

        Debug.Log($"Started action: {action.DisplayName}");

        // Tell interested presentation systems that an action has started.
        ActionStarted?.Invoke(action.Duration);

        // Replace immediately so the player can see/plan the next action.
        actionMemory.ReplaceAction(slot);

        ActionContext context = new ActionContext(
            playerController,
            actionVisuals,
            action
        );

        yield return action.Execute(context);

        Debug.Log($"Finished action: {action.DisplayName}");

        playerController.FinishActing();
    }
}