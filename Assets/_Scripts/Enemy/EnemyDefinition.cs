using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "RPG/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Stats")]
    [SerializeField, Min(1)]
    private int maxHealth = 100;

    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 3.5f;

    [SerializeField, Min(0f)]
    private float patrolSpeed = 2f;

    [Header("Attacks")]
    [SerializeField]
    private AttackDefinition[] attacks = new AttackDefinition[0];

    [SerializeField, Min(0f)]
    private float generalAttackCooldown = 2f;

    [SerializeField, Range(0f, 100f)]
    private float superAttackChance = 20f;

    public int MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float PatrolSpeed => patrolSpeed;
    public AttackDefinition[] Attacks => attacks;

    public float GeneralAttackCooldown => generalAttackCooldown;
    public float SuperAttackChance => superAttackChance;

    private void OnValidate()
    {
        if (attacks == null)
            attacks = new AttackDefinition[0];

        for (int i = 0; i < attacks.Length; i++)
        {
            if (attacks[i] == null)
            {
                Debug.LogWarning($"{name}: Attack slot {i} is empty.", this);
            }
        }
    }
}
