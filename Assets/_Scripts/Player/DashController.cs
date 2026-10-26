using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(ActionController))]
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerMovement))]
public class DashController : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    private PlayerDefinition playerDefinition;
    private CharacterController characterController;
    private ActionController actionController;
    private PlayerMovement playerMovement;
    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;

    public event Action DashStarted;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        stateMachine = GetComponent<PlayerStateMachine>();
        actionController = GetComponent<ActionController>();
        playerMovement = GetComponent<PlayerMovement>();
        Player player = GetComponent<Player>();
        playerDefinition = player.Definition;

        if (playerDefinition == null)
            Debug.LogError("DashController has no PlayerDefinition assigned.");
    }

    private void Update()
    {
        UpdateCooldown();

        if (stateMachine.CurrentState == PlayerState.Dashing)
        {
            UpdateDash();
        }
    }

    public void Dash()
    {
        if (
            stateMachine.CurrentState == PlayerState.Dashing
            || stateMachine.CurrentState == PlayerState.Sleeping
            || stateMachine.CurrentState == PlayerState.Dead
            || cooldownTimer > 0f
        )
        {
            return;
        }

        // Cancel any action before starting the dash.
        if (stateMachine.CurrentState == PlayerState.Acting)
        {
            actionController.CancelCurrentAction();
        }

        // Cancellation should have returned the player to Normal.
        if (!stateMachine.CanDash)
            return;

        if (
            playerDefinition == null
            || playerDefinition.DashDuration <= 0f
            || playerDefinition.DashDistance <= 0f
        )
        {
            return;
        }

        dashDirection = transform.forward;
        dashTimer = playerDefinition.DashDuration;
        cooldownTimer = playerDefinition.DashCooldown;

        stateMachine.StartDashing();

        DashStarted?.Invoke();
    }

    private void UpdateDash()
    {
        float dashSpeed = playerDefinition.DashDistance / playerDefinition.DashDuration;

        characterController.Move(dashDirection * dashSpeed * Time.deltaTime);

        playerMovement.ApplyGravityOnly();

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
            EndDash();
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0f)
        {
            cooldownTimer = 0f;
        }
    }

    private void EndDash()
    {
        stateMachine.FinishDashing();
    }
}
