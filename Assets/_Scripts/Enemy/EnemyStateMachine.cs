using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
    Returning,
    Patrolling,
    Dead
}

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyAggro))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyAnimation))]
public class EnemyStateMachine : MonoBehaviour
{
    private EnemyController enemy;
    private EnemyAggro aggro;
    private EnemyPatrol patrol;
    private EnemyAnimation enemyAnimation;

    private readonly List<IEnemyAttack> attacks = new();
    private IEnemyAttack activeAttack;

    [SerializeField]
    private float destroyDelay = 10f;

    [Header("Attack Points")]
    [SerializeField]
    private Transform attackOrigin;

    [SerializeField]
    private Transform projectileSpawnPoint;
    public EnemyState CurrentState { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        aggro = GetComponent<EnemyAggro>();
        patrol = GetComponent<EnemyPatrol>();
        enemyAnimation = GetComponent<EnemyAnimation>();

        InitializeAttacks();

        CurrentState = EnemyState.Idle;
    }

    private void InitializeAttacks()
    {
        attacks.Clear();

        if (enemy.Definition == null)
        {
            Debug.LogError($"{name}: EnemyController has no EnemyDefinition.", this);
            return;
        }

        AttackDefinition[] definitions = enemy.Definition.Attacks;

        if (definitions == null || definitions.Length == 0)
        {
            Debug.LogWarning($"{name}: EnemyDefinition has no attacks assigned.", this);
            return;
        }

        foreach (AttackDefinition definition in definitions)
        {
            if (definition == null)
                continue;

            IEnemyAttack attack = EnemyAttackFactory.Create(
                definition,
                transform,
                attackOrigin,
                projectileSpawnPoint,
                enemyAnimation
            );

            if (attack != null)
                attacks.Add(attack);
        }
    }

    private void OnEnable()
    {
        if (enemy != null)
            enemy.Died += HandleDeath;
    }

    private void OnDisable()
    {
        if (enemy != null)
            enemy.Died -= HandleDeath;
    }

    private void Start()
    {
        if (CurrentState == EnemyState.Idle && !aggro.HasTarget)
            ChangeState(EnemyState.Patrolling);
    }

    public void Initialize(Transform target)
    {
        if (target == null || CurrentState == EnemyState.Dead)
            return;

        aggro.SetTarget(target);
        ChangeState(EnemyState.Chasing);
    }

    public void AnimationAttackImpact()
    {
        if (CurrentState != EnemyState.Attacking || activeAttack == null)
            return;

        activeAttack.AnimationAttackImpact();
    }

    public void AnimationAttackFinished()
    {
        if (CurrentState != EnemyState.Attacking || activeAttack == null)
            return;

        activeAttack.AnimationAttackFinished();
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
            ChangeState(EnemyState.Chasing);
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

        // First check whether any attack is in range,
        // regardless of whether its cooldown has finished.
        bool anyAttackInRange = false;

        foreach (IEnemyAttack attack in attacks)
        {
            if (attack == null)
                continue;

            if (IsAttackInRange(attack, distanceToTarget))
            {
                anyAttackInRange = true;
                break;
            }
        }

        if (anyAttackInRange)
        {
            enemy.Agent.isStopped = true;
            enemyAnimation.SetLocomotion(0f);
            FaceTarget(target);

            // An attack is in range. Wait if all attacks are on cooldown.
            IEnemyAttack attackToUse = SelectAttack(distanceToTarget);

            if (attackToUse == null)
                return;

            activeAttack = attackToUse;

            if (activeAttack.TryStartAttack(target))
            {
                ChangeState(EnemyState.Attacking);
            }
            else
            {
                activeAttack = null;
            }

            return;
        }

        // No attack is in range, so continue chasing.
        enemy.Agent.speed = enemy.Definition.MoveSpeed;
        enemy.Agent.isStopped = false;
        enemy.Agent.SetDestination(target.position);

        enemyAnimation.SetLocomotion(1f);
    }

    private IEnemyAttack SelectAttack(float distanceToTarget)
    {
        IEnemyAttack bestAttack = null;

        foreach (IEnemyAttack attack in attacks)
        {
            if (attack == null || !attack.CanAttack)
                continue;

            if (!IsAttackInRange(attack, distanceToTarget))
                continue;

            // Prefer the attack with the shorter maximum range.
            // This normally favors melee when both attacks are eligible.
            if (bestAttack == null || attack.AttackRange < bestAttack.AttackRange)
            {
                bestAttack = attack;
            }
        }

        return bestAttack;
    }

    private void UpdateAttacking()
    {
        if (activeAttack == null || !activeAttack.IsAttacking)
        {
            activeAttack = null;
            ChangeState(aggro.HasTarget ? EnemyState.Chasing : EnemyState.Returning);
        }
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

        enemyAnimation.SetLocomotion(0.5f);
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
            enemyAnimation.SetLocomotion(0f);

            if (patrol.UpdateWait())
                SetNewPatrolTarget();

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
            enemyAnimation.SetLocomotion(0f);

            patrol.StartWait();

            if (!patrol.IsWaiting)
                SetNewPatrolTarget();

            return;
        }

        enemyAnimation.SetLocomotion(0.5f);
    }

    private void SetNewPatrolTarget()
    {
        if (!patrol.TrySetNewPatrolTarget())
        {
            enemy.Agent.isStopped = true;
            enemyAnimation.SetLocomotion(0f);
            return;
        }

        enemy.Agent.speed = enemy.Definition.PatrolSpeed;
        enemy.Agent.isStopped = false;
        enemy.Agent.SetDestination(patrol.PatrolTarget);

        enemyAnimation.SetLocomotion(0.5f);
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
        if (CurrentState == newState || CurrentState == EnemyState.Dead)
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
        enemyAnimation.SetLocomotion(0f);
    }

    private void EnterChasing()
    {
        enemy.Agent.speed = enemy.Definition.MoveSpeed;
        enemy.Agent.isStopped = false;
        enemyAnimation.SetLocomotion(1f);
    }

    private void EnterAttacking()
    {
        enemy.Agent.isStopped = true;
        enemyAnimation.SetLocomotion(0f);

        if (!aggro.HasTarget || activeAttack == null)
        {
            activeAttack = null;
            ChangeState(aggro.HasTarget ? EnemyState.Chasing : EnemyState.Returning);
        }
    }

    private void EnterReturning()
    {
        enemy.Agent.speed = enemy.Definition.MoveSpeed;
        enemy.Agent.isStopped = false;
        enemyAnimation.SetLocomotion(0.5f);
    }

    private void EnterPatrolling()
    {
        enemy.Agent.speed = enemy.Definition.PatrolSpeed;
        enemy.Agent.isStopped = false;
        enemyAnimation.SetLocomotion(0.5f);

        SetNewPatrolTarget();
    }

    private void EnterDead()
    {
        enemy.Agent.isStopped = true;
        enemy.Agent.ResetPath();

        foreach (IEnemyAttack attack in attacks)
        {
            if (attack != null)
                attack.CancelAttack();
        }

        activeAttack = null;
        enemyAnimation.PlayDie();
    }

    private void HandleDeath()
    {
        ChangeState(EnemyState.Dead);
    }

    // Called by EnemyAnimationEvents at the end of the death clip.
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

    private bool IsAttackInRange(IEnemyAttack attack, float distanceToTarget)
    {
        return distanceToTarget >= attack.MinimumRange && distanceToTarget <= attack.AttackRange;
    }
}
