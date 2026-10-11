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
            return false;
        }

        // Remove one copy from the player's collection.
        if (!actionCollection.Remove(action))
        {
            return false;
        }

        // Remove one matching copy from the current draw pile.
        drawPile.Remove(action);

        // Replace the action if it is currently assigned to a button.
        int slot = FindSlot(action);

        if (slot >= 0)
        {
            actions[slot] = DrawAction(slot);
            MemoryChanged?.Invoke();
        }

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
        // Rebuild only when a draw is requested and the pile is empty.
        if (drawPile.Count == 0)
        {
            RebuildDrawPile(slotBeingReplaced);
        }

        // The collection may be empty, or all cards may be assigned.
        if (drawPile.Count == 0)
        {
            return null;
        }

        // Draw and remove the last card from the pile.
        int lastIndex = drawPile.Count - 1;
        ActionDefinition drawnAction = drawPile[lastIndex];

        drawPile.RemoveAt(lastIndex);

        return drawnAction;
    }

    private void RebuildDrawPile(int slotBeingReplaced)
    {
        drawPile.Clear();
        drawPile.AddRange(actionCollection.Actions);

        // Exclude actions assigned to the other button.
        for (int i = 0; i < SlotCount; i++)
        {
            if (i == slotBeingReplaced)
            {
                continue;
            }

            // Remove only one matching copy.
            drawPile.Remove(actions[i]);
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
