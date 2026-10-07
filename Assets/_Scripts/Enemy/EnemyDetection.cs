using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float aggroRadius = 6f;

    [SerializeField]
    private float loseTargetRadius = 9f;

    public bool IsTargetInAggroRange
    {
        get
        {
            if (target == null)
            {
                return false;
            }

            return GetDistanceToTarget() <= aggroRadius;
        }
    }

    public bool IsTargetOutsideLoseRange
    {
        get
        {
            if (target == null)
            {
                return true;
            }

            return GetDistanceToTarget() >= loseTargetRadius;
        }
    }

    public Transform Target => target;

    private float GetDistanceToTarget()
    {
        return Vector3.Distance(
            transform.position,
            target.position
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            aggroRadius
        );

        Gizmos.DrawWireSphere(
            transform.position,
            loseTargetRadius
        );
    }
}