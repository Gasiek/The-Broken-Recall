using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyAggro))]
[RequireComponent(typeof(EnemyPatrol))]
public class EnemyStateMachine : MonoBehaviour
{
    private EnemyController enemy;
    private EnemyAggro aggro;
    private EnemyAttack meleeAttack;
    private RangedEnemyAttack rangedAttack;
    private EnemyPatrol patrol;
    private EnemyAnimation enemyAnimation;

    private float destroyDelay = 10f;

    public EnemyState CurrentState { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        aggro = GetComponent<EnemyAggro>();
        meleeAttack = GetComponent<EnemyAttack>();
        rangedAttack = GetComponent<RangedEnemyAttack>();
        patrol = GetComponent<EnemyPatrol>();
        enemyAnimation = GetComponent<EnemyAnimation>();

        CurrentState = EnemyState.Idle;
    }

    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.Died += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.Died -= HandleDeath;
        }
    }

    private void Start()
    {
        if (CurrentState == EnemyState.Idle && !aggro.HasTarget)
        {
            ChangeState(EnemyState.Patrolling);
        }
    }

    public void Initialize(Transform target)
    {
        if (target == null || CurrentState == EnemyState.Dead)
            return;

        aggro.SetTarget(target);
        ChangeState(EnemyState.Chasing);
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

            case EnemyState.Patrolling:
                UpdatePatrolling();
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

        if (rangedAttack != null && distanceToTarget <= rangedAttack.AttackRange)
        {
            enemy.Agent.isStopped = true;
            enemyAnimation?.SetLocomotion(0f);
            FaceTarget(target);

            if (rangedAttack.CanAttack)
            {
                ChangeState(EnemyState.Attacking);
            }

            return;
        }

        if (meleeAttack != null && distanceToTarget <= meleeAttack.AttackRange)
        {
            enemy.Agent.isStopped = true;
            enemyAnimation?.SetLocomotion(0f);
            FaceTarget(target);

            if (meleeAttack.CanAttack)
            {
                ChangeState(EnemyState.Attacking);
            }

            return;
        }

        enemy.Agent.speed = enemy.Definition.MoveSpeed;
        enemy.Agent.isStopped = false;
        enemy.Agent.SetDestination(target.position);

        // Chasing uses Run.
        enemyAnimation?.SetLocomotion(1f);
    }

    private void UpdateAttacking()
    {
        if (rangedAttack != null)
        {
            if (!rangedAttack.IsAttacking)
            {
                ChangeState(aggro.HasTarget ? EnemyState.Chasing : EnemyState.Returning);
            }

            return;
        }

        if (meleeAttack != null)
        {
            if (!meleeAttack.IsAttacking)
            {
                ChangeState(aggro.HasTarget ? EnemyState.Chasing : EnemyState.Returning);
            }

            return;
        }

        // No attack component was configured.
        ChangeState(EnemyState.Chasing);
    }

    private void UpdateReturning()
    {
        if (aggro.HasTarget)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        if (HasReachedHome())
        {
            ChangeState(EnemyState.Patrolling);
            return;
        }

        enemy.Agent.speed = enemy.Definition.MoveSpeed;
        enemy.Agent.isStopped = false;
        enemy.Agent.SetDestination(enemy.HomePosition);

        // Returning uses Walk.
        enemyAnimation?.SetLocomotion(0.5f);
    }

    private void UpdatePatrolling()
    {
        if (aggro.HasTarget)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        if (patrol.IsWaiting)
        {
            enemy.Agent.isStopped = true;
            enemyAnimation?.SetLocomotion(0f);

            if (patrol.UpdateWait())
            {
                SetNewPatrolTarget();
            }

            return;
        }

        if (!patrol.HasPatrolTarget)
        {
            SetNewPatrolTarget();
            return;
        }

        if (patrol.HasReachedPatrolTarget())
        {
            enemy.Agent.isStopped = true;

            enemyAnimation?.SetLocomotion(0f);

            patrol.StartWait();

            if (!patrol.IsWaiting)
            {
                SetNewPatrolTarget();
            }

            return;
        }

        // Keep walking while moving to a patrol destination.
        enemyAnimation?.SetLocomotion(0.5f);
    }

    private void SetNewPatrolTarget()
    {
        if (!patrol.TrySetNewPatrolTarget())
        {
            enemy.Agent.isStopped = true;
            enemyAnimation?.SetLocomotion(0f);
            return;
        }

        enemy.Agent.speed = enemy.Definition.PatrolSpeed;
        enemy.Agent.isStopped = false;
        enemy.Agent.SetDestination(patrol.PatrolTarget);

        enemyAnimation?.SetLocomotion(0.5f);
    }

    private bool HasReachedHome()
    {
        if (enemy.Agent.pathPending)
            return false;

        if (enemy.Agent.hasPath)
        {
            return enemy.Agent.remainingDistance <= enemy.Agent.stoppingDistance + 0.2f;
        }

        return Vector3.Distance(transform.position, enemy.HomePosition)
            <= enemy.Agent.stoppingDistance + 0.2f;
    }

    private void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState)
            return;

        if (CurrentState == EnemyState.Dead)
            return;

        CurrentState = newState;

        switch (newState)
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

            case EnemyState.Patrolling:
                EnterPatrolling();
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

        enemyAnimation?.SetLocomotion(0f);
    }

    private void EnterChasing()
    {
        enemy.Agent.speed = enemy.Definition.MoveSpeed;
        enemy.Agent.isStopped = false;

        enemyAnimation?.SetLocomotion(1f);
    }

    private void EnterAttacking()
    {
        enemy.Agent.isStopped = true;
        enemyAnimation?.SetLocomotion(0f);

        Transform target = aggro.Target;

        if (target == null)
        {
            ChangeState(EnemyState.Returning);
            return;
        }

        FaceTarget(target);

        if (rangedAttack != null)
        {
            rangedAttack.TryAttack(target);
            return;
        }

        if (meleeAttack != null)
        {
            meleeAttack.TryAttack(target);
        }
    }

    private void EnterReturning()
    {
        enemy.Agent.speed = enemy.Definition.MoveSpeed;
        enemy.Agent.isStopped = false;

        enemyAnimation?.SetLocomotion(0.5f);
    }

    private void EnterPatrolling()
    {
        enemy.Agent.speed = enemy.Definition.PatrolSpeed;
        enemy.Agent.isStopped = false;

        enemyAnimation?.SetLocomotion(0.5f);

        SetNewPatrolTarget();
    }

    private void EnterDead()
    {
        enemy.Agent.isStopped = true;
        enemy.Agent.ResetPath();

        // Prevent any further gameplay movement.
        if (meleeAttack != null)
            meleeAttack.CancelAttack();

        enemyAnimation?.PlayDie();
    }

    private void HandleDeath()
    {
        ChangeState(EnemyState.Dead);
    }

    // Called by EnemyAnimationEvents at the end of the Die clip.
    public void FinishDeath()
    {
        if (CurrentState != EnemyState.Dead)
            return;

        Destroy(gameObject, destroyDelay);
    }

    private void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}
