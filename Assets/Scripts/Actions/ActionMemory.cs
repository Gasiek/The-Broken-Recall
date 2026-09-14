using System;
using UnityEngine;

public class ActionMemory : MonoBehaviour
{
    public const int SlotCount = 3;

    [SerializeField]
    private ActionCollection actionCollection;

    [SerializeField]
    private ActionDefinition[] slots = new ActionDefinition[SlotCount];

    public event Action MemoryChanged;

    public ActionDefinition GetAction(int slot)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogWarning($"Invalid action memory slot: {slot}");
            return null;
        }

        return slots[slot];
    }

    public void Initialize()
    {
        if (actionCollection == null)
        {
            Debug.LogError("ActionMemory has no ActionCollection assigned.");
            return;
        }

        for (int i = 0; i < SlotCount; i++)
        {
            slots[i] = actionCollection.GetRandomAction();
        }

        LogMemory();

        MemoryChanged?.Invoke();
    }

    public ActionDefinition ReplaceAction(int slot)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogWarning($"Invalid action memory slot: {slot}");
            return null;
        }

        if (actionCollection == null)
        {
            Debug.LogError("ActionMemory has no ActionCollection assigned.");
            return null;
        }

        ActionDefinition oldAction = slots[slot];
        ActionDefinition newAction = actionCollection.GetRandomAction();

        slots[slot] = newAction;

        LogMemory();

        MemoryChanged?.Invoke();

        return oldAction;
    }

    private void Start()
    {
        Initialize();
    }

    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < SlotCount;
    }

    private void LogMemory()
    {
        Debug.Log(
            $"Action Memory:\n"
                + $"[1] {GetActionName(0)}\t"
                + $"[2] {GetActionName(1)}\t"
                + $"[3] {GetActionName(2)}"
        );
    }

    private string GetActionName(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return "Invalid";
        }

        if (slots[slot] == null)
        {
            return "Empty";
        }

        return slots[slot].DisplayName;
    }
}
