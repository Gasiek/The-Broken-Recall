using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerSceneBinder
{
    private InputAction boundLookAction;
    private CameraFollow boundCamera;

    private InputAction boundPauseAction;
    private PauseController boundPauseController;
    private PlayerInput boundPlayerInput;

    private CardsMenuUI boundCardsMenuUI;
    private ActionCollection boundActionCollection;

    public bool TryBind(Player player, SceneContext context)
    {
        if (player == null || context == null)
        {
            Debug.LogError("[PlayerSceneBinder] Player and SceneContext are required.");
            return false;
        }

        CameraFollow cameraFollow = context.CameraFollow;
        PauseController pauseController = context.PauseController;
        CardsMenuUI cardsMenuUI = context.CardsMenuUI;

        if (cameraFollow == null)
        {
            Debug.LogError(
                $"[PlayerSceneBinder] Scene '{player.gameObject.scene.name}' "
                    + "has no CameraFollow."
            );
            return false;
        }

        if (pauseController == null || cardsMenuUI == null)
        {
            Debug.LogError(
                $"[PlayerSceneBinder] Scene '{player.gameObject.scene.name}' "
                    + "must provide PauseController and CardsMenuUI."
            );
            return false;
        }

        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("[PlayerSceneBinder] Player prefab has no PlayerInput.", player);
            return false;
        }

        if (playerInput.actions == null)
        {
            Debug.LogError("[PlayerSceneBinder] PlayerInput has no Input Actions asset.", player);
            return false;
        }

        InputAction lookAction = playerInput.actions.FindAction("Look", false);

        InputAction pauseAction = playerInput.actions.FindAction("Player/Pause", false);

        if (lookAction == null)
        {
            Debug.LogError("[PlayerSceneBinder] No 'Look' action was found.", player);
            return false;
        }

        if (pauseAction == null)
        {
            Debug.LogError("[PlayerSceneBinder] No 'Player/Pause' action was found.", player);
            return false;
        }

        ActionCollection actionCollection = player.GetComponent<ActionCollection>();

        if (actionCollection == null)
        {
            Debug.LogError("[PlayerSceneBinder] Player has no ActionCollection.", player);
            return false;
        }

        // Clear previous subscriptions before binding the new player.
        Unbind();

        cameraFollow.SetTarget(player.transform);

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.SetMovementReference(cameraFollow.transform);
        }

        boundLookAction = lookAction;
        boundCamera = cameraFollow;
        boundLookAction.performed += boundCamera.OnLook;

        boundPlayerInput = playerInput;
        boundPauseController = pauseController;
        boundPauseAction = pauseAction;

        boundPauseController.BindPlayer(boundPlayerInput);
        boundPauseAction.performed += boundPauseController.OnPause;

        boundCardsMenuUI = cardsMenuUI;
        boundActionCollection = actionCollection;
        boundCardsMenuUI.BindActionCollection(boundActionCollection);

        return true;
    }

    public void Unbind()
    {
        if (boundLookAction != null && boundCamera != null)
        {
            boundLookAction.performed -= boundCamera.OnLook;
        }

        if (boundPauseAction != null && boundPauseController != null)
        {
            boundPauseAction.performed -= boundPauseController.OnPause;
        }

        if (boundPauseController != null && boundPlayerInput != null)
        {
            boundPauseController.UnbindPlayer(boundPlayerInput);
        }

        if (boundCardsMenuUI != null && boundActionCollection != null)
        {
            boundCardsMenuUI.UnbindActionCollection(boundActionCollection);
        }

        boundLookAction = null;
        boundCamera = null;

        boundPauseAction = null;
        boundPauseController = null;
        boundPlayerInput = null;

        boundCardsMenuUI = null;
        boundActionCollection = null;
    }
}
