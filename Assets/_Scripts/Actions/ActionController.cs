using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(ActionMemory))]
[RequireComponent(typeof(PlayerActionVisuals))]
[RequireComponent(typeof(WeaponBuffManager))]
[RequireComponent(typeof(Health))]
public class ActionController : MonoBehaviour
{
    [Header("Action Settings")]
    [SerializeField, Min(0f)]
    private float actionCooldown = 0.5f;

    private PlayerStateMachine stateMachine;
    private PlayerMovement playerMovement;
    private ActionMemory actionMemory;
    private PlayerActionVisuals actionVisuals;
    private Health health;
    private WeaponBuffManager weaponBuffs;

    private float nextActionTime;
    private Coroutine activeActionRoutine;
    private Coroutine actionExecutionRoutine;
    private bool isExecutingAction;

    public event Action<float> ActionStarted;

    private void Awake()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        playerMovement = GetComponent<PlayerMovement>();
        actionMemory = GetComponent<ActionMemory>();
        actionVisuals = GetComponent<PlayerActionVisuals>();
        health = GetComponent<Health>();
        weaponBuffs = GetComponent<WeaponBuffManager>();
    }

    public void ExecuteAction(int slot)
    {
        if (isExecutingAction || !stateMachine.CanAct || Time.time < nextActionTime)
        {
            return;
        }

        ActionCardInstance card = actionMemory.GetAction(slot);

        if (card == null || card.Definition == null)
            return;

        if (!stateMachine.StartActing())
            return;

        ActionDefinition action = card.Definition;

        isExecutingAction = true;
        nextActionTime = Time.time + actionCooldown;

        ActionStarted?.Invoke(actionCooldown);

        actionMemory.PutOnCooldown(card);
        actionMemory.ReplaceAction(slot);

        ActionContext context = new ActionContext(
            transform,
            playerMovement,
            health,
            actionVisuals,
            action,
            weaponBuffs
        );

        activeActionRoutine = StartCoroutine(ExecuteActionRoutine(action, context));
    }

    private IEnumerator ExecuteActionRoutine(ActionDefinition action, ActionContext context)
    {
        bool executionFinished = false;

        actionExecutionRoutine = StartCoroutine(
            RunAction(action, context, () => executionFinished = true)
        );

        // Temporary acting-state duration until animation events
        // become responsible for signaling action completion.
        yield return new WaitForSeconds(action.Duration);

        // Never allow another action while the previous action's
        // gameplay coroutine is still running.
        while (!executionFinished)
            yield return null;

        FinishCurrentAction();
    }

    private IEnumerator RunAction(ActionDefinition action, ActionContext context, Action onFinished)
    {
        yield return action.Execute(context);
        onFinished?.Invoke();
    }

    private void FinishCurrentAction()
    {
        actionExecutionRoutine = null;
        activeActionRoutine = null;
        isExecutingAction = false;

        stateMachine.FinishActing();
    }

    private void OnDisable()
    {
        CancelCurrentAction();
    }

    public void CancelCurrentAction()
    {
        if (actionExecutionRoutine != null)
        {
            StopCoroutine(actionExecutionRoutine);
            actionExecutionRoutine = null;
        }

        if (activeActionRoutine != null)
        {
            StopCoroutine(activeActionRoutine);
            activeActionRoutine = null;
        }

        // Always restore movement, even if no action coroutine is running.
        playerMovement.CancelChargeMovement();
        playerMovement.SetMovementOverridden(false);

        isExecutingAction = false;
        stateMachine?.FinishActing();
    }
}
