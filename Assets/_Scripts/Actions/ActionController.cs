using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(ActionMemory))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ActionRoutineRunner))]
public class ActionController : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    private PlayerMovement playerMovement;
    private Player player;
    private ActionMemory actionMemory;
    private Health health;
    private ActionRoutineRunner routineRunner;

    private bool isExecutingAction;
    private bool actionFinished;

    private ActionDefinition currentAction;
    private ActionContext currentContext;

    public ActionDefinition CurrentAction => currentAction;
    public bool IsExecutingAction => isExecutingAction;

    public event Action ActionStarted;

    private void Awake()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        playerMovement = GetComponent<PlayerMovement>();
        player = GetComponent<Player>();
        actionMemory = GetComponent<ActionMemory>();
        health = GetComponent<Health>();
        routineRunner = GetComponent<ActionRoutineRunner>();
    }

    public void ExecuteAction(int slot)
    {
        if (isExecutingAction || !stateMachine.CanAct)
            return;

        ActionCardInstance card = actionMemory.GetAction(slot);

        if (card == null || card.Definition == null)
            return;

        if (!stateMachine.StartActing())
            return;

        ActionDefinition action = card.Definition;

        ActionContext context = new ActionContext(player, playerMovement, health, action, this, routineRunner);

        // Establish the active execution before invoking callbacks.
        currentAction = action;
        currentContext = context;
        isExecutingAction = true;
        actionFinished = false;

        ActionStarted?.Invoke();

        actionMemory.PutOnCooldown(card);
        actionMemory.ReplaceAction(slot);

        // Start any immediate action effects.
        action.OnStarted(context);
    }

    // Called by PlayerAnimationEvents at the appropriate
    // frame of the active animation.
    public void HandleAnimationImpact()
    {
        if (!isExecutingAction || actionFinished || currentAction == null || currentContext == null)
        {
            return;
        }

        currentAction.OnAnimationImpact(currentContext);
    }

    public void HandleAnimationFinished()
    {
        FinishCurrentAction(null);
    }

    public void CompleteAction(ActionContext expectedContext)
    {
        if (expectedContext == null)
            return;

        FinishCurrentAction(expectedContext);
    }

    private void FinishCurrentAction(ActionContext expectedContext)
    {
        if (!isExecutingAction || actionFinished || currentAction == null || currentContext == null)
        {
            return;
        }

        // A timed action may only finish the execution that
        // started its timer, never a newer execution.
        if (expectedContext != null && !ReferenceEquals(currentContext, expectedContext))
        {
            return;
        }

        actionFinished = true;

        ActionDefinition finishedAction = currentAction;
        ActionContext finishedContext = currentContext;

        currentAction = null;
        currentContext = null;
        isExecutingAction = false;

        finishedAction.OnAnimationFinished(finishedContext);

        stateMachine.FinishActing();
    }

    public void HandleHealSittingStarted()
    {
        if (!isExecutingAction || actionFinished || currentAction == null || currentContext == null)
            return;

        currentAction.OnHealSittingStarted(currentContext);
    }

    private void OnDisable()
    {
        CancelCurrentAction();
    }

    public void CancelCurrentAction()
    {
        ActionDefinition cancelledAction = currentAction;
        ActionContext cancelledContext = currentContext;

        bool hadActiveAction = isExecutingAction;

        // Invalidate the execution before cleanup.
        currentAction = null;
        currentContext = null;
        isExecutingAction = false;
        actionFinished = true;

        if (hadActiveAction && cancelledAction != null && cancelledContext != null)
        {
            cancelledAction.OnCancelled(cancelledContext);
        }

        // Always restore movement, even if no action
        // is currently executing.
        if (playerMovement != null)
        {
            playerMovement.CancelChargeMovement();
            playerMovement.SetMovementOverridden(false);
        }

        stateMachine?.FinishActing();
    }
}
