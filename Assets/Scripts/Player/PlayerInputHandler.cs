using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField]
    private ActionController actionController;
    [SerializeField]
    private InteractionController interactionController;
    [SerializeField]
    private DashController dashController;

    public void OnActionSlot1(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        actionController.ExecuteSlot(0);
    }

    public void OnActionSlot2(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        actionController.ExecuteSlot(1);
    }

    public void OnActionSlot3(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        actionController.ExecuteSlot(2);
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
