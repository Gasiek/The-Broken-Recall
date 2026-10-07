using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillRemovalUI : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private Transform buttonContainer;

    [SerializeField]
    private Button actionButtonPrefab;

    private SkillRemoval currentSkillRemoval;

    public void Show(SkillRemoval skillRemoval, ActionCollection actionCollection)
    {
        currentSkillRemoval = skillRemoval;

        ClearButtons();

        foreach (ActionDefinition action in actionCollection.Actions)
        {
            if (action == null)
            {
                continue;
            }

            CreateButton(action);
        }

        panel.SetActive(true);
    }

    private void CreateButton(ActionDefinition action)
    {
        Button button = Instantiate(actionButtonPrefab, buttonContainer);

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text = action.DisplayName;
        }

        button.onClick.AddListener(() => OnActionSelected(action));
    }

    private void OnActionSelected(ActionDefinition action)
    {
        if (currentSkillRemoval == null)
        {
            return;
        }

        currentSkillRemoval.RemoveAction(action);

        Close();
    }

    public void OnCancelPressed()
    {
        Close();
    }

    private void Close()
    {
        ClearButtons();

        currentSkillRemoval = null;

        panel.SetActive(false);
    }

    private void ClearButtons()
    {
        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonContainer.GetChild(i).gameObject);
        }
    }
}
