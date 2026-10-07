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

        skillRemovalUI.Show(this, actionCollection);
    }

    public void RemoveAction(ActionDefinition action)
    {
        if (action == null)
        {
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
