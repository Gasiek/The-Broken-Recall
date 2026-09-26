using UnityEngine;

public class TeacherNPC : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ActionDefinition actionToTeach;

    [SerializeField]
    private ActionCollection actionCollection;

    [SerializeField]
    private SkillLearnUI skillLearnUI;

    private bool hasTaught;

    public void Interact()
    {
        if (hasTaught)
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
            hasTaught = true;
            return;
        }

        skillLearnUI.Show(this, actionToTeach);
    }

    public void LearnAction()
    {
        if (hasTaught)
        {
            return;
        }

        if (actionToTeach == null)
        {
            return;
        }

        if (actionCollection == null)
        {
            return;
        }

        if (actionCollection.Contains(actionToTeach))
        {
            hasTaught = true;
            return;
        }

        actionCollection.Add(actionToTeach);

        hasTaught = true;
    }
}
