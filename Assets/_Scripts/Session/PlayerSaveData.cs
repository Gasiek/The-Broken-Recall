using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PlayerSaveData
{
    public int CurrentHealth;

    public List<ActionCardInstance> Actions = new();

    public ActionMemorySaveData ActionMemory = new();

    public WeaponBuffSaveData WeaponBuff = new();
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

[Serializable]
public sealed class WeaponBuffSaveData
{
    public WeaponBuffDefinition Definition;

    public bool HasCustomBuff;
    public int CustomBonusDamage;

    public LightningVfx CustomSwordLightningVfx;
    public ParticleSystem CustomOnHitVfx;

    public AudioClip CustomOnHitSound;
    public float CustomVolume = 1f;

    public float RemainingDuration;
}
