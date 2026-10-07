using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField]
    private GameObject promptObject;

    [SerializeField]
    private TextMeshProUGUI promptText;

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        if (promptText != null)
        {
            promptText.text = "Press E to interact";
        }

        if (promptObject != null)
        {
            promptObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }
}
