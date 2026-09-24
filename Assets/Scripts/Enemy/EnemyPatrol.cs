using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyController))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField]
    private float patrolRadius = 5f;

    [SerializeField, Range(0f, 1f)]
    private float waitChance = 0.3f;

    [SerializeField]
    private float minWaitDuration = 2f;

    [SerializeField]
    private float maxWaitDuration = 4f;

    [Header("Navigation")]
    [SerializeField]
    private int maxPointAttempts = 10;

    [SerializeField]
    private float navMeshSampleDistance = 2f;

    private EnemyController enemy;

    private NavMeshPath path;

    public Vector3 PatrolTarget { get; private set; }

    public bool HasPatrolTarget { get; private set; }

    public bool IsWaiting { get; private set; }

    public float WaitTimer { get; private set; }

    public float PatrolRadius => patrolRadius;

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();

        path = new NavMeshPath();
    }

    public bool TrySetNewPatrolTarget()
    {
        for (int i = 0; i < maxPointAttempts; i++)
        {
            Vector3 randomPoint = GetRandomPointInPatrolArea();

            if (!TryGetValidNavMeshPoint(randomPoint, out Vector3 validPoint))
            {
                continue;
            }

            if (!IsPathValid(validPoint))
            {
                continue;
            }

            PatrolTarget = validPoint;
            HasPatrolTarget = true;
            IsWaiting = false;
            WaitTimer = 0f;

            return true;
        }

        HasPatrolTarget = false;

        return false;
    }

    public bool HasReachedPatrolTarget()
    {
        if (!HasPatrolTarget)
        {
            return false;
        }

        if (enemy.Agent.pathPending)
        {
            return false;
        }

        if (enemy.Agent.hasPath)
        {
            return enemy.Agent.remainingDistance <= enemy.Agent.stoppingDistance + 0.2f;
        }

        return Vector3.Distance(transform.position, PatrolTarget)
            <= enemy.Agent.stoppingDistance + 0.2f;
    }

    public void StartWait()
    {
        HasPatrolTarget = false;

        if (Random.value > waitChance)
        {
            IsWaiting = false;
            WaitTimer = 0f;
            return;
        }

        IsWaiting = true;

        WaitTimer = Random.Range(minWaitDuration, maxWaitDuration);
    }

    public bool UpdateWait()
    {
        if (!IsWaiting)
        {
            return true;
        }

        WaitTimer -= Time.deltaTime;

        if (WaitTimer > 0f)
        {
            return false;
        }

        IsWaiting = false;
        WaitTimer = 0f;

        return true;
    }

    private Vector3 GetRandomPointInPatrolArea()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;

        return enemy.HomePosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
    }

    private bool TryGetValidNavMeshPoint(Vector3 point, out Vector3 validPoint)
    {
        if (
            NavMesh.SamplePosition(
                point,
                out NavMeshHit hit,
                navMeshSampleDistance,
                NavMesh.AllAreas
            )
        )
        {
            validPoint = hit.position;
            return true;
        }

        validPoint = default;
        return false;
    }

    private bool IsPathValid(Vector3 point)
    {
        if (!NavMesh.CalculatePath(transform.position, point, NavMesh.AllAreas, path))
        {
            return false;
        }

        return path.status == NavMeshPathStatus.PathComplete;
    }
}
