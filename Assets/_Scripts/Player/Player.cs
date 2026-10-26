using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(ActionCollection))]
[RequireComponent(typeof(ActionMemory))]
[RequireComponent(typeof(CharacterController))]
public sealed class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerDefinition definition;

    public PlayerDefinition Definition => definition;
    public Health Health { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    public ActionCollection ActionCollection { get; private set; }
    public ActionMemory ActionMemory { get; private set; }
    public CharacterEffectManager CharacterEffects { get; private set; }
    public CharacterController CharacterController { get; private set; }
    public Vector3 AimPosition =>
        CharacterController.transform.TransformPoint(CharacterController.center);

    private bool initialized;

    private void Awake()
    {
        Health = GetComponent<Health>();
        StateMachine = GetComponent<PlayerStateMachine>();
        ActionCollection = GetComponent<ActionCollection>();
        ActionMemory = GetComponent<ActionMemory>();
        CharacterController = GetComponent<CharacterController>();
        CharacterEffects = GetComponent<CharacterEffectManager>();
        if (definition == null)
        {
            Debug.LogError("[Player] No PlayerDefinition assigned.", this);
            return;
        }

        if (Health == null)
        {
            Debug.LogError("[Player] Health component is missing.", this);
            return;
        }

        Health.Initialize(definition.MaxHealth);
        initialized = true;
    }

    public PlayerSaveData CaptureState()
    {
        if (!initialized || ActionCollection == null || ActionMemory == null)
        {
            Debug.LogError("[Player] Cannot capture state before player initialization.", this);
            return null;
        }

        PlayerSaveData data = new PlayerSaveData
        {
            CurrentHealth = Health.CurrentHealth,
            ActionMemory = ActionMemory.CaptureState(),
        };

        data.Actions.AddRange(ActionCollection.Actions);
        if (CharacterEffects != null)
            data.CharacterEffects = CharacterEffects.CaptureState();
        return data;
    }

    public void RestoreState(PlayerSaveData data)
    {
        if (data == null)
            return;

        if (!initialized)
        {
            Debug.LogError(
                "[Player] Cannot restore state because initialization failed. "
                    + "Check the PlayerDefinition and Health component.",
                this
            );
            return;
        }

        Health.RestoreHealth(data.CurrentHealth);

        if (ActionCollection != null && data.Actions != null)
        {
            // Remove the prefab's default collection before restoring.
            foreach (ActionCardInstance card in ActionCollection.Actions.ToArray())
                ActionCollection.Remove(card);

            foreach (ActionCardInstance card in data.Actions)
            {
                if (card != null)
                    ActionCollection.Add(card);
            }
        }

        if (data.ActionMemory != null)
            ActionMemory?.RestoreState(data.ActionMemory);
        if (data.CharacterEffects != null)
            CharacterEffects?.RestoreState(data.CharacterEffects);
    }
}
