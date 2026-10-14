using System.Collections.Generic;
using UnityEngine;

public class TeacherNPC : MonoBehaviour, IInteractable
{
    [SerializeField]
    private List<ActionDefinition> actionsToTeach = new List<ActionDefinition>(3);

    private ActionCollection actionCollection;

    [SerializeField]
    private SkillLearnUI skillLearnUI;

    [SerializeField]
    private string[] completedInteractionTexts;

    private bool interactionCompleted;

    public bool CanInteract(Player player)
    {
        if (player == null || player.ActionCollection == null)
        {
            return false;
        }

        if (!interactionCompleted)
        {
            return true;
        }

        return completedInteractionTexts != null && completedInteractionTexts.Length > 0;
    }

    public void Interact(Player player)
    {
        if (!CanInteract(player))
        {
            return;
        }

        if (interactionCompleted)
        {
            InteractAfterCompletion();
            return;
        }

        if (actionsToTeach == null || actionsToTeach.Count == 0)
        {
            Debug.LogWarning($"{name} has no actions assigned to teach.");
            return;
        }

        if (skillLearnUI == null)
        {
            Debug.LogWarning($"{name} has no SkillLearnUI assigned.");
            return;
        }

        actionCollection = player.ActionCollection;

        skillLearnUI.Show(this, actionsToTeach);
    }

    public void LearnAction(ActionDefinition chosenAction)
    {
        if (interactionCompleted)
        {
            return;
        }

        if (chosenAction == null)
        {
            Debug.LogWarning($"{name} was given a null action to learn.");
            return;
        }

        if (actionCollection == null)
        {
            Debug.LogWarning($"{name} has no ActionCollection assigned.");
            return;
        }

        // Add chosen action to player's collection
        actionCollection.Add(chosenAction);

        interactionCompleted = true;
    }

    private void InteractAfterCompletion()
    {
        if (completedInteractionTexts == null || completedInteractionTexts.Length == 0)
        {
            return;
        }

        string text = completedInteractionTexts[Random.Range(0, completedInteractionTexts.Length)];

        Debug.Log(text);
    }
}
