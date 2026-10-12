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
        yesButton = root.Q<Button>("ButtonYes");
        noButton = root.Q<Button>("ButtonNo");
        actionIcon = root.Q<Image>("ActionIcon");
        actionName = root.Q<Label>("ActionName");

        if (yesButton != null)
        {
            yesButton.clicked += HandleYesClicked;
        }

        if (noButton != null)
        {
            noButton.clicked += HandleNoClicked;
        }

        Hide();
    }

    public void Show(TeacherNPC teacher, ActionDefinition action)
    {
        if (isOpen)
        {
            return;
        }

        if (teacher == null || action == null)
        {
            Debug.LogWarning("Cannot show SkillLearnUI without a teacher and action.");
            return;
        }

        if (cameraFollow == null || playerInput == null)
        {
            Debug.LogWarning("SkillLearnUI is missing CameraFollow or PlayerInput.");
            return;
        }

        if (
            backdrop == null
            || yesButton == null
            || noButton == null
            || actionIcon == null
            || actionName == null
        )
        {
            Debug.LogWarning("SkillLearnUI has not initialized its UI elements.");
            return;
        }

        currentTeacher = teacher;

        actionName.text = action.DisplayName;
        actionIcon.image = action.Icon != null ? action.Icon.texture : null;

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
        if (yesButton != null)
        {
            yesButton.clicked -= HandleYesClicked;
        }

        if (noButton != null)
        {
            noButton.clicked -= HandleNoClicked;
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
