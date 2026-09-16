using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    private Health health;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

        health.Died += Die;
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        agent.SetDestination(target.position);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Died -= Die;
        }
    }

    private void Die()
    {
        agent.isStopped = true;

        Destroy(gameObject);
    }
}
