using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PushReceiver : MonoBehaviour, IPushable
{
    [Header("Push Settings")]
    [SerializeField, Min(0.01f)]
    private float pushDuration = 0.2f;

    private NavMeshAgent agent;
    private CharacterController characterController;
    private PlayerMovement playerMovement;

    private Coroutine pushCoroutine;
    private bool previousAgentStopped;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

        if (agent == null && characterController == null)
        {
            Debug.LogError(
                $"{name}: PushReceiver requires either a NavMeshAgent "
                    + "or a CharacterController.",
                this
            );
        }
    }

    public void Push(Vector3 direction, float distance)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f || distance <= 0f)
            return;

        if (pushCoroutine != null)
            StopActivePush();

        direction.Normalize();

        pushCoroutine = StartCoroutine(PushRoutine(direction, distance));
    }

    private IEnumerator PushRoutine(Vector3 direction, float distance)
    {
        bool useAgent = agent != null && agent.enabled && agent.isOnNavMesh;

        bool useCharacterController =
            !useAgent && characterController != null && characterController.enabled;

        if (!useAgent && !useCharacterController)
        {
            pushCoroutine = null;
            yield break;
        }

        if (useAgent)
        {
            previousAgentStopped = agent.isStopped;
            agent.isStopped = true;
        }
        else if (playerMovement != null)
        {
            playerMovement.SetMovementOverridden(true);
        }

        float duration = Mathf.Max(0.01f, pushDuration);
        float elapsed = 0f;
        float speed = distance / duration;

        while (elapsed < duration)
        {
            float deltaTime = Mathf.Min(Time.deltaTime, duration - elapsed);

            if (useAgent)
            {
                if (agent == null || !agent.enabled || !agent.isOnNavMesh)
                    break;

                agent.Move(direction * speed * deltaTime);
            }
            else
            {
                if (characterController == null || !characterController.enabled)
                    break;

                characterController.Move(direction * speed * deltaTime);
            }

            elapsed += deltaTime;
            yield return null;
        }

        FinishPush(useAgent);
    }

    private void FinishPush(bool useAgent)
    {
        if (useAgent)
        {
            if (agent != null && agent.enabled)
                agent.isStopped = previousAgentStopped;
        }
        else if (playerMovement != null)
        {
            playerMovement.SetMovementOverridden(false);
        }

        pushCoroutine = null;
    }

    private void StopActivePush()
    {
        StopCoroutine(pushCoroutine);
        pushCoroutine = null;

        if (agent != null && agent.enabled)
            agent.isStopped = previousAgentStopped;

        if (playerMovement != null)
            playerMovement.SetMovementOverridden(false);
    }

    private void OnDisable()
    {
        if (pushCoroutine != null)
            StopActivePush();
    }
}
