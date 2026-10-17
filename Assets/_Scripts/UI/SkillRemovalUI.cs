using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class SkillRemovalUI : MonoBehaviour
{
    [SerializeField]
    private VisualTreeAsset actionListItemTemplate;

    private PanelRenderer panelRenderer;
    private CameraFollow cameraFollow;
    private PlayerInput playerInput;

    private VisualElement backdrop;
    private ScrollView scrollView;
    private VisualElement cardContainer;
    private Button cancelButton;

    private SkillRemoval currentSkillRemoval;
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
            cancelButton.clicked -= OnCancelPressed;

        backdrop = null;
        scrollView = null;
        cardContainer = null;
        cancelButton = null;
    }

    /// <summary>
    /// Called by SceneContext after the current player and scene camera
    /// have been connected.
    /// </summary>
    public void BindRuntimeReferences(
        CameraFollow sceneCameraFollow,
        PlayerInput currentPlayerInput
    )
    {
        cameraFollow = sceneCameraFollow;
        playerInput = currentPlayerInput;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        if (isOpen)
            Close();

        if (cancelButton != null)
            cancelButton.clicked -= OnCancelPressed;

        backdrop = root.Q<VisualElement>("Backdrop");
        scrollView = root.Q<ScrollView>("ScrollView");
        cardContainer = root.Q<VisualElement>("CardContainer");
        cancelButton = root.Q<Button>("CancelButton");

        if (backdrop == null || cardContainer == null || cancelButton == null)
        {
            Debug.LogError(
                "[SkillRemovalUI] UXML must contain Backdrop, CardContainer, "
                    + "and CancelButton.",
                this
            );

            Hide();
            return;
        }

        cancelButton.clicked += OnCancelPressed;
        Hide();
    }

    public void Show(SkillRemoval skillRemoval, ActionCollection actionCollection)
    {
        if (isOpen)
            return;

        if (skillRemoval == null || actionCollection == null)
        {
            Debug.LogWarning(
                "[SkillRemovalUI] A SkillRemoval and ActionCollection are required.",
                this
            );
            return;
        }

        if (cameraFollow == null || playerInput == null)
        {
            Debug.LogWarning(
                "[SkillRemovalUI] Runtime references have not been bound. "
                    + "Check SceneContext and SceneTransitionManager setup.",
                this
            );
            return;
        }

        if (actionListItemTemplate == null)
        {
            Debug.LogWarning("[SkillRemovalUI] Action list item template is not assigned.", this);
            return;
        }

        if (backdrop == null || cardContainer == null || cancelButton == null)
        {
            Debug.LogWarning("[SkillRemovalUI] UI elements have not initialized.", this);
            return;
        }

        currentSkillRemoval = skillRemoval;
        PopulateCards(actionCollection.Actions);

        previousTimeScale = Time.timeScale;
        previousActionMap =
            playerInput.currentActionMap != null ? playerInput.currentActionMap.name : null;

        isOpen = true;
        Time.timeScale = 0f;

        playerInput.SwitchCurrentActionMap("UI");
        cameraFollow.OnPause();
        cameraPausedByThisPanel = true;

        backdrop.style.display = DisplayStyle.Flex;
        backdrop.style.visibility = Visibility.Visible;

        (scrollView as VisualElement ?? cardContainer).Focus();
    }

    private void PopulateCards(IEnumerable<ActionCardInstance> actions)
    {
        cardContainer.Clear();

        if (actions == null)
            return;

        foreach (ActionCardInstance card in actions)
        {
            if (card == null || card.Definition == null)
                continue;

            VisualElement itemElement = actionListItemTemplate.Instantiate();

            Button button = itemElement.Q<Button>("ActionButton");
            Label nameLabel = itemElement.Q<Label>("Name");
            Image iconImage = itemElement.Q<Image>("Icon");
            Label descLabel = itemElement.Q<Label>("Description");

            ActionDefinition definition = card.Definition;

            if (nameLabel != null)
                nameLabel.text = definition.DisplayName;

            if (iconImage != null)
            {
                iconImage.image = definition.Icon != null ? definition.Icon.texture : null;
            }

            if (descLabel != null)
                descLabel.text = definition.GetDescription();

            if (button != null)
            {
                ActionCardInstance selectedCard = card;
                button.clicked += () => OnActionSelected(selectedCard);
            }

            cardContainer.Add(itemElement);
        }
    }

    private void OnActionSelected(ActionCardInstance card)
    {
        if (!isOpen || currentSkillRemoval == null || card == null)
            return;

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
            return;

        backdrop.style.display = DisplayStyle.None;
        backdrop.style.visibility = Visibility.Hidden;
    }

    private void Close()
    {
        Hide();

        if (cardContainer != null)
            cardContainer.Clear();

        currentSkillRemoval = null;
        isOpen = false;

        ReleasePause();
    }

    private void ReleasePause()
    {
        // Don't alter global pause or input state if this panel never opened.
        if (!isOpen && !cameraPausedByThisPanel && previousActionMap == null)
            return;

        Time.timeScale = previousTimeScale;

        if (playerInput != null && !string.IsNullOrEmpty(previousActionMap))
            playerInput.SwitchCurrentActionMap(previousActionMap);

        if (cameraPausedByThisPanel && cameraFollow != null)
            cameraFollow.OnResume();

        cameraPausedByThisPanel = false;
        previousActionMap = null;
    }
}
