using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class InteractionPromptUI : MonoBehaviour
{
    private const string PromptElementName = "InteractionPrompt";
    private const string PromptTextElementName = "InteractionPromptText";
    private const string DefaultPromptText = "Press E to interact";

    private PanelRenderer panelRenderer;
    private VisualElement promptElement;
    private Label promptText;

    private bool isVisible;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        if (panelRenderer == null)
            panelRenderer = GetComponent<PanelRenderer>();

        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        ApplyVisibility();
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        isVisible = false;
        promptElement = null;
        promptText = null;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        promptElement = root.Q<VisualElement>(PromptElementName);
        promptText = root.Q<Label>(PromptTextElementName);

        if (promptElement == null)
        {
            Debug.LogError(
                $"[InteractionPromptUI] Could not find "
                    + $"'{PromptElementName}' in the UI document.",
                this
            );
            return;
        }

        if (promptText == null)
        {
            Debug.LogError(
                $"[InteractionPromptUI] Could not find "
                    + $"'{PromptTextElementName}' in the UI document.",
                this
            );
        }
        else
        {
            promptText.text = DefaultPromptText;
        }

        ApplyVisibility();
    }

    public void Show()
    {
        isVisible = true;

        if (promptText != null)
            promptText.text = DefaultPromptText;

        ApplyVisibility();
    }

    public void Hide()
    {
        isVisible = false;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (promptElement == null)
            return;

        promptElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
