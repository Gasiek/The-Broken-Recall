using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(ActionCollection))]
[RequireComponent(typeof(ActionMemory))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerDefinition definition;

    [SerializeField]
    private Transform weapon;

    [Tooltip("Lefty loose")]
    [SerializeField]
    private Transform leftHand;

    public PlayerDefinition Definition => definition;
    public Transform Weapon => weapon;
    public Health Health { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    public ActionCollection ActionCollection { get; private set; }
    public ActionMemory ActionMemory { get; private set; }
    public Transform LeftHand => leftHand;

    private void Awake()
    {
        Health = GetComponent<Health>();
        StateMachine = GetComponent<PlayerStateMachine>();
        ActionCollection = GetComponent<ActionCollection>();
        ActionMemory = GetComponent<ActionMemory>();
        if (definition == null)
        {
            Debug.LogError("[Player] No PlayerDefinition assigned.", this);

            return;
        }

        Health.Initialize(definition.MaxHealth);
    }
}
