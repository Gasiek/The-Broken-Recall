using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActionCollection))]
public class ActionMemory : MonoBehaviour
{
    public const int SlotCount = 2;

    private ActionCollection actionCollection;

    private readonly ActionDefinition[] actions = new ActionDefinition[SlotCount];

    public IReadOnlyList<ActionDefinition> Actions => actions;

    public event Action MemoryChanged;

    private void Awake()
    {
        actionCollection = GetComponent<ActionCollection>();
        FillMemory();
    }

    public ActionDefinition GetAction(int slot)
    {
        if (slot < 0 || slot >= SlotCount)
        {
            Debug.LogWarning($"Invalid action memory slot: {slot}");
            return null;
        }

        return actions[slot];
    }

    public void FillMemory()
    {
        for (int slot = 0; slot < SlotCount; slot++)
        {
            actions[slot] = GetRandomAvailableAction();
        }

        MemoryChanged?.Invoke();
    }

    public void ReplaceAction(int slot)
    {
        if (slot < 0 || slot >= SlotCount)
        {
            Debug.LogWarning($"Invalid action memory slot: {slot}");
            return;
        }

        actions[slot] = GetRandomAvailableAction();

        MemoryChanged?.Invoke();
    }

    public bool RemoveAndReplace(ActionDefinition action)
    {
        if (action == null)
        {
            return false;
        }

        int slot = FindSlot(action);

        if (slot < 0)
        {
            return false;
        }

        actions[slot] = GetRandomAvailableAction();

        MemoryChanged?.Invoke();

        return true;
    }

    private int FindSlot(ActionDefinition action)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (actions[i] == action)
            {
                return i;
            }
        }

        return -1;
    }

    private ActionDefinition GetRandomAvailableAction()
    {
        if (actionCollection == null)
        {
            Debug.LogWarning("ActionMemory has no ActionCollection.");

            return null;
        }

        if (actionCollection.Actions.Count == 0)
        {
            return null;
        }

        List<ActionDefinition> availableActions = new();
        availableActions.AddRange(actionCollection.Actions);
        for (int i = 0; i < SlotCount; i++)
        {
            availableActions.Remove(actions[i]);
        }

        if (availableActions.Count == 0)
        {
            Debug.LogWarning("No more available actions.");
        }

        int index = UnityEngine.Random.Range(0, availableActions.Count);

        ActionDefinition chosenAction = availableActions[index];
        int count = 0;
        int countAll = 0;
        foreach (ActionDefinition a in availableActions)
        {
            countAll++;
            if (a == chosenAction)
            {
                count++;
            }
        }
        Debug.LogWarning($"Chances of getting {chosenAction.name}: {count} / {countAll}");

        return chosenAction;
    }
}
