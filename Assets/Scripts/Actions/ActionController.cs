using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(ActionMemory))]
[RequireComponent(typeof(PlayerActionVisuals))]
public class ActionController : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    private PlayerMovement playerMovement;
    private ActionMemory actionMemory;
    private PlayerActionVisuals actionVisuals;
    private Health health;
    public event Action<float> ActionStarted;

    private void Awake()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        playerMovement = GetComponent<PlayerMovement>();
        actionMemory = GetComponent<ActionMemory>();
        actionVisuals = GetComponent<PlayerActionVisuals>();
        health = GetComponent<Health>();
    }

    public void ExecuteAction(int slot)
    {
        if (!stateMachine.CanAct)
            return;

        ActionDefinition action = actionMemory.GetAction(slot);

        if (action == null)
            return;

        StartCoroutine(ExecuteActionRoutine(slot, action));
    }

    private IEnumerator ExecuteActionRoutine(int slot, ActionDefinition action)
    {
        stateMachine.StartActing();

        Debug.Log($"Started action: {action.DisplayName}");

        ActionStarted?.Invoke(action.Duration);

        actionMemory.ReplaceAction(slot);

        ActionContext context = new ActionContext(transform, playerMovement, health, actionVisuals, action);

        yield return action.Execute(context);

        Debug.Log($"Finished action: {action.DisplayName}");

        stateMachine.FinishActing();
    }
}
