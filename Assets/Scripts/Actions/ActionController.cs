using UnityEngine;

public class ActionController : MonoBehaviour
{
    [SerializeField] private ActionMemory actionMemory;

    public void ExecuteSlot(int slot)
    {
        if (actionMemory == null)
        {
            Debug.LogError("ActionController has no ActionMemory assigned.");
            return;
        }

        ActionDefinition action = actionMemory.GetAction(slot);

        if (action == null)
        {
            Debug.LogWarning($"No action assigned to slot {slot}.");
            return;
        }

        Debug.Log($"Executing action: {action.DisplayName}");

        actionMemory.ReplaceAction(slot);
    }
}