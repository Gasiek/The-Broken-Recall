using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SkillLearnUI : MonoBehaviour
{
    [SerializeField]
    private CameraFollow cameraFollow;

    [SerializeField]
    private PlayerInput playerInput;

    private PanelRenderer panelRenderer;

    private VisualElement backdrop;
    private Button yesButton;
    private Button noButton;
    private Image actionIcon;
    private Label actionName;

    private TeacherNPC currentTeacher;

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
        yesButton = root.Q<Button>("ButtonYes");
        noButton = root.Q<Button>("ButtonNo");
        actionIcon = root.Q<Image>("ActionIcon");
        actionName = root.Q<Label>("ActionName");

        yesButton.clicked += HandleYesClicked;
        noButton.clicked += HandleNoClicked;

        Hide();
    }

    public void Show(TeacherNPC teacher, ActionDefinition action)
    {
        if (isOpen)
        {
            return;
        }

        if (teacher == null)
        {
            Debug.LogWarning("Cannot show SkillLearnUI without a teacher.");
            return;
        }

        if (action == null)
        {
            Debug.LogWarning("Cannot show SkillLearnUI without an action.");
            return;
        }

        if (cameraFollow == null)
        {
            Debug.LogWarning("SkillLearnUI has no CameraFollow assigned.");
            return;
        }

        if (playerInput == null)
        {
            Debug.LogWarning("SkillLearnUI has no PlayerInput assigned.");
            return;
        }

        currentTeacher = teacher;

        actionName.text = action.DisplayName;

        if (action.Icon != null)
        {
            actionIcon.image = action.Icon.texture;
        }
        else
        {
            actionIcon.image = null;
        }

        isOpen = true;

        Time.timeScale = 0f;
        playerInput.SwitchCurrentActionMap("UI");
        cameraFollow.OnPause();

        backdrop.style.display = DisplayStyle.Flex;
        backdrop.style.visibility = Visibility.Visible;

        yesButton.Focus();
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

    private void HandleYesClicked()
    {
        if (currentTeacher == null)
        {
            return;
        }

        currentTeacher.LearnAction();

        Close();
    }

    private void HandleNoClicked()
    {
        Close();
    }

    private void Close()
    {
        Hide();

        currentTeacher = null;
        isOpen = false;

        Time.timeScale = 1f;
        playerInput.SwitchCurrentActionMap("Player");
        cameraFollow.OnResume();
    }

    private void OnDestroy()
    {
        if (yesButton != null)
        {
            yesButton.clicked -= HandleYesClicked;
        }

        if (noButton != null)
        {
            noButton.clicked -= HandleNoClicked;
        }

        // Make sure the game doesn't remain paused
        // if this UI object is destroyed while the popup is open.
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
