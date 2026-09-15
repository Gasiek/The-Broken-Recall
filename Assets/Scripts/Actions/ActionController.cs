using System.Collections;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ActionMemory actionMemory;

    public void ExecuteAction(int slot)
    {
        if (!playerController.CanAct)
        {
            return;
        }

        ActionDefinition action = actionMemory.GetAction(slot);

        if (action == null)
        {
            Debug.LogWarning($"No action assigned to slot {slot}.");
            return;
        }

        StartCoroutine(ExecuteActionRoutine(slot, action));
    }

    private IEnumerator ExecuteActionRoutine(
        int slot,
        ActionDefinition action)
    {
        playerController.StartActing();

        Debug.Log($"Started action: {action.DisplayName}");

        actionMemory.ReplaceAction(slot);

        // Action is active.
        yield return new WaitForSeconds(action.Duration);

        Debug.Log($"Finished action: {action.DisplayName}");

        playerController.FinishActing();
    }
}