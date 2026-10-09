using UnityEngine;

public class SkillRemoval : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ActionCollection actionCollection;

    [SerializeField]
    private ActionMemory actionMemory;

    [SerializeField]
    private SkillRemovalUI skillRemovalUI;

    private bool interactionCompleted;

    public bool CanInteract => !interactionCompleted;

    public void Interact()
    {
        if (interactionCompleted)
        {
            return;
        }

        if (actionCollection == null)
        {
            Debug.LogWarning($"{name} has no ActionCollection assigned.");

            return;
        }

        if (skillRemovalUI == null)
        {
            Debug.LogWarning($"{name} has no SkillRemovalUI assigned.");

            return;
        }

        skillRemovalUI.Show(this, actionCollection);
    }

    public void RemoveAction(ActionDefinition action)
    {
        if (interactionCompleted)
        {
            return;
        }

        if (action == null)
        {
            return;
        }

        if (actionCollection == null)
        {
            Debug.LogWarning($"{name} has no ActionCollection assigned.");

            return;
        }

        if (actionMemory == null)
        {
            Debug.LogWarning($"{name} has no ActionMemory assigned.");

            return;
        }

        bool removed = actionCollection.Remove(action);

        if (!removed)
        {
            return;
        }

        actionMemory.RemoveAndReplace(action);

        interactionCompleted = true;
    }
}
