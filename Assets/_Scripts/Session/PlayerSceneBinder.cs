using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Owns the scene-specific camera, movement, and Look input connections
/// for the currently spawned player.
/// </summary>
public sealed class PlayerSceneBinder
{
    private InputAction boundLookAction;
    private CameraFollow boundCamera;

    /// <summary>
    /// Connects a player to the scene's camera and input handling.
    /// If validation fails, no new connections are established.
    /// </summary>
    public bool TryBind(Player player, SceneContext context)
    {
        if (player == null || context == null)
        {
            Debug.LogError("[PlayerSceneBinder] Player and SceneContext are required.");
            return false;
        }

        CameraFollow cameraFollow = context.CameraFollow;

        if (cameraFollow == null)
        {
            Debug.LogError(
                $"[PlayerSceneBinder] Scene '{player.gameObject.scene.name}' "
                    + "has no CameraFollow."
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

        if (lookAction == null)
        {
            Debug.LogError("[PlayerSceneBinder] No 'Look' action was found.", player);
            return false;
        }

        // Remove any previous binding before establishing a new one.
        Unbind();

        cameraFollow.SetTarget(player.transform);

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
            movement.SetMovementReference(cameraFollow.transform);

        boundLookAction = lookAction;
        boundCamera = cameraFollow;

        // This binder owns the Look callback. Do not also configure
        // the same callback through PlayerInput Inspector Unity Events.
        boundLookAction.performed += boundCamera.OnLook;

        return true;
    }

    /// <summary>
    /// Removes the Look callback subscription owned by this binder.
    /// </summary>
    public void Unbind()
    {
        if (boundLookAction != null && boundCamera != null)
            boundLookAction.performed -= boundCamera.OnLook;

        boundLookAction = null;
        boundCamera = null;
    }
}
