using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PushReceiver : MonoBehaviour, IPushable
{
    [SerializeField] private float pushDuration = 0.2f;

    private NavMeshAgent agent;
    private Coroutine pushCoroutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Push(Vector3 direction, float distance)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();

        if (pushCoroutine != null)
        {
            StopCoroutine(pushCoroutine);
        }

        pushCoroutine = StartCoroutine(
            PushRoutine(direction, distance)
        );
    }

    private IEnumerator PushRoutine(
        Vector3 direction,
        float distance)
    {
        agent.isStopped = true;

        float elapsed = 0f;
        float speed = distance / pushDuration;

        while (elapsed < pushDuration)
        {
            agent.Move(
                direction * speed * Time.deltaTime
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        agent.isStopped = false;
        pushCoroutine = null;
    }
}