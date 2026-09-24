using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private EnemyDefinition definition;

    private Health health;
    private NavMeshAgent agent;

    private Vector3 homePosition;

    public EnemyDefinition Definition => definition;
    public NavMeshAgent Agent => agent;
    public Vector3 HomePosition => homePosition;

    public event Action Died;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

        homePosition = transform.position;

        health.Initialize(definition.MaxHealth);

        agent.speed = definition.MoveSpeed;

        health.Died += HandleDeath;
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Died -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        Died?.Invoke();
    }
}
