using System;
using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    public Transform Target { get; private set; }
    public bool HasTarget => Target != null;

    public event Action TargetLost;

    private Health targetHealth;

    public void SetTarget(Transform target)
    {
        if (Target == target)
            return;

        UnsubscribeFromTargetDeath();

        Target = target;

        if (Target == null)
            return;

        targetHealth = Target.GetComponentInParent<Health>();

        if (targetHealth != null)
            targetHealth.Died += HandleTargetDied;
    }

    public void ClearTarget()
    {
        if (Target == null)
            return;

        UnsubscribeFromTargetDeath();
        Target = null;

        TargetLost?.Invoke();
    }

    public void HandleAggroEnter(Collider other)
    {
        if (other == null || !IsTarget(other))
            return;

        if (Target == null)
            SetTarget(other.transform);
    }

    public void HandleLoseTargetExit(Collider other)
    {
        if (other == null || Target == null)
            return;

        if (other.transform == Target)
            ClearTarget();
    }

    private void HandleTargetDied()
    {
        ClearTarget();
    }

    private void UnsubscribeFromTargetDeath()
    {
        if (targetHealth != null)
            targetHealth.Died -= HandleTargetDied;

        targetHealth = null;
    }

    private bool IsTarget(Collider other)
    {
        return (targetLayer.value & (1 << other.gameObject.layer)) != 0;
    }

    private void OnDestroy()
    {
        UnsubscribeFromTargetDeath();
    }
}
