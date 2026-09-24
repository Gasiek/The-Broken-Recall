using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    public Transform Target { get; private set; }

    public bool HasTarget => Target != null;

    public void HandleAggroEnter(Collider other)
    {
        if (!IsTarget(other))
        {
            return;
        }

        if (Target == null)
        {
            Target = other.transform;
        }
    }

    public void HandleLoseTargetExit(Collider other)
    {
        if (Target == null)
        {
            return;
        }

        if (other.transform == Target)
        {
            Target = null;
        }
    }

    private bool IsTarget(Collider other)
    {
        return (targetLayer.value & (1 << other.gameObject.layer)) != 0;
    }
}
