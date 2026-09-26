using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Definition")]
public class PlayerDefinition : ScriptableObject
{
    [Header("Health")]
    [SerializeField]
    private int maxHealth = 100;

    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float rotationSpeed = 15f;

    [SerializeField]
    private float gravity = -20f;

    [Header("Dash")]
    [SerializeField]
    private float dashDistance = 5f;

    [SerializeField]
    private float dashDuration = 0.2f;

    [SerializeField]
    private float dashCooldown = 0.5f;

    public int MaxHealth => maxHealth;

    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float Gravity => gravity;

    public float DashDistance => dashDistance;
    public float DashDuration => dashDuration;
    public float DashCooldown => dashCooldown;
}
