using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField]
    private PauseController pauseController;

    [SerializeField]
    private CardsMenuUI cardsMenuUI;

    private PanelRenderer panelRenderer;

    private VisualElement pauseMenu;
    private VisualElement mainMenuContainer;

    private Button resumeButton;
    private Button cardsButton;
    private Button quitButton;

    public event System.Action CardsClicked;

    private void Awake()
    {
        EnsureCardsMenuUI();
    }

    private void OnEnable()
    {
        EnsureCardsMenuUI();

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

    private void EnsureCardsMenuUI()
    {
        if (cardsMenuUI == null)
        {
            cardsMenuUI = GetComponent<CardsMenuUI>();
            if (cardsMenuUI == null)
            {
                cardsMenuUI = gameObject.AddComponent<CardsMenuUI>();
            }
        }
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        pauseMenu = root.Q<VisualElement>("PauseMenu");
        mainMenuContainer = root.Q<VisualElement>("Buttons");

        resumeButton = root.Q<Button>("ResumeButton");
        cardsButton = root.Q<Button>("CardsButton");
        quitButton = root.Q<Button>("QuitButton");

        if (resumeButton != null) resumeButton.clicked += HandleResumeClicked;
        if (cardsButton != null) cardsButton.clicked += HandleCardsClicked;
        if (quitButton != null) quitButton.clicked += HandleQuitClicked;

        EnsureCardsMenuUI();
        if (cardsMenuUI != null)
        {
            cardsMenuUI.Initialize(root, HandleCardsBackClicked);
        }

        Hide();
    }

    public void Show()
    {
        if (pauseMenu != null)
        {
            pauseMenu.style.display = DisplayStyle.Flex;
            pauseMenu.style.visibility = Visibility.Visible;
        }

        ShowMainMenu();

        if (resumeButton != null)
        {
            resumeButton.Focus();
        }
    }

    public void Hide()
    {
        if (cardsMenuUI != null)
        {
            cardsMenuUI.Hide();
        }

        if (pauseMenu != null)
        {
            pauseMenu.style.display = DisplayStyle.None;
            pauseMenu.style.visibility = Visibility.Hidden;
        }

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (cardsMenuUI != null)
        {
            cardsMenuUI.Hide();
        }

        if (mainMenuContainer != null)
        {
            mainMenuContainer.style.display = DisplayStyle.Flex;
        }
    }

    public void ShowCardsMenu()
    {
        if (mainMenuContainer != null)
        {
            mainMenuContainer.style.display = DisplayStyle.None;
        }

        if (cardsMenuUI != null)
        {
            cardsMenuUI.Show();
        }
    }

    private void HandleResumeClicked()
    {
        pauseController.Resume();
    }

    private void HandleCardsClicked()
    {
        ShowCardsMenu();
        CardsClicked?.Invoke();
    }

    private void HandleCardsBackClicked()
    {
        ShowMainMenu();
        if (cardsButton != null)
        {
            cardsButton.Focus();
        }
    }

    private void HandleQuitClicked()
    {
        pauseController.Quit();
    }

    private void OnDestroy()
    {
        if (resumeButton != null) resumeButton.clicked -= HandleResumeClicked;
        if (cardsButton != null) cardsButton.clicked -= HandleCardsClicked;
        if (quitButton != null) quitButton.clicked -= HandleQuitClicked;
    }
}
