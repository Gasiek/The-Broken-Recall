using UnityEngine;

[RequireComponent(typeof(Player))]
public sealed class InteractionController : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField, Min(0f)]
    private float interactionDistance = 1.5f;

    [SerializeField, Min(0.1f)]
    private float interactionRadius = 1.8f;

    [SerializeField]
    private LayerMask interactionLayer;

    [Header("UI")]
    [SerializeField]
    private InteractionPromptUI interactionPrompt;

    private const float OriginHeight = 1f;
    private const int InitialColliderCapacity = 32;

    private Player player;
    private Collider[] colliderResults = new Collider[InitialColliderCapacity];

    public IInteractable CurrentInteractable { get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        ClearCurrentInteractable();
    }

    private void OnDisable()
    {
        ClearCurrentInteractable();
    }

    /// <summary>
    /// Connects this player to the interaction prompt UI
    /// belonging to the currently loaded scene.
    /// </summary>
    public void SetInteractionPrompt(InteractionPromptUI prompt)
    {
        if (interactionPrompt == prompt)
            return;

        interactionPrompt?.Hide();
        interactionPrompt = prompt;

        if (CurrentInteractable != null && interactionPrompt != null)
            interactionPrompt.Show();
    }

    private void Update()
    {
        if (player == null || !player.isActiveAndEnabled)
        {
            ClearCurrentInteractable();
            return;
        }

        FindInteractable();
    }

    private void FindInteractable()
    {
        Vector3 origin = transform.position + Vector3.up * OriginHeight;
        Vector3 center = origin + transform.forward * interactionDistance;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            interactionRadius,
            colliderResults,
            interactionLayer
        );

        // Expand the buffer if it fills, so potential targets aren't missed.
        if (count == colliderResults.Length)
        {
            colliderResults = Physics.OverlapSphere(center, interactionRadius, interactionLayer);

            count = colliderResults.Length;
        }

        IInteractable bestTarget = null;
        float closestDistanceSquared = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider hit = colliderResults[i];

            if (hit == null)
                continue;

            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable == null)
                continue;

            if (!interactable.CanInteract(player))
                continue;

            Vector3 closestPoint = hit.ClosestPoint(center);
            float distanceSquared = (closestPoint - center).sqrMagnitude;

            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                bestTarget = interactable;
            }
        }

        SetCurrentInteractable(bestTarget);
    }

    public void Interact()
    {
        if (player == null || !player.isActiveAndEnabled)
            return;

        IInteractable target = CurrentInteractable;

        if (target == null)
        {
            ClearCurrentInteractable();
            return;
        }

        // Recheck because the target or player's state may have changed
        // since the most recent detection scan.
        if (!target.CanInteract(player))
        {
            ClearCurrentInteractable();
            return;
        }

        target.Interact(player);

        // Refresh detection next frame in case the interaction changed
        // or consumed the target.
        ClearCurrentInteractable();
    }

    private void SetCurrentInteractable(IInteractable newTarget)
    {
        CurrentInteractable = newTarget;

        if (interactionPrompt == null)
            return;

        if (newTarget != null)
            interactionPrompt.Show();
        else
            interactionPrompt.Hide();
    }

    private void ClearCurrentInteractable()
    {
        CurrentInteractable = null;
        interactionPrompt?.Hide();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * OriginHeight;
        Vector3 center = origin + transform.forward * interactionDistance;

        Gizmos.DrawWireSphere(center, interactionRadius);
        Gizmos.DrawLine(origin, center);
    }
}
