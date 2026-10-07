using UnityEngine;

[RequireComponent(typeof(Health))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerDefinition definition;

    private Health health;

    public PlayerDefinition Definition => definition;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (definition == null)
        {
            Debug.LogError("Player has no PlayerDefinition assigned.");
            return;
        }

        health.Initialize(definition.MaxHealth);
    }
}
