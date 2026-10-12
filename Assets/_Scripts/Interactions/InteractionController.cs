using UnityEngine;

[RequireComponent(typeof(Player))]
public class InteractionController : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField]
    private float interactionDistance = 1.5f;

    [SerializeField]
    private float interactionRadius = 1.8f;

    [SerializeField]
    private LayerMask interactionLayer;

    [Header("UI")]
    [SerializeField]
    private InteractionPromptUI interactionPrompt;

    private Player player;

    public IInteractable CurrentInteractable { get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();

        if (interactionPrompt == null)
        {
            Debug.LogWarning("[InteractionController] No InteractionPromptUI assigned.", this);
        }
    }

    private void Update()
    {
        FindInteractable();
    }

    private void FindInteractable()
    {
        Vector3 origin = transform.position + Vector3.up * 1.0f;

        Vector3 interactionCenter = origin + transform.forward * interactionDistance;

        Collider[] colliders = Physics.OverlapSphere(
            interactionCenter,
            interactionRadius,
            interactionLayer
        );

        IInteractable newInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in colliders)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null || !interactable.CanInteract(player))
                continue;

            float distance = Vector3.Distance(
                interactionCenter,
                hit.ClosestPoint(interactionCenter)
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                newInteractable = interactable;
            }
        }

        if (newInteractable == CurrentInteractable)
            return;

        CurrentInteractable = newInteractable;

        if (interactionPrompt == null)
            return;

        if (CurrentInteractable != null)
            interactionPrompt.Show();
        else
            interactionPrompt.Hide();
    }

    public void Interact()
    {
        if (player == null || CurrentInteractable == null)
            return;

        // The player's state might have changed since target detection.
        if (!CurrentInteractable.CanInteract(player))
        {
            CurrentInteractable = null;

            if (interactionPrompt != null)
                interactionPrompt.Hide();

            return;
        }

        CurrentInteractable.Interact(player);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * 1.0f;

        Vector3 interactionCenter = origin + transform.forward * interactionDistance;

        Gizmos.DrawWireSphere(interactionCenter, interactionRadius);
        Gizmos.DrawLine(origin, interactionCenter);
    }
}
