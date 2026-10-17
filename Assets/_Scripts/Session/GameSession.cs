using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1100)]
public sealed class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public PlayerSaveData PlayerData { get; private set; }
    public Player CurrentPlayer { get; private set; }
    public string CurrentSceneName { get; private set; }
    public string CurrentSpawnPointId { get; private set; }

    private readonly HashSet<string> deadEnemyIds = new();
    private readonly HashSet<string> completedEncounterIds = new();

    public event Action<Player> PlayerSpawned;
    public event Action<Player> PlayerDespawning;

    public bool TransitionInProgress { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentSceneName = SceneManager.GetActiveScene().name;
    }

    public void SetPlayerData(PlayerSaveData data)
    {
        PlayerData = data;
    }

    public void SetDestination(string sceneName, string spawnPointId)
    {
        CurrentSceneName = sceneName;
        CurrentSpawnPointId = spawnPointId;
    }

    public void RegisterPlayer(Player player)
    {
        if (player == null)
        {
            Debug.LogError("[GameSession] Cannot register a null player.", this);
            return;
        }

        if (CurrentPlayer == player)
            return;

        if (CurrentPlayer != null)
        {
            Debug.LogError(
                "[GameSession] Cannot register a new player while another "
                    + "player is still registered. Unregister the previous player first.",
                this
            );
            return;
        }

        CurrentPlayer = player;
        CurrentSceneName = player.gameObject.scene.name;

        PlayerSpawned?.Invoke(player);
    }

    public void UnregisterPlayer(Player player)
    {
        if (CurrentPlayer != player)
            return;

        // Notify listeners before clearing the reference.
        PlayerDespawning?.Invoke(player);

        CurrentPlayer = null;
    }

    /// <summary>
    /// Completes player spawning for both initial spawns and scene transitions.
    /// </summary>
    public bool CompletePlayerSpawn(Player player)
    {
        if (player == null)
        {
            Debug.LogError("[GameSession] Cannot complete a spawn with a null player.", this);
            return false;
        }

        if (TransitionInProgress && CurrentPlayer != null)
        {
            Debug.LogError(
                "[GameSession] Cannot complete a transition while another "
                    + "player is registered.",
                this
            );
            return false;
        }

        RegisterPlayer(player);

        if (CurrentPlayer != player)
            return false;

        TransitionInProgress = false;
        return true;
    }

    public bool IsEnemyDead(string stableId)
    {
        return !string.IsNullOrWhiteSpace(stableId) && deadEnemyIds.Contains(stableId);
    }

    public void MarkEnemyDead(string stableId)
    {
        if (!string.IsNullOrWhiteSpace(stableId))
            deadEnemyIds.Add(stableId);
    }

    public bool IsEncounterCompleted(string stableId)
    {
        return !string.IsNullOrWhiteSpace(stableId) && completedEncounterIds.Contains(stableId);
    }

    public void MarkEncounterCompleted(string stableId)
    {
        if (!string.IsNullOrWhiteSpace(stableId))
            completedEncounterIds.Add(stableId);
    }

    public void BeginTransition()
    {
        TransitionInProgress = true;
    }

    public void CancelTransition()
    {
        TransitionInProgress = false;
    }
}
