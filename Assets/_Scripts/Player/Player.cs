using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(ActionCollection))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerDefinition definition;

    public PlayerDefinition Definition => definition;
    public Health Health { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    public ActionCollection ActionCollection { get; private set; }

    private void Awake()
    {
        Health = GetComponent<Health>();
        StateMachine = GetComponent<PlayerStateMachine>();
        ActionCollection = GetComponent<ActionCollection>();

        if (definition == null)
        {
            Debug.LogError("[Player] No PlayerDefinition assigned.", this);

            return;
        }

        Health.Initialize(definition.MaxHealth);
    }
}
