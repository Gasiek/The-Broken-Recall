using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActionCollection))]
public class ActionMemory : MonoBehaviour
{
    public const int SlotCount = 2;

    private ActionCollection actionCollection;

    private readonly ActionDefinition[] actions = new ActionDefinition[SlotCount];

    private readonly List<ActionDefinition> drawPile = new();

    public IReadOnlyList<ActionDefinition> Actions => actions;
    public int DrawPileCount => drawPile.Count;

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
        Array.Clear(actions, 0, actions.Length);
        drawPile.Clear();

        for (int slot = 0; slot < SlotCount; slot++)
        {
            actions[slot] = DrawAction(slot);
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

        actions[slot] = DrawAction(slot);

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

        actions[slot] = DrawAction(slot);

        MemoryChanged?.Invoke();
        return true;
    }

    public bool RemoveAction(ActionDefinition action)
    {
        if (action == null)
        {
            Debug.LogWarning("[ActionMemory] Removal aborted: action is NULL.");
            return false;
        }

        int slot = FindSlot(action);

        if (!actionCollection.Remove(action))
        {
            Debug.LogWarning("[ActionMemory] Collection removal FAILED.");
            return false;
        }


        if (slot >= 0)
        {
            actions[slot] = null;

            actions[slot] = DrawAction(slot);

        }
        else
        {
            drawPile.Remove(action);
        }

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

    private ActionDefinition DrawAction(int slotBeingReplaced)
    {
        if (drawPile.Count == 0)
        {
            RebuildDrawPile(slotBeingReplaced);
        }

        if (drawPile.Count == 0)
        {
            return null;
        }

        int lastIndex = drawPile.Count - 1;
        ActionDefinition drawnAction = drawPile[lastIndex];

        drawPile.RemoveAt(lastIndex);

        return drawnAction;
    }

    private void RebuildDrawPile(int slotBeingReplaced)
    {
        drawPile.Clear();
        drawPile.AddRange(actionCollection.Actions);

        // Exclude actions assigned to every other slot.
        for (int i = 0; i < SlotCount; i++)
        {
            if (i == slotBeingReplaced)
                continue;

            ActionDefinition assignedAction = actions[i];

            if (assignedAction != null)
                drawPile.Remove(assignedAction);
        }

        ShuffleDrawPile();
    }

    private void ShuffleDrawPile()
    {
        // Fisher-Yates shuffle.
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
        }
    }
}
