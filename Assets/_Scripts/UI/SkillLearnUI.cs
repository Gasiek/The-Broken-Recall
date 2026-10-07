using TMPro;
using UnityEngine;

public class SkillLearnUI : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private TMP_Text questionText;

    private TeacherNPC currentTeacher;

    public void Show(TeacherNPC teacher, ActionDefinition action)
    {
        currentTeacher = teacher;

        questionText.text = $"Do you want to learn {action.DisplayName}?";

        panel.SetActive(true);
    }

    public void OnYesPressed()
    {
        if (currentTeacher == null)
        {
            return;
        }

        currentTeacher.LearnAction();

        Close();
    }

    public void OnNoPressed()
    {
        Close();
    }

    private void Close()
    {
        currentTeacher = null;
        panel.SetActive(false);
    }
}
