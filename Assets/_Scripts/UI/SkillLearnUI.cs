using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class SkillLearnUI : MonoBehaviour
{
    [SerializeField]
    private VisualTreeAsset cardTemplate;

    private PanelRenderer panelRenderer;
    private PlayerInput playerInput;

    private VisualElement backdrop;
    private VisualElement cardContainer;
    private Button cancelButton;

    private TeacherNPC currentTeacher;
    private bool isOpen;
    private bool cameraPausedByThisPanel;
    private float previousTimeScale = 1f;
    private string previousActionMap;

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

        ReleasePause();

        if (cancelButton != null)
            cancelButton.clicked -= HandleCancelClicked;

        backdrop = null;
        cardContainer = null;
        cancelButton = null;
    }

    /// <summary>
    /// Called by SceneContext after the current player
    /// have been connected.
    /// </summary>
    public void BindRuntimeReferences(
        PlayerInput currentPlayerInput
    )
    {
        playerInput = currentPlayerInput;
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

        if (playerInput == null)
        {
            Debug.LogWarning(
                "[SkillLearnUI] Runtime references have not been bound. "
                    + "Check SceneContext and SceneTransitionManager setup.",
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

        previousTimeScale = Time.timeScale;
        previousActionMap =
            playerInput.currentActionMap != null ? playerInput.currentActionMap.name : null;

        isOpen = true;
        Time.timeScale = 0f;

        playerInput.SwitchCurrentActionMap("UI");
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        cameraPausedByThisPanel = true;

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

    private void Close()
    {
        Hide();

        if (cardContainer != null)
            cardContainer.Clear();

        currentTeacher = null;
        isOpen = false;

        ReleasePause();
    }

    private void ReleasePause()
    {
        if (!isOpen && !cameraPausedByThisPanel && previousActionMap == null)
            return;
        if (isOpen)
            isOpen = false;

        Time.timeScale = previousTimeScale;

        if (playerInput != null && !string.IsNullOrEmpty(previousActionMap))
            playerInput.SwitchCurrentActionMap(previousActionMap);

        if (cameraPausedByThisPanel)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        cameraPausedByThisPanel = false;
        previousActionMap = null;
    }
}
