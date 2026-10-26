using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
    Returning,
    Patrolling,
    Stunned,
    Dead,
}

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyAggro))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(CharacterEffectManager))]
[RequireComponent(typeof(EnemyAnimation))]
public class EnemyStateMachine : MonoBehaviour
{
    private EnemyController enemy;
    private EnemyAggro aggro;
    private EnemyPatrol patrol;
    private CharacterEffectManager characterEffects;
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

    [SerializeField]
    private float rotationSpeed = 360f;

    private float nextGeneralAttackTime;

    public EnemyState CurrentState { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        aggro = GetComponent<EnemyAggro>();
        patrol = GetComponent<EnemyPatrol>();
        characterEffects = GetComponent<CharacterEffectManager>();
        enemyAnimation = GetComponent<EnemyAnimation>();

        InitializeAttacks();

        CurrentState = EnemyState.Idle;
    }

    private void OnEnable()
    {
        if (enemy != null)
            enemy.Died += HandleDeath;

        if (characterEffects != null)
            characterEffects.StunStateChanged += HandleStunStateChanged;
    }

    private void OnDisable()
    {
        if (enemy != null)
            enemy.Died -= HandleDeath;

        if (characterEffects != null)
            characterEffects.StunStateChanged -= HandleStunStateChanged;
    }

    private void Start()
    {
        if (characterEffects != null && characterEffects.IsStunned)
        {
            ChangeState(EnemyState.Stunned);
            return;
        }

        if (CurrentState == EnemyState.Idle && !aggro.HasTarget)
            ChangeState(EnemyState.Patrolling);
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

            if (attack == null)
                continue;

            attacks.Add(attack);
        }
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

        if (!activeAttack.IsAttacking)
            return;

        activeAttack.AnimationAttackFinished();

        // Start the general cooldown after the attack animation finishes.
        nextGeneralAttackTime = Time.time + enemy.Definition.GeneralAttackCooldown;
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

            case EnemyState.Stunned:
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

        // Check whether any attack is in range, even if it is on cooldown.
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

            if (!IsFacingTarget(target))
            {
                FaceTargetSmoothly(target);
                return;
            }

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
        // Prevent starting any attack during the general cooldown.
        if (Time.time < nextGeneralAttackTime)
            return null;

        IEnemyAttack bestNormalAttack = null;
        IEnemyAttack availableSuperAttack = null;

        foreach (IEnemyAttack attack in attacks)
        {
            if (attack == null || !attack.CanAttack)
                continue;

            if (!IsAttackInRange(attack, distanceToTarget))
                continue;

            if (attack.IsSuperAttack)
            {
                if (availableSuperAttack == null)
                    availableSuperAttack = attack;
            }
            else
            {
                if (bestNormalAttack == null || attack.AttackRange < bestNormalAttack.AttackRange)
                {
                    bestNormalAttack = attack;
                }
            }
        }

        // No eligible super attack: use a normal attack if possible.
        if (availableSuperAttack == null)
        {
            if (bestNormalAttack == null)
                Debug.Log($"{name}: No attack available at distance {distanceToTarget:F2}.", this);

            return bestNormalAttack;
        }

        // A super attack is the only available option.
        if (bestNormalAttack == null)
        {
            Debug.Log(
                $"{name}: Selecting super attack '{availableSuperAttack.Definition.name}'.",
                this
            );
            return availableSuperAttack;
        }

        // Both are available: roll the configured chance.
        float chance = enemy.Definition.SuperAttackChance;

        if (Random.Range(0f, 100f) < chance)
        {
            Debug.Log($"{name}: Super attack selected (chance {chance}%).", this);
            return availableSuperAttack;
        }

        Debug.Log($"{name}: Normal attack selected (super chance {chance}%).", this);
        return bestNormalAttack;
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

            case EnemyState.Stunned:
                EnterStunned();
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

    private void HandleStunStateChanged(bool isStunned)
    {
        if (CurrentState == EnemyState.Dead)
            return;

        if (isStunned)
        {
            ChangeState(EnemyState.Stunned);
        }
        else if (CurrentState == EnemyState.Stunned)
        {
            ResumeAfterStun();
        }
    }

    private void EnterStunned()
    {
        // Cancel the current attack so it cannot deal damage after being stunned.
        if (activeAttack != null)
        {
            activeAttack.CancelAttack();
            activeAttack = null;
        }

        if (enemy.Agent != null)
        {
            enemy.Agent.isStopped = true;
            enemy.Agent.ResetPath();
        }

        enemyAnimation.SetLocomotion(0f);
        enemyAnimation.SetStunned(true);
    }

    private void ResumeAfterStun()
    {
        enemyAnimation.SetStunned(false);

        if (aggro.HasTarget)
        {
            ChangeState(EnemyState.Chasing);
        }
        else if (HasReachedHome())
        {
            ChangeState(EnemyState.Patrolling);
        }
        else
        {
            ChangeState(EnemyState.Returning);
        }
    }

    private void HandleDeath()
    {
        ChangeState(EnemyState.Dead);
    }

    // Called by EnemyAnimationEvents at the end of the death animation.
    public void FinishDeath()
    {
        if (CurrentState != EnemyState.Dead)
            return;

        Destroy(gameObject, destroyDelay);
    }

    private bool IsAttackInRange(IEnemyAttack attack, float distanceToTarget)
    {
        return distanceToTarget >= attack.MinimumRange && distanceToTarget <= attack.AttackRange;
    }

    private bool IsFacingTarget(Transform target, float angleTolerance = 8f)
    {
        if (target == null)
            return false;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return true;

        float angle = Vector3.Angle(transform.forward, direction.normalized);

        return angle <= angleTolerance;
    }

    private void FaceTargetSmoothly(Transform target)
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
