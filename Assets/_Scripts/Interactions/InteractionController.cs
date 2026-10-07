using UnityEngine;

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

    public IInteractable CurrentInteractable { get; private set; }

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

        foreach (Collider collider in colliders)
        {
            IInteractable interactable = collider.GetComponentInParent<IInteractable>();

            if (interactable != null && interactable.CanInteract)
            {
                newInteractable = interactable;
                break;
            }
        }

        if (newInteractable == CurrentInteractable)
        {
            return;
        }

        CurrentInteractable = newInteractable;

        if (CurrentInteractable != null)
        {
            interactionPrompt.Show();
        }
        else
        {
            interactionPrompt.Hide();
        }
    }

    public void Interact()
    {
        if (CurrentInteractable == null)
        {
            return;
        }

        CurrentInteractable.Interact();
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
