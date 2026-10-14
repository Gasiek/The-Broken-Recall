using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SkillRemovalUI : MonoBehaviour
{
    [SerializeField]
    private CameraFollow cameraFollow;

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private VisualTreeAsset actionListItemTemplate;

    private PanelRenderer panelRenderer;
    private VisualElement backdrop;
    private ScrollView scrollView;
    private VisualElement cardContainer;
    private Button cancelButton;

    private SkillRemoval currentSkillRemoval;
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
        scrollView = root.Q<ScrollView>("ScrollView");
        cardContainer = root.Q<VisualElement>("CardContainer");
        cancelButton = root.Q<Button>("CancelButton");

        if (cardContainer == null || cancelButton == null)
        {
            Debug.LogWarning("SkillRemovalUI is missing CardContainer or CancelButton.");
            return;
        }

        cancelButton.clicked += OnCancelPressed;

        Hide();
    }

    public void Show(SkillRemoval skillRemoval, ActionCollection actionCollection)
    {
        if (isOpen)
        {
            return;
        }

        if (skillRemoval == null || actionCollection == null)
        {
            Debug.LogWarning("Cannot show SkillRemovalUI without a SkillRemoval and ActionCollection.");
            return;
        }

        if (cameraFollow == null || playerInput == null)
        {
            Debug.LogWarning("SkillRemovalUI is missing CameraFollow or PlayerInput.");
            return;
        }

        if (actionListItemTemplate == null)
        {
            Debug.LogWarning("SkillRemovalUI has no action list item template assigned.");
            return;
        }

        if (backdrop == null || cardContainer == null)
        {
            Debug.LogWarning("SkillRemovalUI has not initialized its UI elements.");
            return;
        }

        currentSkillRemoval = skillRemoval;

        // Populate cards dynamically into the wrapping grid container
        PopulateCards(actionCollection.Actions);

        isOpen = true;
        Time.timeScale = 0f;

        playerInput.SwitchCurrentActionMap("UI");
        cameraFollow.OnPause();

        backdrop.style.display = DisplayStyle.Flex;
        backdrop.style.visibility = Visibility.Visible;

        scrollView?.Focus();
    }

    private void PopulateCards(IEnumerable<ActionCardInstance> actions)
    {
        cardContainer.Clear();

        foreach (ActionCardInstance card in actions)
        {
            if (card == null || card.Definition == null)
            {
                continue;
            }

            // Instantiate the card UXML template
            VisualElement itemElement = actionListItemTemplate.Instantiate();
            Button button = itemElement.Q<Button>("ActionButton");
            Label nameLabel = itemElement.Q<Label>("Name");
            Image iconImage = itemElement.Q<Image>("Icon");
            Label descLabel = itemElement.Q<Label>("Description");

            ActionDefinition definition = card.Definition;

            // Set Display Name
            if (nameLabel != null)
            {
                nameLabel.text = definition.DisplayName;
            }

            // Set Icon
            if (iconImage != null)
            {
                iconImage.image = definition.Icon != null ? definition.Icon.texture : null;
            }

            // Set Description (Adjust if GetDescription() is on definition instead of card)
            if (descLabel != null)
            {
                descLabel.text = definition.GetDescription(); 
            }

            // Wire up click event to remove this specific card
            if (button != null)
            {
                ActionCardInstance targetCard = card;
                button.clicked += () => OnActionSelected(targetCard);
            }

            cardContainer.Add(itemElement);
        }
    }

    private void OnActionSelected(ActionCardInstance card)
    {
        if (currentSkillRemoval == null || card == null)
        {
            return;
        }

        currentSkillRemoval.RemoveAction(card);

        Close();
    }

    private void OnCancelPressed()
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

        currentSkillRemoval = null;
        if (cardContainer != null)
        {
            cardContainer.Clear();
        }

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
            cancelButton.clicked -= OnCancelPressed;
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