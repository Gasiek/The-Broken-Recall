using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField]
    private EnemyController enemyPrefab;

    [SerializeField]
    private Transform spawnPoint;

    [Header("Spawning")]
    [SerializeField]
    private int enemyCount = 3;

    [SerializeField]
    private float spawnInterval = 1f;

    [Header("Detection")]
    [SerializeField]
    private LayerMask playerLayer;

    private bool hasTriggered;
    private bool isSpawning;
    private bool spawningFinished;
    private bool encounterCompleted;

    private PersistentEncounter persistence;

    private readonly HashSet<Health> livingEnemies = new HashSet<Health>();
    private readonly Dictionary<Health, Action> deathHandlers = new Dictionary<Health, Action>();

    private void Awake()
    {
        persistence = GetComponent<PersistentEncounter>();

        encounterCompleted = persistence != null && persistence.IsCompleted;
        hasTriggered = encounterCompleted;

        if (enemyCount < 0)
        {
            Debug.LogError($"[{nameof(EnemySpawner)}] Enemy count cannot be negative.", this);

            enemyCount = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (
            hasTriggered
            || encounterCompleted
            || (persistence != null && persistence.IsCompleted)
            || !IsPlayer(other)
        )
        {
            return;
        }

        if (enemyPrefab == null || spawnPoint == null)
        {
            Debug.LogError($"{name}: Assign an enemy prefab and spawn point.", this);

            return;
        }

        hasTriggered = true;
        isSpawning = true;
        spawningFinished = false;

        StartCoroutine(SpawnEnemies(other.transform));
    }

    private IEnumerator SpawnEnemies(Transform player)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (player == null)
            {
                isSpawning = false;
                yield break;
            }

            SpawnEnemy(player);

            if (i < enemyCount - 1)
                yield return new WaitForSeconds(Mathf.Max(0f, spawnInterval));
        }

        isSpawning = false;
        spawningFinished = true;

        TryCompleteEncounter();
    }

    private void SpawnEnemy(Transform player)
    {
        EnemyController enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyStateMachine stateMachine = enemy.GetComponent<EnemyStateMachine>();

        Health health = enemy.GetComponent<Health>();

        if (stateMachine == null || health == null)
        {
            Debug.LogError(
                $"{enemy.name}: Enemy requires both EnemyStateMachine " + "and Health components.",
                enemy
            );

            Destroy(enemy.gameObject);
            return;
        }

        livingEnemies.Add(health);

        Action deathHandler = () => OnEnemyDied(health);
        deathHandlers[health] = deathHandler;
        health.Died += deathHandler;

        stateMachine.Initialize(player);
    }

    private void OnEnemyDied(Health health)
    {
        if (health == null || !livingEnemies.Contains(health))
            return;

        RemoveTrackedEnemy(health);
        TryCompleteEncounter();
    }

    private void Update()
    {
        if (encounterCompleted || !hasTriggered)
            return;

        // Handle enemies destroyed without firing Health.Died.
        List<Health> destroyedEnemies = null;

        foreach (Health health in livingEnemies)
        {
            if (health == null)
            {
                destroyedEnemies ??= new List<Health>();
                destroyedEnemies.Add(health);
            }
        }

        if (destroyedEnemies == null)
            return;

        foreach (Health health in destroyedEnemies)
            RemoveTrackedEnemy(health);

        TryCompleteEncounter();
    }

    private void RemoveTrackedEnemy(Health health)
    {
        livingEnemies.Remove(health);

        if (deathHandlers.TryGetValue(health, out Action handler))
        {
            if (health != null)
                health.Died -= handler;

            deathHandlers.Remove(health);
        }
    }

    private void TryCompleteEncounter()
    {
        if (
            encounterCompleted
            || !hasTriggered
            || isSpawning
            || !spawningFinished
            || livingEnemies.Count > 0
        )
        {
            return;
        }

        encounterCompleted = true;

        if (persistence != null)
            persistence.MarkCompleted();

        Debug.Log($"{name}: Encounter completed.", this);
    }

    private bool IsPlayer(Collider other)
    {
        return other != null
            && (playerLayer.value & (1 << other.gameObject.layer)) != 0
            && other.GetComponent<Player>() != null;
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<Health, Action> pair in deathHandlers)
        {
            if (pair.Key != null)
                pair.Key.Died -= pair.Value;
        }

        deathHandlers.Clear();
        livingEnemies.Clear();
    }
}
