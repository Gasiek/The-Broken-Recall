using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private Health health;
    private NavMeshAgent agent;

    private Vector3 homePosition;

    public NavMeshAgent Agent => agent;
    public Vector3 HomePosition => homePosition;

    public event Action Died;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

        homePosition = transform.position;

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