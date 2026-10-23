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
    private readonly Dictionary<ActionCardInstance, int> cooldowns = new();

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
        return IsValidSlot(slot) ? actions[slot] : null;
    }

    public void FillMemory()
    {
        Array.Clear(actions, 0, actions.Length);
        drawPile.Clear();
        hasActiveDrawCycle = false;

        for (int slot = 0; slot < SlotCount; slot++)
            actions[slot] = DrawAction(slot, false);

        hasActiveDrawCycle = drawPile.Count > 0;
        MemoryChanged?.Invoke();
    }

    public void PutOnCooldown(ActionCardInstance card)
    {
        if (card == null || card.Definition == null)
            return;

        int cooldown = card.Definition.ReshuffleCooldown;

        if (cooldown <= 0)
            cooldowns.Remove(card);
        else
            cooldowns[card] = cooldown;
    }

    public void ReplaceAction(int slot)
    {
        if (!IsValidSlot(slot))
            return;

        actions[slot] = DrawAction(slot);
        MemoryChanged?.Invoke();
    }

    public bool RemoveAndReplace(ActionCardInstance card)
    {
        int slot = FindSlot(card);

        if (slot < 0)
            return false;

        return RemoveAction(card);
    }

    public bool RemoveAction(ActionCardInstance card)
    {
        if (card == null)
            return false;

        int slot = FindSlot(card);

        if (!actionCollection.Remove(card))
        {
            Debug.LogWarning("[ActionMemory] Card was not in the collection.", this);
            return false;
        }

        cooldowns.Remove(card);
        drawPile.Remove(card);

        if (slot >= 0)
            actions[slot] = DrawAction(slot);

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

    public ActionMemorySaveData CaptureState()
    {
        var data = new ActionMemorySaveData();

        data.Collection.AddRange(actionCollection.Actions);
        data.DrawPile.AddRange(drawPile);
        data.Slots.AddRange(actions);
        data.HasActiveDrawCycle = hasActiveDrawCycle;

        foreach (var entry in cooldowns)
        {
            data.Cooldowns.Add(
                new ActionCooldownSaveData { Card = entry.Key, RemainingDrawCycles = entry.Value }
            );
        }

        return data;
    }

    public void RestoreState(ActionMemorySaveData data)
    {
        if (data == null)
        {
            FillMemory();
            return;
        }

        // Replace the collection first so all restored cards belong to it.
        actionCollection.ReplaceAll(data.Collection);

        Array.Clear(actions, 0, actions.Length);
        drawPile.Clear();
        cooldowns.Clear();

        foreach (ActionCardInstance card in data.DrawPile)
        {
            if (card != null && actionCollection.Contains(card))
                drawPile.Add(card);
        }

        for (int i = 0; i < Mathf.Min(SlotCount, data.Slots.Count); i++)
        {
            ActionCardInstance card = data.Slots[i];

            if (card != null && actionCollection.Contains(card))
                actions[i] = card;
        }

        foreach (ActionCooldownSaveData entry in data.Cooldowns)
        {
            if (entry == null || entry.Card == null)
                continue;

            if (actionCollection.Contains(entry.Card) && entry.RemainingDrawCycles > 0)
                cooldowns[entry.Card] = entry.RemainingDrawCycles;
        }

        hasActiveDrawCycle = data.HasActiveDrawCycle;
        MemoryChanged?.Invoke();
    }

    private ActionCardInstance DrawAction(int slotBeingReplaced, bool advanceCooldown = true)
    {
        if (drawPile.Count == 0)
        {
            bool completedCycle = hasActiveDrawCycle && advanceCooldown;

            hasActiveDrawCycle = false;
            RebuildDrawPile(slotBeingReplaced);

            if (completedCycle)
                AdvanceCooldowns();

            hasActiveDrawCycle = drawPile.Count > 0;

            if (drawPile.Count == 0)
                return null;
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
                continue;

            if (cooldowns.ContainsKey(card))
                continue;

            drawPile.Add(card);
        }

        for (int i = 0; i < SlotCount; i++)
        {
            if (i == slotBeingReplaced)
                continue;

            ActionCardInstance assignedCard = actions[i];

            if (assignedCard != null)
                drawPile.Remove(assignedCard);
        }

        ShuffleDrawPile();
    }

    private void AdvanceCooldowns()
    {
        var readyCards = new List<ActionCardInstance>();
        var updatedCooldowns = new Dictionary<ActionCardInstance, int>();

        foreach (var entry in cooldowns)
        {
            int remaining = entry.Value - 1;

            if (remaining <= 0)
                readyCards.Add(entry.Key);
            else
                updatedCooldowns[entry.Key] = remaining;
        }

        cooldowns.Clear();

        foreach (var entry in updatedCooldowns)
            cooldowns[entry.Key] = entry.Value;

        foreach (ActionCardInstance card in readyCards)
            cooldowns.Remove(card);
    }

    private int FindSlot(ActionCardInstance card)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i] == card)
                return i;
        }

        return -1;
    }

    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < SlotCount;
    }

    private void ShuffleDrawPile()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
        }
    }
}
