using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    StatModifier,
    Stun,
    NextHitBonus,
}

[CreateAssetMenu(fileName = "NewEffect", menuName = "RPG/Effects/Effect Definition")]
public class EffectDefinition : ScriptableObject
{
    [Header("General")]
    [SerializeField]
    private string displayName = "New Effect";

    [SerializeField]
    private EffectType effectType;

    [Header("Duration")]
    [Tooltip("Duration in seconds. Next-hit effects can also expire after this time.")]
    [Min(0f)]
    [SerializeField]
    private float duration = 5f;

    [Header("Stat Modifiers")]
    [Tooltip("1 = normal speed; 1.5 = 50% faster.")]
    [Min(0f)]
    [SerializeField]
    private float speedMultiplier = 1f;

    [Tooltip("1 = normal damage; 1.5 = 50% more damage.")]
    [Min(0f)]
    [SerializeField]
    private float damageMultiplier = 1f;

    [Min(0)]
    [SerializeField]
    private int bonusDamage;

    [Header("Visuals")]
    [SerializeField]
    private List<EffectVisualConfig> visuals = new List<EffectVisualConfig>();

    public string DisplayName => displayName;
    public EffectType Type => effectType;
    public float Duration => duration;
    public float SpeedMultiplier => speedMultiplier;
    public float DamageMultiplier => damageMultiplier;
    public int BonusDamage => bonusDamage;
    public IReadOnlyList<EffectVisualConfig> Visuals => visuals;
}
