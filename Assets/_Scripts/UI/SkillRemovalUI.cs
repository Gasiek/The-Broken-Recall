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

    private readonly List<ActionDefinition> actions = new();

    private bool isOpen;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        backdrop = root.Q<VisualElement>("Backdrop");
        listView = root.Q<ListView>("ListView");
        cancelButton = root.Q<Button>("CancelButton");

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

        if (skillRemoval == null)
        {
            Debug.LogWarning("Cannot show SkillRemovalUI without a SkillRemoval.");
            return;
        }

        if (actionCollection == null)
        {
            Debug.LogWarning("Cannot show SkillRemovalUI without an ActionCollection.");
            return;
        }

        if (cameraFollow == null)
        {
            Debug.LogWarning("SkillRemovalUI has no CameraFollow assigned.");
            return;
        }

        if (playerInput == null)
        {
            Debug.LogWarning("SkillRemovalUI has no PlayerInput assigned.");
            return;
        }

        if (actionListItemTemplate == null)
        {
            Debug.LogWarning("SkillRemovalUI has no action list item template assigned.");
            return;
        }

        currentSkillRemoval = skillRemoval;

        actions.Clear();

        foreach (ActionDefinition action in actionCollection.Actions)
        {
            if (action == null)
            {
                continue;
            }

            actions.Add(action);
        }

        listView.itemsSource = actions;
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

        button.clicked += () =>
        {
            if (button.userData is ActionDefinition action)
            {
                OnActionSelected(action);
            }
        };

        return element;
    }

    private void BindItem(VisualElement element, int index)
    {
        if (index < 0 || index >= actions.Count)
        {
            return;
        }

        ActionDefinition action = actions[index];

        Button button = element.Q<Button>("ActionButton");
        Image icon = element.Q<Image>("Icon");
        Label name = element.Q<Label>("Name");

        button.userData = action;

        name.text = action.DisplayName;

        if (action.Icon != null)
        {
            icon.image = action.Icon.texture;
        }
        else
        {
            icon.image = null;
        }
    }

    private void OnActionSelected(ActionDefinition action)
    {
        if (currentSkillRemoval == null)
        {
            return;
        }

        currentSkillRemoval.RemoveAction(action);

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
        actions.Clear();

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
