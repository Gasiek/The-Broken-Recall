using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class SceneContext : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField]
    private CameraFollow cameraFollow;

    [SerializeField]
    private SceneSpawnPoint[] spawnPoints;

    [SerializeField]
    private InteractionPromptUI interactionPrompt;

    [SerializeField]
    private SkillLearnUI skillLearnUI;

    [SerializeField]
    private SkillRemovalUI skillRemovalUI;

    public CameraFollow CameraFollow => cameraFollow;
    public InteractionPromptUI InteractionPrompt => interactionPrompt;

    public SceneSpawnPoint FindSpawnPoint(SceneDestination destination)
    {
        if (destination == null || spawnPoints == null)
            return null;

        foreach (SceneSpawnPoint point in spawnPoints)
        {
            if (point != null && point.Destination == destination)
                return point;
        }

        return null;
    }

    public SceneSpawnPoint GetDefaultSpawnPoint()
    {
        if (spawnPoints == null)
            return null;

        // Prefer a spawn point explicitly named DefaultSpawn.
        foreach (SceneSpawnPoint point in spawnPoints)
        {
            if (
                point != null
                && string.Equals(
                    point.gameObject.name,
                    "DefaultSpawn",
                    System.StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return point;
            }
        }

        // Otherwise, return the first valid entry.
        foreach (SceneSpawnPoint point in spawnPoints)
        {
            if (point != null)
                return point;
        }

        return null;
    }

    public void BindPlayerUIReferences(Player player)
    {
        if (player == null)
            return;

        PlayerInput input = player.GetComponent<PlayerInput>();

        if (input == null)
        {
            Debug.LogWarning(
                "[SceneContext] The spawned player has no PlayerInput component.",
                this
            );
            return;
        }

        if (skillLearnUI != null)
            skillLearnUI.BindRuntimeReferences(cameraFollow, input);

        if (skillRemovalUI != null)
            skillRemovalUI.BindRuntimeReferences(cameraFollow, input);
    }

    private void Awake()
    {
        if (cameraFollow == null)
        {
            Debug.LogError("[SceneContext] Assign CameraFollow.", this);
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SceneContext] Assign the scene's spawn points.", this);
        }
        else
        {
            ValidateSpawnPoints();
        }
    }

    private void ValidateSpawnPoints()
    {
        HashSet<SceneDestination> seenDestinations = new HashSet<SceneDestination>();

        bool hasValidSpawn = false;

        foreach (SceneSpawnPoint point in spawnPoints)
        {
            if (point == null)
            {
                Debug.LogWarning(
                    "[SceneContext] The spawn-point array contains an empty entry.",
                    this
                );
                continue;
            }

            hasValidSpawn = true;

            SceneDestination destination = point.Destination;

            if (destination == null)
            {
                Debug.LogWarning(
                    $"[SceneContext] Spawn point '{point.name}' has no SceneDestination assigned.",
                    point
                );
                continue;
            }

            if (!seenDestinations.Add(destination))
            {
                Debug.LogError(
                    $"[SceneContext] Duplicate destination '{destination.name}'. "
                        + "Each destination should identify only one spawn point per scene.",
                    point
                );
            }

            if (destination.SceneName != gameObject.scene.name)
            {
                Debug.LogWarning(
                    $"[SceneContext] Destination '{destination.name}' targets scene "
                        + $"'{destination.SceneName}', but this spawn point is in "
                        + $"'{gameObject.scene.name}'.",
                    point
                );
            }
        }

        if (!hasValidSpawn)
        {
            Debug.LogError("[SceneContext] No valid spawn-point references are assigned.", this);
        }
    }
}
