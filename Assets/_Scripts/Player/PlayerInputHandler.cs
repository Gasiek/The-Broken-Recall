using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ActionController))]
[RequireComponent(typeof(InteractionController))]
[RequireComponent(typeof(DashController))]
public class PlayerInputHandler : MonoBehaviour
{
    private ActionController actionController;

    [SerializeField]
    private InteractionController interactionController;
    private DashController dashController;

    private void Awake()
    {
        actionController = GetComponent<ActionController>();
        interactionController = GetComponent<InteractionController>();
        dashController = GetComponent<DashController>();
    }

    public void OnActionSlot1(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        actionController.ExecuteAction(0);
    }

    public void OnActionSlot2(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        actionController.ExecuteAction(1);
    }

    public void OnActionSlot3(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        actionController.ExecuteAction(2);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        interactionController.Interact();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        dashController.Dash();
    }
}
