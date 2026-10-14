using System.Collections;
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

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !IsPlayer(other))
            return;

        if (enemyPrefab == null || spawnPoint == null)
        {
            Debug.LogError($"{name}: Assign an enemy prefab and spawn point.", this);

            return;
        }

        hasTriggered = true;

        StartCoroutine(SpawnEnemies(other.transform));
    }

    private IEnumerator SpawnEnemies(Transform player)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (player == null)
                yield break;

            SpawnEnemy(player);

            if (i < enemyCount - 1)
            {
                yield return new WaitForSeconds(Mathf.Max(0f, spawnInterval));
            }
        }
    }

    private void SpawnEnemy(Transform player)
    {
        EnemyController enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyStateMachine stateMachine = enemy.GetComponent<EnemyStateMachine>();

        stateMachine.Initialize(player);
    }

    private bool IsPlayer(Collider other)
    {
        return (playerLayer.value & (1 << other.gameObject.layer)) != 0;
    }
}
