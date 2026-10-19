using System;
using UnityEngine;

/// <summary>
/// Creates and initializes a player for a scene.
/// SceneTransitionManager remains responsible for transition timing
/// and GameSession registration.
/// </summary>
public sealed class PlayerSpawner
{
    private readonly GameObject playerPrefab;
    private readonly PlayerSceneBinder sceneBinder;

    public PlayerSpawner(GameObject playerPrefab, PlayerSceneBinder sceneBinder)
    {
        this.playerPrefab = playerPrefab;
        this.sceneBinder = sceneBinder;
    }

    /// <summary>
    /// Spawns and initializes a player at the specified spawn point.
    /// Returns the initialized Player, or null if setup fails.
    /// </summary>
    public Player TrySpawn(
        SceneContext context,
        SceneSpawnPoint spawnPoint,
        PlayerSaveData savedData
    )
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Player prefab is not assigned.");
            return null;
        }

        if (sceneBinder == null)
        {
            Debug.LogError("[PlayerSpawner] PlayerSceneBinder is missing.");
            return null;
        }

        if (context == null)
        {
            Debug.LogError("[PlayerSpawner] SceneContext is missing.");
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                $"[PlayerSpawner] No spawn point was provided for scene '{context.gameObject.scene.name}'."
            );
            return null;
        }

        GameObject playerObject = null;
        bool bindingSucceeded = false;

        try
        {
            playerObject = UnityEngine.Object.Instantiate(
                playerPrefab,
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );

            Player player = playerObject.GetComponent<Player>();

            if (player == null)
            {
                Debug.LogError(
                    "[PlayerSpawner] The player prefab has no Player component.",
                    playerObject
                );

                DestroyFailedPlayer(playerObject, false);
                return null;
            }

            playerObject.name = playerPrefab.name;

            if (!sceneBinder.TryBind(player, context))
            {
                Debug.LogError(
                    "[PlayerSpawner] Failed to bind the player to the scene.",
                    playerObject
                );

                DestroyFailedPlayer(playerObject, false);
                return null;
            }

            bindingSucceeded = true;

            // Connect scene-specific UI references.
            context.BindPlayerUIReferences(player);

            // Connect the scene's interaction prompt.
            InteractionController interactionController =
                player.GetComponent<InteractionController>();

            if (interactionController != null)
            {
                interactionController.SetInteractionPrompt(context.InteractionPrompt);
            }

            // Restore saved state after the player is initialized.
            if (savedData != null)
            {
                player.RestoreState(savedData);
            }

            // Resume timers for the newly spawned player.
            if (player.WeaponBuffs != null)
            {
                player.WeaponBuffs.ResumeBuffTimer();
            }

            return player;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            DestroyFailedPlayer(playerObject, bindingSucceeded);
            return null;
        }
    }

    private void DestroyFailedPlayer(GameObject playerObject, bool bindingSucceeded)
    {
        if (bindingSucceeded && sceneBinder != null)
        {
            sceneBinder.Unbind();
        }

        if (playerObject != null)
        {
            UnityEngine.Object.Destroy(playerObject);
        }
    }
}
