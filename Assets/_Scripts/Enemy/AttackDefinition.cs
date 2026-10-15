using UnityEngine;

public enum EnemyAttackType
{
    Melee,
    Ranged,
}

[CreateAssetMenu(fileName = "AttackDefinition", menuName = "Combat/Attack Definition")]
public class AttackDefinition : ScriptableObject
{
    [Header("General")]
    [SerializeField]
    private EnemyAttackType attackType;

    [SerializeField, Min(0)]
    private int damage = 10;

    [SerializeField, Min(0f)]
    private float cooldown = 1f;

    [SerializeField, Min(0f)]
    private float attackRange = 2f;

    [Header("Animation")]
    [SerializeField]
    private string animationTrigger = "Attack";

    [Header("Melee")]
    [SerializeField, Min(0f)]
    private float attackRadius = 0.5f;

    [SerializeField]
    private AudioClip meleeImpactSound;

    [SerializeField, Range(0f, 1f)]
    private float meleeImpactSoundVolume = 1f;

    [Header("Ranged")]
    [SerializeField, Min(0f)]
    private float minimumRange = 0f;

    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField, Min(0f)]
    private float projectileSpeed = 10f;

    [SerializeField, Min(0f)]
    private float projectileLifetime = 5f;

    [Header("Audio")]
    [SerializeField]
    private AudioClip projectileLaunchSound;

    [SerializeField, Range(0f, 1f)]
    private float projectileLaunchSoundVolume = 1f;

    public AudioClip ProjectileLaunchSound => projectileLaunchSound;
    public float ProjectileLaunchSoundVolume => projectileLaunchSoundVolume;
    public AudioClip MeleeImpactSound => meleeImpactSound;
    public float MeleeImpactSoundVolume => meleeImpactSoundVolume;
    public EnemyAttackType AttackType => attackType;
    public int Damage => damage;
    public float Cooldown => cooldown;
    public float AttackRange => attackRange;
    public string AnimationTrigger => animationTrigger;
    public float AttackRadius => attackRadius;
    public float MinimumRange => minimumRange;
    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;
}
