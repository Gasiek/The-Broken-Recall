using System;
using System.Collections.Generic;
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

    private void Start()
    {
        Initialize();
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
        for (int i = 0; i < SlotCount; i++)
        {
            slots[i] = GetRandomAvailableAction();
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

        ActionDefinition oldAction = slots[slot];

        ActionDefinition newAction = GetRandomAvailableAction(slot);

        if (newAction == null)
        {
            Debug.LogWarning("Could not replace action because no unique action is available.");

            return null;
        }

        slots[slot] = newAction;

        MemoryChanged?.Invoke();

        return oldAction;
    }

    private ActionDefinition GetRandomAvailableAction(int slotToIgnore = -1)
    {
        List<ActionDefinition> availableActions = new();

        foreach (ActionDefinition action in actionCollection.Actions)
        {
            if (action == null)
                continue;

            if (IsInMemory(action, slotToIgnore))
                continue;

            availableActions.Add(action);
        }

        if (availableActions.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, availableActions.Count);

        return availableActions[randomIndex];
    }

    private bool IsInMemory(ActionDefinition action, int slotToIgnore = -1)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == slotToIgnore)
                continue;

            if (slots[i] == action)
                return true;
        }

        return false;
    }

    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < SlotCount;
    }
}
