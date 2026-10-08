using UnityEngine;

[CreateAssetMenu(
    fileName = "NewStatusEffect",
    menuName = "RPG/Combat/Status Effect Definition"
)]
public class StatusEffectDefinition : ScriptableObject
{
    [Header("General")]
    [SerializeField] private string id = "status.effect";
    [SerializeField] private string displayName = "Status Effect";
    [SerializeField] private StatusEffectType effectType = StatusEffectType.Burn;
    [SerializeField] private float duration = 3.0f;

    [Header("Damage Over Time (Burn / Poison)")]
    [Tooltip("How often damage is dealt in seconds.")]
    [SerializeField] private float tickInterval = 1.0f;
    [SerializeField] private int damagePerTick = 5;

    [Header("Movement Penalty (Slow / Freeze / Stun)")]
    [Tooltip("Movement speed multiplier (e.g. 0.5 for 50% slow, 0 for freeze/stun).")]
    [Range(0f, 1f)]
    [SerializeField] private float speedMultiplier = 0.5f;

    [Header("Visual & Audio")]
    [Tooltip("VFX prefab spawned as a child of the victim while effect is active.")]
    [SerializeField] private ParticleSystem vfxPrefab;
    [SerializeField] private AudioClip applySound;
    [SerializeField] private AudioClip tickSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 0.7f;

    public string Id => id;
    public string DisplayName => displayName;
    public StatusEffectType EffectType => effectType;
    public float Duration => duration;
    public float TickInterval => tickInterval;
    public int DamagePerTick => damagePerTick;
    public float SpeedMultiplier => speedMultiplier;
    public ParticleSystem VfxPrefab => vfxPrefab;
    public AudioClip ApplySound => applySound;
    public AudioClip TickSound => tickSound;
    public float SoundVolume => soundVolume;
}
