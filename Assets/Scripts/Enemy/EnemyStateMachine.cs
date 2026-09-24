using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyAggro))]
[RequireComponent(typeof(EnemyAttack))]
public class EnemyStateMachine : MonoBehaviour
{
    private EnemyController enemy;
    private EnemyAggro aggro;
    private EnemyAttack attack;

    public EnemyState CurrentState { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        aggro = GetComponent<EnemyAggro>();
        attack = GetComponent<EnemyAttack>();

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

            case EnemyState.Attacking:
                UpdateAttacking();
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
        if (aggro.HasTarget)
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    private void UpdateChasing()
    {
        if (!aggro.HasTarget)
        {
            ChangeState(EnemyState.Returning);
            return;
        }

        Transform target = aggro.Target;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attack.AttackRange && attack.CanAttack)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }

        enemy.Agent.isStopped = false;

        enemy.Agent.SetDestination(target.position);
    }

    private void UpdateAttacking()
    {
        if (!attack.IsAttacking)
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    private void UpdateReturning()
    {
        if (aggro.HasTarget)
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

            case EnemyState.Attacking:
                EnterAttacking();
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

    private void EnterAttacking()
    {
        enemy.Agent.isStopped = true;

        attack.TryAttack(aggro.Target);
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
