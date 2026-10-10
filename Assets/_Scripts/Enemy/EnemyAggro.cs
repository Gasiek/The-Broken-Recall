using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    public Transform Target { get; private set; }

    public bool HasTarget => Target != null;

    private void Start()
    {
        if (Target == null)
        {
            float detectRadius = 12f;
            EnemyAggroTrigger[] triggers = GetComponentsInChildren<EnemyAggroTrigger>();
            foreach (var t in triggers)
            {
                SphereCollider sc = t.GetComponent<SphereCollider>();
                if (sc != null && sc.isTrigger && sc.radius > 0f)
                {
                    if (sc.radius < detectRadius)
                    {
                        detectRadius = sc.radius;
                    }
                }
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, targetLayer);
            if (hits.Length > 0)
            {
                Target = hits[0].transform;
            }
        }
    }

    public void SetTarget(Transform target)
    {
        Target = target;
    }

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
