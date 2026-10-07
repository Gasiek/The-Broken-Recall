using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField]
    private PauseController pauseController;
    private PanelRenderer panelRenderer;
    private Button resumeButton;
    private Button quitButton;
    private VisualElement pauseMenu;


    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        resumeButton = root.Q<Button>("ResumeButton");
        quitButton = root.Q<Button>("QuitButton");
        pauseMenu = root.Q<VisualElement>("PauseMenu");

        resumeButton.clicked += HandleResumeClicked;
        quitButton.clicked += HandleQuitClicked;

        Hide();
    }

    public void Show()
    {
        pauseMenu.visible = true;
        resumeButton.Focus();
    }

    public void Hide()
    {
        pauseMenu.visible = false;
    }

    private void HandleResumeClicked()
    {
        pauseController.Resume();
    }

    private void HandleQuitClicked()
    {
        pauseController.Quit();
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
        {
            resumeButton.clicked -= HandleResumeClicked;
        }

        if (quitButton != null)
        {
            quitButton.clicked -= HandleQuitClicked;
        }
    }
}
