using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyDetection))]
public class EnemyStateMachine : MonoBehaviour
{
    private EnemyController enemy;
    private EnemyDetection detection;

    public EnemyState CurrentState { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        detection = GetComponent<EnemyDetection>();

        CurrentState = EnemyState.Idle;
    }

    private void OnEnable()
    {
        enemy.Died += HandleDeath;
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.Died -= HandleDeath;
        }
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;

            case EnemyState.Chasing:
                UpdateChasing();
                break;

            case EnemyState.Returning:
                UpdateReturning();
                break;

            case EnemyState.Dead:
                break;
        }
    }

    private void UpdateIdle()
    {
        if (detection.IsTargetInAggroRange)
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    private void UpdateChasing()
    {
        if (detection.Target == null)
        {
            ChangeState(EnemyState.Returning);
            return;
        }

        if (detection.IsTargetOutsideLoseRange)
        {
            ChangeState(EnemyState.Returning);
            return;
        }

        enemy.Agent.isStopped = false;

        enemy.Agent.SetDestination(detection.Target.position);
    }

    private void UpdateReturning()
    {
        if (detection.IsTargetInAggroRange)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        enemy.Agent.isStopped = false;

        enemy.Agent.SetDestination(enemy.HomePosition);

        if (HasReachedHome())
        {
            ChangeState(EnemyState.Idle);
        }
    }

    private bool HasReachedHome()
    {
        if (enemy.Agent.pathPending)
        {
            return false;
        }

        if (!enemy.Agent.hasPath)
        {
            return true;
        }

        return enemy.Agent.remainingDistance <= enemy.Agent.stoppingDistance + 0.2f;
    }

    private void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }

        CurrentState = newState;

        Debug.Log($"{gameObject.name} → {CurrentState}");

        switch (CurrentState)
        {
            case EnemyState.Idle:
                EnterIdle();
                break;

            case EnemyState.Chasing:
                EnterChasing();
                break;

            case EnemyState.Returning:
                EnterReturning();
                break;

            case EnemyState.Dead:
                EnterDead();
                break;
        }
    }

    private void EnterIdle()
    {
        enemy.Agent.isStopped = true;
        enemy.Agent.ResetPath();
    }

    private void EnterChasing()
    {
        enemy.Agent.isStopped = false;
    }

    private void EnterReturning()
    {
        enemy.Agent.isStopped = false;
    }

    private void EnterDead()
    {
        enemy.Agent.isStopped = true;
        enemy.Agent.ResetPath();
    }

    private void HandleDeath()
    {
        ChangeState(EnemyState.Dead);

        Destroy(gameObject);
    }
}
