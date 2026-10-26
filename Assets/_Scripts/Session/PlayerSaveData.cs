using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PlayerSaveData
{
    public int CurrentHealth;

    public List<ActionCardInstance> Actions = new();

    public ActionMemorySaveData ActionMemory = new();

    public CharacterEffectSaveData CharacterEffects = new();
}

[Serializable]
public sealed class ActionMemorySaveData
{
    public List<ActionCardInstance> Collection = new();
    public List<ActionCardInstance> DrawPile = new();
    public List<ActionCardInstance> Slots = new();

    public List<ActionCooldownSaveData> Cooldowns = new();

    public bool HasActiveDrawCycle;
}

[Serializable]
public sealed class ActionCooldownSaveData
{
    public ActionCardInstance Card;
    public int RemainingDrawCycles;
}
