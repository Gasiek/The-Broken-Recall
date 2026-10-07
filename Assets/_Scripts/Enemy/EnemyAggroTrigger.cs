using UnityEngine;

public class EnemyAggroTrigger : MonoBehaviour
{
    [SerializeField]
    private EnemyAggro aggro;

    [SerializeField]
    private bool isLoseTargetTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoseTargetTrigger)
        {
            return;
        }

        aggro.HandleAggroEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLoseTargetTrigger)
        {
            return;
        }

        aggro.HandleLoseTargetExit(other);
    }
}
