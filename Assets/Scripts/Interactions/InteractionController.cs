using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField]
    private float interactionDistance = 2f;

    [SerializeField]
    private LayerMask interactionLayer;

    public void Interact()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (
            Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                interactionDistance,
                interactionLayer
            )
        )
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}
