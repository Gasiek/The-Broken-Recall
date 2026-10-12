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
    private ListView listView;
    private Button cancelButton;

    private SkillRemoval currentSkillRemoval;

    private readonly List<ActionCardInstance> cards = new();

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
        listView = root.Q<ListView>("ListView");
        cancelButton = root.Q<Button>("CancelButton");

        if (listView == null || cancelButton == null)
        {
            Debug.LogWarning("SkillRemovalUI is missing ListView or CancelButton.");
            return;
        }

        SetupListView();

        cancelButton.clicked += OnCancelPressed;

        Hide();
    }

    private void SetupListView()
    {
        listView.makeItem = MakeItem;
        listView.bindItem = BindItem;
        listView.selectionType = SelectionType.None;
    }

    public void Show(SkillRemoval skillRemoval, ActionCollection actionCollection)
    {
        if (isOpen)
        {
            return;
        }

        if (skillRemoval == null || actionCollection == null)
        {
            Debug.LogWarning(
                "Cannot show SkillRemovalUI without a SkillRemoval " + "and ActionCollection."
            );
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

        if (backdrop == null || listView == null)
        {
            Debug.LogWarning("SkillRemovalUI has not initialized its UI elements.");
            return;
        }

        currentSkillRemoval = skillRemoval;

        cards.Clear();

        foreach (ActionCardInstance card in actionCollection.Actions)
        {
            if (card == null || card.Definition == null)
            {
                continue;
            }

            cards.Add(card);
        }

        listView.itemsSource = cards;
        listView.Rebuild();

        isOpen = true;
        Time.timeScale = 0f;

        playerInput.SwitchCurrentActionMap("UI");
        cameraFollow.OnPause();

        backdrop.style.display = DisplayStyle.Flex;
        backdrop.style.visibility = Visibility.Visible;

        listView.Focus();
    }

    private VisualElement MakeItem()
    {
        VisualElement element = actionListItemTemplate.Instantiate();

        Button button = element.Q<Button>("ActionButton");

        if (button == null)
        {
            Debug.LogWarning("Action list item template has no ActionButton.");
            return element;
        }

        // Register once per created visual element.
        // BindItem updates userData when ListView recycles the row.
        button.clicked += () =>
        {
            if (button.userData is ActionCardInstance card)
            {
                OnActionSelected(card);
            }
        };

        return element;
    }

    private void BindItem(VisualElement element, int index)
    {
        Button button = element.Q<Button>("ActionButton");
        Image icon = element.Q<Image>("Icon");
        Label name = element.Q<Label>("Name");

        if (button == null)
        {
            return;
        }

        if (index < 0 || index >= cards.Count)
        {
            button.userData = null;
            button.SetEnabled(false);

            if (name != null)
            {
                name.text = string.Empty;
            }

            if (icon != null)
            {
                icon.image = null;
            }

            return;
        }

        ActionCardInstance card = cards[index];
        ActionDefinition definition = card?.Definition;

        if (definition == null)
        {
            button.userData = null;
            button.SetEnabled(false);

            if (name != null)
            {
                name.text = string.Empty;
            }

            if (icon != null)
            {
                icon.image = null;
            }

            return;
        }

        button.userData = card;
        button.SetEnabled(true);

        if (name != null)
        {
            name.text = definition.DisplayName;
        }

        if (icon != null)
        {
            icon.image = definition.Icon != null ? definition.Icon.texture : null;
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
        cards.Clear();

        if (listView != null)
        {
            listView.itemsSource = cards;
            listView.Rebuild();
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
