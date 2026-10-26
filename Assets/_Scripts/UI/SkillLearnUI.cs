using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class SkillLearnUI : MonoBehaviour
{
    [SerializeField]
    private VisualTreeAsset cardTemplate;

    private PanelRenderer panelRenderer;
    private PauseController pauseController;

    private VisualElement backdrop;
    private VisualElement cardContainer;
    private Button cancelButton;

    private TeacherNPC currentTeacher;
    private bool isOpen;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        if (panelRenderer == null)
            panelRenderer = GetComponent<PanelRenderer>();

        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        Close();

        if (cancelButton != null)
            cancelButton.clicked -= HandleCancelClicked;

        backdrop = null;
        cardContainer = null;
        cancelButton = null;
    }

    /// <summary>
    /// Called by the scene's setup code after the player is bound.
    /// </summary>
    public void BindRuntimeReferences(PauseController controller)
    {
        pauseController = controller;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        if (isOpen)
            Close();

        if (cancelButton != null)
            cancelButton.clicked -= HandleCancelClicked;

        backdrop = root.Q<VisualElement>("Backdrop");
        cardContainer = root.Q<VisualElement>("CardContainer");
        cancelButton = root.Q<Button>("CancelButton");

        if (backdrop == null || cardContainer == null || cancelButton == null)
        {
            Debug.LogError(
                "[SkillLearnUI] UXML must contain Backdrop, CardContainer, " + "and CancelButton.",
                this
            );

            Hide();
            return;
        }

        cancelButton.clicked += HandleCancelClicked;
        Hide();
    }

    public void Show(TeacherNPC teacher, List<ActionDefinition> choices)
    {
        if (isOpen)
            return;

        if (teacher == null || choices == null || choices.Count == 0)
        {
            Debug.LogWarning(
                "[SkillLearnUI] Cannot open without a teacher and action choices.",
                this
            );
            return;
        }

        if (pauseController == null)
        {
            Debug.LogWarning(
                "[SkillLearnUI] PauseController has not been bound. "
                    + "Check SceneContext and player scene binding.",
                this
            );
            return;
        }

        if (cardTemplate == null)
        {
            Debug.LogWarning("[SkillLearnUI] Card template is not assigned.", this);
            return;
        }

        if (backdrop == null || cardContainer == null || cancelButton == null)
        {
            Debug.LogWarning("[SkillLearnUI] UI elements have not initialized.", this);
            return;
        }

        currentTeacher = teacher;
        PopulateCards(choices);

        // The central controller owns pause state, input maps, and cursor state.
        if (!pauseController.OpenModal(Close))
        {
            currentTeacher = null;

            if (cardContainer != null)
                cardContainer.Clear();

            return;
        }

        isOpen = true;
        backdrop.style.display = DisplayStyle.Flex;
        backdrop.style.visibility = Visibility.Visible;

        cancelButton.Focus();
    }

    private void PopulateCards(List<ActionDefinition> choices)
    {
        cardContainer.Clear();

        foreach (ActionDefinition actionDef in choices)
        {
            if (actionDef == null)
                continue;

            VisualElement cardElement = cardTemplate.Instantiate();

            Button button = cardElement.Q<Button>("ActionButton");
            Label nameLabel = cardElement.Q<Label>("Name");
            Image iconImage = cardElement.Q<Image>("Icon");
            Label descLabel = cardElement.Q<Label>("Description");

            if (nameLabel != null)
                nameLabel.text = actionDef.DisplayName;

            if (iconImage != null)
            {
                iconImage.image = actionDef.Icon != null ? actionDef.Icon.texture : null;
            }

            if (descLabel != null)
                descLabel.text = actionDef.GetDescription();

            if (button != null)
            {
                ActionDefinition selected = actionDef;
                button.clicked += () => OnActionSelected(selected);
            }

            cardContainer.Add(cardElement);
        }
    }

    private void OnActionSelected(ActionDefinition selectedAction)
    {
        if (!isOpen || currentTeacher == null || selectedAction == null)
            return;

        currentTeacher.LearnAction(selectedAction);
        Close();
    }

    private void HandleCancelClicked()
    {
        Close();
    }

    public void Hide()
    {
        if (backdrop == null)
            return;

        backdrop.style.display = DisplayStyle.None;
        backdrop.style.visibility = Visibility.Hidden;
    }

    public void Close()
    {
        bool wasOpen = isOpen;

        Hide();

        if (cardContainer != null)
            cardContainer.Clear();

        currentTeacher = null;
        isOpen = false;

        if (wasOpen && pauseController != null)
            pauseController.CloseModal();
    }
}
