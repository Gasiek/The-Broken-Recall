using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActionCollection))]
public class ActionMemory : MonoBehaviour
{
    public const int SlotCount = 2;

    private ActionCollection actionCollection;

    private readonly ActionCardInstance[] actions = new ActionCardInstance[SlotCount];

    private readonly List<ActionCardInstance> drawPile = new();

    // Each physical card copy has its own cooldown.
    private readonly Dictionary<ActionCardInstance, int> cooldowns = new();

    // True when gameplay has an active draw-pile cycle.
    private bool hasActiveDrawCycle;

    public IReadOnlyList<ActionCardInstance> Actions => actions;

    public int DrawPileCount => drawPile.Count;

    public event Action MemoryChanged;

    private void Awake()
    {
        actionCollection = GetComponent<ActionCollection>();
        FillMemory();
    }

    public ActionCardInstance GetAction(int slot)
    {
        if (!IsValidSlot(slot))
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
        hasActiveDrawCycle = false;

        // Initial filling does not count as completing a draw cycle.
        for (int slot = 0; slot < SlotCount; slot++)
        {
            actions[slot] = DrawAction(slot, false);
        }

        // Only a pile that still has cards can be exhausted during play.
        hasActiveDrawCycle = drawPile.Count > 0;

        MemoryChanged?.Invoke();
    }

    public void PutOnCooldown(ActionCardInstance card)
    {
        if (card == null || card.Definition == null)
        {
            return;
        }

        int cooldown = card.Definition.ReshuffleCooldown;

        if (cooldown <= 0)
        {
            cooldowns.Remove(card);
            return;
        }

        cooldowns[card] = cooldown;
    }

    public void ReplaceAction(int slot)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogWarning($"Invalid action memory slot: {slot}");
            return;
        }

        actions[slot] = DrawAction(slot);
        MemoryChanged?.Invoke();
    }

    public bool RemoveAndReplace(ActionCardInstance card)
    {
        if (card == null)
        {
            return false;
        }

        int slot = FindSlot(card);

        if (slot < 0)
        {
            return false;
        }

        actions[slot] = DrawAction(slot);
        MemoryChanged?.Invoke();

        return true;
    }

    public bool RemoveAction(ActionCardInstance card)
    {
        if (card == null)
        {
            return false;
        }

        int slot = FindSlot(card);

        if (!actionCollection.Remove(card))
        {
            Debug.LogWarning("[ActionMemory] Card removal failed.");
            return false;
        }

        // Remove only this copy's cooldown and draw-pile entry.
        cooldowns.Remove(card);
        drawPile.Remove(card);

        if (slot >= 0)
        {
            actions[slot] = null;
            actions[slot] = DrawAction(slot);
        }

        MemoryChanged?.Invoke();
        return true;
    }

    public void ResetCooldownsAndReshuffle()
    {
        cooldowns.Clear();

        drawPile.Clear();
        RebuildDrawPile(-1);

        hasActiveDrawCycle = drawPile.Count > 0;

        MemoryChanged?.Invoke();
    }

    private ActionCardInstance DrawAction(int slotBeingReplaced, bool advanceCooldown = true)
    {
        if (drawPile.Count == 0)
        {
            bool completedCycle = hasActiveDrawCycle && advanceCooldown;

            // Consume the completed-cycle marker so an empty pile
            // cannot advance cooldowns repeatedly.
            hasActiveDrawCycle = false;

            RebuildDrawPile(slotBeingReplaced);

            // Build the pile BEFORE advancing cooldowns.
            // Newly ready cards therefore enter the following pile.
            if (completedCycle)
            {
                AdvanceCooldowns();
            }

            hasActiveDrawCycle = drawPile.Count > 0;

            if (drawPile.Count == 0)
            {
                return null;
            }
        }

        int lastIndex = drawPile.Count - 1;
        ActionCardInstance drawnCard = drawPile[lastIndex];

        drawPile.RemoveAt(lastIndex);

        return drawnCard;
    }

    private void RebuildDrawPile(int slotBeingReplaced)
    {
        drawPile.Clear();

        foreach (ActionCardInstance card in actionCollection.Actions)
        {
            if (card == null || card.Definition == null)
            {
                continue;
            }

            // Exclude this particular copy if it is cooling down.
            if (cooldowns.ContainsKey(card))
            {
                continue;
            }

            drawPile.Add(card);
        }

        // Cards currently occupying the other slot cannot be drawn.
        for (int i = 0; i < SlotCount; i++)
        {
            if (i == slotBeingReplaced)
            {
                continue;
            }

            ActionCardInstance assignedCard = actions[i];

            if (assignedCard != null)
            {
                drawPile.Remove(assignedCard);
            }
        }

        ShuffleDrawPile();
    }

    private void AdvanceCooldowns()
    {
        List<ActionCardInstance> readyCards = new();

        foreach (var entry in cooldowns)
        {
            int remaining = entry.Value - 1;

            if (remaining <= 0)
            {
                readyCards.Add(entry.Key);
            }
            else
            {
                cooldowns[entry.Key] = remaining;
            }
        }

        foreach (ActionCardInstance card in readyCards)
        {
            cooldowns.Remove(card);
        }
    }

    private int FindSlot(ActionCardInstance card)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (actions[i] == card)
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < SlotCount;
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
