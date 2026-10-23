using System.Collections.Generic;
using UnityEngine;

public sealed class TeacherNPC : MonoBehaviour, IInteractable
{
    [Header("Teaching")]
    [SerializeField]
    private List<ActionDefinition> actionsToTeach = new List<ActionDefinition>(3);

    [SerializeField]
    private SkillLearnUI skillLearnUI;

    [Header("After Completion")]
    [SerializeField]
    private string[] completedInteractionTexts;

    private ActionCollection actionCollection;

    private bool interactionCompleted;

    public bool CanInteract(Player player)
    {
        if (player == null || player.ActionCollection == null)
            return false;

        if (!interactionCompleted)
            return true;

        return completedInteractionTexts != null && completedInteractionTexts.Length > 0;
    }

    public void Interact(Player player)
    {
        if (!CanInteract(player))
            return;

        if (interactionCompleted)
        {
            InteractAfterCompletion();
            return;
        }

        if (actionsToTeach == null || actionsToTeach.Count == 0)
        {
            Debug.LogWarning(
                $"[{nameof(TeacherNPC)}] {name} has no actions assigned to teach.",
                this
            );
            return;
        }

        if (skillLearnUI == null)
        {
            Debug.LogWarning($"[{nameof(TeacherNPC)}] {name} has no SkillLearnUI assigned.", this);
            return;
        }

        actionCollection = player.ActionCollection;

        skillLearnUI.Show(this, actionsToTeach);
    }

    public void LearnAction(ActionDefinition chosenAction)
    {
        if (interactionCompleted || chosenAction == null)
            return;

        if (actionCollection == null)
        {
            Debug.LogWarning($"[{nameof(TeacherNPC)}] No ActionCollection is assigned.", this);
            return;
        }

        if (actionsToTeach == null || !actionsToTeach.Contains(chosenAction))
        {
            Debug.LogWarning(
                $"[{nameof(TeacherNPC)}] {chosenAction.name} is not in this "
                    + "NPC's list of teachable actions.",
                this
            );
            return;
        }

        actionCollection.Add(chosenAction);

        interactionCompleted = true;
        actionCollection = null;
    }

    private void InteractAfterCompletion()
    {
        if (completedInteractionTexts == null || completedInteractionTexts.Length == 0)
        {
            return;
        }

        string text = completedInteractionTexts[Random.Range(0, completedInteractionTexts.Length)];

        Debug.Log(text, this);
    }
}
