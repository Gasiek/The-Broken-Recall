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
        if (hasTriggered)
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        hasTriggered = true;

        StartCoroutine(
            SpawnEnemies(other.transform)
        );
    }

    private IEnumerator SpawnEnemies(Transform player)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(player);

            if (i < enemyCount - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    private void SpawnEnemy(Transform player)
    {
        EnemyController enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        EnemyAggro aggro = enemy.GetComponent<EnemyAggro>();

        if (aggro != null)
        {
            aggro.Initialize(player);
        }
    }

    private bool IsPlayer(Collider other)
    {
        return (playerLayer.value & (1 << other.gameObject.layer)) != 0;
    }
}