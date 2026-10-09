using UnityEngine;

public class TeacherNPC : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ActionDefinition actionToTeach;

    [SerializeField]
    private ActionCollection actionCollection;

    [SerializeField]
    private SkillLearnUI skillLearnUI;

    [SerializeField]
    private string[] completedInteractionTexts;

    private bool interactionCompleted;

    public bool CanInteract
    {
        get
        {
            if (!interactionCompleted)
            {
                return true;
            }

            return completedInteractionTexts != null && completedInteractionTexts.Length > 0;
        }
    }

    public void Interact()
    {
        if (interactionCompleted)
        {
            InteractAfterCompletion();
            return;
        }

        if (actionToTeach == null)
        {
            Debug.LogWarning($"{name} has no action assigned to teach.");
            return;
        }

        if (actionCollection == null)
        {
            Debug.LogWarning($"{name} has no ActionCollection assigned.");
            return;
        }

        if (skillLearnUI == null)
        {
            Debug.LogWarning($"{name} has no SkillLearnUI assigned.");
            return;
        }

        if (actionCollection.Contains(actionToTeach))
        {
            interactionCompleted = true;
            return;
        }

        skillLearnUI.Show(this, actionToTeach);
    }

    public void LearnAction()
    {
        if (interactionCompleted)
        {
            return;
        }

        if (actionToTeach == null)
        {
            Debug.LogWarning($"{name} has no action assigned to teach.");
            return;
        }

        if (actionCollection == null)
        {
            Debug.LogWarning($"{name} has no ActionCollection assigned.");
            return;
        }

        if (actionCollection.Contains(actionToTeach))
        {
            interactionCompleted = true;
            return;
        }

        actionCollection.Add(actionToTeach);

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
