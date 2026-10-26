using System;
using System.Collections.Generic;

[Serializable]
public sealed class CharacterEffectSaveData
{
    public List<SavedEffect> Effects = new List<SavedEffect>();

    public float StunRemainingTime;
    public EffectDefinition StunDefinition;
}

[Serializable]
public sealed class SavedEffect
{
    public EffectDefinition Definition;
    public float RemainingTime;
}
