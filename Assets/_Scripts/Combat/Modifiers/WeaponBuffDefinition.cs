using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWeaponBuff",
    menuName = "RPG/Combat/Weapon Buff Definition"
)]
public class WeaponBuffDefinition : ScriptableObject
{
    [Header("General")]
    [SerializeField] private string id = "buff.weapon";
    [SerializeField] private string displayName = "Weapon Buff";
    [SerializeField] private WeaponBuffMode mode = WeaponBuffMode.SingleHit;

    [Tooltip("Only used if Mode is set to Duration.")]
    [SerializeField] private float duration = 5.0f;

    [Header("Damage Modifiers")]
    [SerializeField] private int flatBonusDamage = 10;
    [SerializeField] private float damageMultiplier = 1.0f;

    [Header("Lifesteal")]
    [Tooltip("Fraction of dealt damage converted to player healing (e.g. 0.25 = 25%).")]
    [Range(0f, 1f)]
    [SerializeField] private float lifestealPercentage = 0f;

    [Header("On-Hit Status Effect")]
    [Tooltip("Optional status effect inflicted onto targets hit by this buff.")]
    [SerializeField] private StatusEffectDefinition statusEffectOnHit;

    [Header("Visual Effects")]
    [Tooltip("Looping VFX attached to the player's sword while buff is active.")]
    [SerializeField] private ParticleSystem swordVfxPrefab;

    [Tooltip("Burst VFX spawned at the impact point when hitting an enemy.")]
    [SerializeField] private ParticleSystem onHitVfxPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip applySound;
    [SerializeField] private AudioClip onHitSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 0.8f;

    public string Id => id;
    public string DisplayName => displayName;
    public WeaponBuffMode Mode => mode;
    public float Duration => duration;
    public int FlatBonusDamage => flatBonusDamage;
    public float DamageMultiplier => damageMultiplier;
    public float LifestealPercentage => lifestealPercentage;
    public StatusEffectDefinition StatusEffectOnHit => statusEffectOnHit;
    public ParticleSystem SwordVfxPrefab => swordVfxPrefab;
    public ParticleSystem OnHitVfxPrefab => onHitVfxPrefab;
    public AudioClip ApplySound => applySound;
    public AudioClip OnHitSound => onHitSound;
    public float SoundVolume => soundVolume;
}
