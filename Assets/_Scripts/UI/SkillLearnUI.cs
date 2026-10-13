using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SkillLearnUI : MonoBehaviour
{
    [SerializeField]
    private CameraFollow cameraFollow;

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private VisualTreeAsset cardTemplate;

    private PanelRenderer panelRenderer;
    private VisualElement backdrop;
    private VisualElement cardContainer;
    private Button cancelButton;

    private TeacherNPC currentTeacher;
    private bool isOpen;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();

        if (panelRenderer != null)
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        backdrop = root.Q<VisualElement>("Backdrop");
        cardContainer = root.Q<VisualElement>("CardContainer");
        cancelButton = root.Q<Button>("CancelButton");

        if (cancelButton != null)
        {
            cancelButton.clicked += HandleCancelClicked;
        }

        Hide();
    }

    public void Show(TeacherNPC teacher, List<ActionDefinition> choices)
    {
        if (isOpen)
        {
            return;
        }

        if (teacher == null || choices == null || choices.Count == 0)
        {
            Debug.LogWarning("Cannot show SkillLearnUI without a teacher and action choices.");
            return;
        }

        if (cameraFollow == null || playerInput == null)
        {
            Debug.LogWarning("SkillLearnUI is missing CameraFollow or PlayerInput.");
            return;
        }

        if (cardTemplate == null)
        {
            Debug.LogWarning("SkillLearnUI is missing cardTemplate reference.");
            return;
        }

        if (backdrop == null || cardContainer == null || cancelButton == null)
        {
            Debug.LogWarning("SkillLearnUI has not initialized its UI elements.");
            return;
        }

        currentTeacher = teacher;

        // Populate the 3 action cards
        PopulateCards(choices);

        isOpen = true;
        Time.timeScale = 0f;

        playerInput.SwitchCurrentActionMap("UI");
        cameraFollow.OnPause();

        backdrop.style.display = DisplayStyle.Flex;
        backdrop.style.visibility = Visibility.Visible;

        cancelButton.Focus();
    }

    private void PopulateCards(List<ActionDefinition> choices)
    {
        cardContainer.Clear();

        foreach (var actionDef in choices)
        {
            if (actionDef == null)
                continue;

            VisualElement cardElement = cardTemplate.Instantiate();
            Button button = cardElement.Q<Button>("ActionButton");
            Label nameLabel = cardElement.Q<Label>("Name");
            Image iconImage = cardElement.Q<Image>("Icon");
            Label descLabel = cardElement.Q<Label>("Description");

            if (nameLabel != null)
            {
                nameLabel.text = actionDef.DisplayName;
            }

            if (iconImage != null)
            {
                iconImage.image = actionDef.Icon != null ? actionDef.Icon.texture : null;
            }

            if (descLabel != null)
            {
                descLabel.text = actionDef.GetDescription();
            }

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
        if (currentTeacher == null || selectedAction == null)
        {
            return;
        }

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
        {
            return;
        }

        backdrop.style.display = DisplayStyle.None;
        backdrop.style.visibility = Visibility.Hidden;
    }

    private void Close()
    {
        Hide();

        if (cardContainer != null)
        {
            cardContainer.Clear();
        }

        currentTeacher = null;
        isOpen = false;

        Time.timeScale = 1f;

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }

        if (cameraFollow != null)
        {
            cameraFollow.OnResume();
        }
    }

    private void OnDestroy()
    {
        if (cancelButton != null)
        {
            cancelButton.clicked -= HandleCancelClicked;
        }

        if (isOpen)
        {
            Time.timeScale = 1f;

            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("Player");
            }

            if (cameraFollow != null)
            {
                cameraFollow.OnResume();
            }
        }
    }
}
