using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Game/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Stats")]
    [SerializeField]
    private int maxHealth = 100;

    [SerializeField]
    private float moveSpeed = 3.5f;

    [Header("Attack")]
    [SerializeField]
    private int attackDamage = 10;

    [SerializeField]
    private float attackCooldown = 1f;

    [SerializeField]
    private float attackRange = 1.5f;

    [SerializeField]
    private float attackRadius = 0.35f;

    [SerializeField]
    private float attackDuration = 0.4f;

    public int MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;

    public int AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;
    public float AttackRange => attackRange;
    public float AttackRadius => attackRadius;
    public float AttackDuration => attackDuration;
}
