using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(ActionMemory))]
[RequireComponent(typeof(PlayerActionVisuals))]
[RequireComponent(typeof(WeaponBuffManager))]
public class ActionController : MonoBehaviour
{
    [Header("Action Settings")]
    [SerializeField]
    private float actionCooldown = 0.5f;

    private PlayerStateMachine stateMachine;
    private PlayerMovement playerMovement;
    private ActionMemory actionMemory;
    private PlayerActionVisuals actionVisuals;
    private Health health;
    private WeaponBuffManager weaponBuffs;
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
        // Player must be in a state where actions are allowed.
        // This also prevents actions during the cooldown.
        if (!stateMachine.CanAct)
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

    private IEnumerator ExecuteActionRoutine(int slot, ActionDefinition action)
    {
        // Lock action/dash input for the cooldown duration.
        stateMachine.StartActing();

        Debug.Log($"Started action: {action.DisplayName}");

        // Tell the UI to start the global action cooldown.
        ActionStarted?.Invoke(actionCooldown);

        // Immediately replace the used action in memory.
        actionMemory.ReplaceAction(slot);

        ActionContext context = new ActionContext(
            transform,
            playerMovement,
            health,
            actionVisuals,
            action,
            weaponBuffs
        );

        // Start the actual action independently.
        // We intentionally do NOT yield here because the action
        // may have a much longer duration than the cooldown.
        StartCoroutine(action.Execute(context));

        // Acting only lasts for the global action cooldown.
        yield return new WaitForSeconds(actionCooldown);

        // Player can now start another action or dash.
        stateMachine.FinishActing();
    }
}
