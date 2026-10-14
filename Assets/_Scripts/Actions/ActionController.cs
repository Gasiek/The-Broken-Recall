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
        if (!stateMachine.CanAct)
        {
            return;
        }

        ActionCardInstance card = actionMemory.GetAction(slot);

        if (card == null || card.Definition == null)
        {
            return;
        }

        StartCoroutine(ExecuteActionRoutine(slot, card));
    }

    private IEnumerator ExecuteActionRoutine(int slot, ActionCardInstance card)
    {
        ActionDefinition action = card.Definition;

        stateMachine.StartActing();

        Debug.Log($"Started action: {action.DisplayName}");

        ActionStarted?.Invoke(actionCooldown);

        // Apply cooldown to this copy, not to the shared definition.
        actionMemory.PutOnCooldown(card);

        // Replace the used card immediately.
        actionMemory.ReplaceAction(slot);

        ActionContext context = new ActionContext(
            transform,
            playerMovement,
            health,
            actionVisuals,
            action,
            weaponBuffs
        );

        // The action runs independently of the input cooldown.
        StartCoroutine(action.Execute(context));

        yield return new WaitForSeconds(actionCooldown);

        stateMachine.FinishActing();
    }
}
