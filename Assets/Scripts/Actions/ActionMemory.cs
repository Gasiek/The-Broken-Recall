using System;
using UnityEngine;

[RequireComponent(typeof(ActionCollection))]
public class ActionMemory : MonoBehaviour
{
    public const int SlotCount = 3;

    private ActionCollection actionCollection;

    [SerializeField]
    private ActionDefinition[] slots = new ActionDefinition[SlotCount];

    public event Action MemoryChanged;

    private void Awake()
    {
        actionCollection = GetComponent<ActionCollection>();
    }

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
}
