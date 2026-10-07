using UnityEngine;
using UnityEngine.UI;

public class ActionSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private Image cooldownImage;

    [SerializeField]
    private TMPro.TMP_Text inputText;

    [SerializeField]
    private TMPro.TMP_Text actionNameText;

    public void SetAction(ActionDefinition action)
    {
        if (action == null)
        {
            Clear();
            return;
        }

        iconImage.sprite = action.Icon;
        iconImage.enabled = action.Icon != null;

        actionNameText.text = action.DisplayName;
    }

    public void SetCooldown(float normalizedProgress)
    {
        if (cooldownImage == null)
        {
            return;
        }

        cooldownImage.fillAmount = Mathf.Clamp01(normalizedProgress);
        cooldownImage.enabled = normalizedProgress > 0f;
    }

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;

        actionNameText.text = string.Empty;

        SetCooldown(0f);
    }
}
