using UnityEngine;

public class SkillRemoval : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ActionCollection actionCollection;

    [SerializeField]
    private ActionMemory actionMemory;

    [SerializeField]
    private SkillRemovalUI skillRemovalUI;
    private bool hasRemoved;

    public void Interact()
    {
        if (hasRemoved)
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

        hasRemoved = true;
    }
}
