using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1200)]
public sealed class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions inputActions;

    private Player boundPlayer;
    private PlayerMovement boundMovement;
    private PlayerInputHandler boundInputHandler;
    private CameraFollow boundCamera;
    private PauseController boundPauseController;

    private bool playerBound;
    private bool uiMapActive;

    public InputSystem_Actions Actions => inputActions;
    public bool IsUIMapActive => uiMapActive;
    public bool IsPlayerBound => playerBound;

    public event Action<Vector2> MoveChanged;
    public event Action<Vector2> LookChanged;
    public event Action CancelRequested;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new InputSystem_Actions();
        SubscribeActions();
        EnablePlayerMap();
    }

    private void SubscribeActions()
    {
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Player.ActionSlot1.performed += OnActionSlot1;
        inputActions.Player.ActionSlot2.performed += OnActionSlot2;
        inputActions.Player.ActionSlot3.performed += OnActionSlot3;
        inputActions.Player.Dash.performed += OnDash;
        inputActions.Player.Pause.performed += OnPause;

        inputActions.UI.Cancel.performed += OnCancel;
    }

    private void UnsubscribeActions()
    {
        if (inputActions == null)
            return;

        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;

        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.ActionSlot1.performed -= OnActionSlot1;
        inputActions.Player.ActionSlot2.performed -= OnActionSlot2;
        inputActions.Player.ActionSlot3.performed -= OnActionSlot3;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.Pause.performed -= OnPause;

        inputActions.UI.Cancel.performed -= OnCancel;
    }

    public bool BindPlayer(
        Player player,
        CameraFollow cameraFollow,
        PauseController pauseController
    )
    {
        if (player == null)
        {
            Debug.LogError("[InputManager] Cannot bind a null player.", this);
            return false;
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerInputHandler inputHandler = player.GetComponent<PlayerInputHandler>();

        if (movement == null || inputHandler == null)
        {
            Debug.LogError(
                "[InputManager] Player requires PlayerMovement and "
                    + "PlayerInputHandler before it can be bound.",
                player
            );
            return false;
        }

        if (cameraFollow == null || pauseController == null)
        {
            Debug.LogError("[InputManager] CameraFollow and PauseController are required.", player);
            return false;
        }

        UnbindPlayer();

        boundPlayer = player;
        boundMovement = movement;
        boundInputHandler = inputHandler;
        boundCamera = cameraFollow;
        boundPauseController = pauseController;

        boundCamera.SetTarget(player.transform);
        boundMovement.SetMovementReference(boundCamera.transform);

        playerBound = true;
        EnablePlayerMap();

        return true;
    }

    public void UnbindPlayer()
    {
        ClearMovementInput();

        boundPlayer = null;
        boundMovement = null;
        boundInputHandler = null;
        boundCamera = null;
        boundPauseController = null;

        playerBound = false;
    }

    public void EnablePlayerMap()
    {
        if (inputActions == null)
            return;

        inputActions.UI.Disable();
        inputActions.Player.Enable();

        uiMapActive = false;
        ClearMovementInput();
    }

    public void EnableUIMap()
    {
        if (inputActions == null)
            return;

        ClearMovementInput();

        inputActions.Player.Disable();
        inputActions.UI.Enable();

        uiMapActive = true;
    }

    private void ClearMovementInput()
    {
        boundMovement?.SetMoveInput(Vector2.zero);

        MoveChanged?.Invoke(Vector2.zero);
        LookChanged?.Invoke(Vector2.zero);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();

        if (!playerBound || uiMapActive)
            value = Vector2.zero;

        boundMovement?.SetMoveInput(value);
        MoveChanged?.Invoke(value);
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        if (!playerBound || uiMapActive)
            return;

        Vector2 value = context.ReadValue<Vector2>();

        boundCamera?.Look(value);
        LookChanged?.Invoke(value);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (CanReceiveGameplayInput())
            boundInputHandler.OnInteract(context);
    }

    private void OnActionSlot1(InputAction.CallbackContext context)
    {
        if (CanReceiveGameplayInput())
            boundInputHandler.OnActionSlot1(context);
    }

    private void OnActionSlot2(InputAction.CallbackContext context)
    {
        if (CanReceiveGameplayInput())
            boundInputHandler.OnActionSlot2(context);
    }

    private void OnActionSlot3(InputAction.CallbackContext context)
    {
        if (CanReceiveGameplayInput())
            boundInputHandler.OnActionSlot3(context);
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (CanReceiveGameplayInput())
            boundInputHandler.OnDash(context);
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (CanReceiveGameplayInput())
            boundPauseController.TogglePause();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (uiMapActive)
            CancelRequested?.Invoke();
    }

    private bool CanReceiveGameplayInput()
    {
        return playerBound
            && !uiMapActive
            && boundPlayer != null
            && boundInputHandler != null
            && boundPauseController != null;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        UnsubscribeActions();

        if (inputActions != null)
        {
            inputActions.Disable();
            inputActions.Dispose();
            inputActions = null;
        }

        Instance = null;
    }
}
