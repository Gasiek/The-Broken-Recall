using System;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField]
    private PauseController pauseController;

    [SerializeField]
    private CardsMenuUI cardsMenuUI;

    public CameraFollow CameraFollow => cameraFollow;
    public InteractionPromptUI InteractionPrompt => interactionPrompt;
    public PauseController PauseController => pauseController;
    public CardsMenuUI CardsMenuUI => cardsMenuUI;

    public SkillLearnUI SkillLearnUI => skillLearnUI;
    public SkillRemovalUI SkillRemovalUI => skillRemovalUI;

    /// <summary>
    /// Finds the spawn point associated with the supplied destination asset.
    /// </summary>
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

    /// <summary>
    /// Returns DefaultSpawn if present; otherwise returns the first valid entry.
    /// </summary>
    public SceneSpawnPoint GetDefaultSpawnPoint()
    {
        if (spawnPoints == null)
            return null;

        foreach (SceneSpawnPoint point in spawnPoints)
        {
            if (
                point != null
                && string.Equals(
                    point.gameObject.name,
                    "DefaultSpawn",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return point;
            }
        }

        foreach (SceneSpawnPoint point in spawnPoints)
        {
            if (point != null)
                return point;
        }

        return null;
    }

    /// <summary>
    /// Validates required scene references and spawn-point configuration.
    /// </summary>
    public bool ValidateConfiguration()
    {
        bool isValid = true;

        if (cameraFollow == null)
        {
            Debug.LogError("[SceneContext] CameraFollow is not assigned.", this);

            isValid = false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SceneContext] No spawn points are assigned.", this);

            return false;
        }

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

            if (point.gameObject.scene != gameObject.scene)
            {
                Debug.LogError(
                    $"[SceneContext] Spawn point '{point.name}' belongs to a different scene.",
                    point
                );

                isValid = false;
                continue;
            }

            SceneDestination destination = point.Destination;

            if (destination == null)
            {
                Debug.LogWarning(
                    $"[SceneContext] Spawn point '{point.name}' has no "
                        + "SceneDestination assigned.",
                    point
                );

                // Destination-less points may still be fallback spawn points.
                continue;
            }

            if (!seenDestinations.Add(destination))
            {
                Debug.LogError(
                    $"[SceneContext] Destination '{destination.name}' is assigned "
                        + "to more than one spawn point in this scene.",
                    point
                );

                isValid = false;
            }

            if (destination.SceneName != gameObject.scene.name)
            {
                Debug.LogError(
                    $"[SceneContext] Destination '{destination.name}' targets "
                        + $"scene '{destination.SceneName}', but this context is in "
                        + $"'{gameObject.scene.name}'.",
                    point
                );

                isValid = false;
            }
        }

        if (!hasValidSpawn)
        {
            Debug.LogError("[SceneContext] No valid spawn-point references are assigned.", this);

            isValid = false;
        }

        return isValid;
    }

    private void Awake()
    {
        ValidateConfiguration();
    }
}
