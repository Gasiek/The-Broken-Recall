using UnityEngine;

public sealed class SkillRemoval : MonoBehaviour, IInteractable
{
    [SerializeField]
    private SkillRemovalUI skillRemovalUI;

    private ActionCollection actionCollection;
    private ActionMemory actionMemory;

    private bool interactionCompleted;

    public bool CanInteract(Player player)
    {
        return player != null
            && player.ActionCollection != null
            && player.ActionMemory != null
            && !interactionCompleted;
    }

    public void Interact(Player player)
    {
        if (!CanInteract(player))
            return;

        if (skillRemovalUI == null)
        {
            Debug.LogWarning(
                $"[{nameof(SkillRemoval)}] {name} has no SkillRemovalUI assigned.",
                this
            );
            return;
        }

        actionCollection = player.ActionCollection;
        actionMemory = player.ActionMemory;

        skillRemovalUI.Show(this, actionCollection);
    }

    public void RemoveAction(ActionCardInstance card)
    {
        if (interactionCompleted || card == null)
            return;

        if (actionMemory == null)
        {
            Debug.LogWarning($"[{nameof(SkillRemoval)}] No ActionMemory is assigned.", this);
            return;
        }

        bool removed = actionMemory.RemoveAction(card);

        if (!removed)
            return;

        interactionCompleted = true;

        actionCollection = null;
        actionMemory = null;
    }
}
