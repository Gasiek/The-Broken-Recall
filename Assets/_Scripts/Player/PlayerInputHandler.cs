using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ActionController))]
[RequireComponent(typeof(InteractionController))]
[RequireComponent(typeof(DashController))]
public class PlayerInputHandler : MonoBehaviour
{
    private ActionController actionController;
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
            return;

        actionController.ExecuteAction(0);
    }

    public void OnActionSlot2(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        actionController.ExecuteAction(1);
    }

    public void OnActionSlot3(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Debug.Log("Currently slot 3 is disabled.", this);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        interactionController.Interact();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        dashController.Dash();
    }
}
