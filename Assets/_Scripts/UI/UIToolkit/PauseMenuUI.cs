using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(CardsMenuUI))]
[RequireComponent(typeof(PauseController))]
public class PauseMenuUI : MonoBehaviour
{
    private PauseController pauseController;
    private CardsMenuUI cardsMenuUI;
    private PanelRenderer panelRenderer;

    private VisualElement pauseMenu;
    private VisualElement mainMenuContainer;

    private Button resumeButton;
    private Button cardsButton;
    private Button quitButton;

    public event Action CardsClicked;

    public void ScheduleCancel()
    {
        if (pauseMenu == null)
            return;

        pauseMenu.schedule.Execute(HandleCancel);
    }

    public void HandleCancel()
    {
        if (cardsMenuUI != null && cardsMenuUI.IsVisible)
        {
            ShowMainMenu();
            return;
        }

        if (pauseController != null && pauseController.IsPaused)
        {
            pauseController.Resume();
        }
    }

    private void Awake()
    {
        pauseController = GetComponent<PauseController>();
        cardsMenuUI = GetComponent<CardsMenuUI>();
    }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();

        if (panelRenderer != null)
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }
        else
        {
            Debug.LogError("PauseMenuUI requires a PanelRenderer on the same GameObject.", this);
        }
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        UnbindUI();
        cardsMenuUI?.Hide();
    }

    private void OnDestroy()
    {
        UnbindUI();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        UnbindUI();

        if (root == null)
            return;

        pauseMenu = root.Q<VisualElement>("PauseMenu");
        mainMenuContainer = root.Q<VisualElement>("Buttons");

        resumeButton = root.Q<Button>("ResumeButton");
        cardsButton = root.Q<Button>("CardsButton");
        quitButton = root.Q<Button>("QuitButton");

        if (resumeButton != null)
            resumeButton.clicked += HandleResumeClicked;

        if (cardsButton != null)
            cardsButton.clicked += HandleCardsClicked;

        if (quitButton != null)
            quitButton.clicked += HandleQuitClicked;

        cardsMenuUI.Initialize(root, HandleCardsBackClicked);

        Hide();
    }

    private void UnbindUI()
    {
        if (resumeButton != null)
            resumeButton.clicked -= HandleResumeClicked;

        if (cardsButton != null)
            cardsButton.clicked -= HandleCardsClicked;

        if (quitButton != null)
            quitButton.clicked -= HandleQuitClicked;

        resumeButton = null;
        cardsButton = null;
        quitButton = null;
        pauseMenu = null;
        mainMenuContainer = null;
    }

    public void Show()
    {
        if (pauseMenu != null)
        {
            pauseMenu.style.display = DisplayStyle.Flex;
            pauseMenu.style.visibility = Visibility.Visible;
        }

        ShowMainMenu();
        resumeButton?.Focus();
    }

    public void Hide()
    {
        cardsMenuUI?.Hide();

        if (pauseMenu != null)
            pauseMenu.style.display = DisplayStyle.None;

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        cardsMenuUI?.Hide();

        if (mainMenuContainer != null)
            mainMenuContainer.style.display = DisplayStyle.Flex;
    }

    public void ShowCardsMenu()
    {
        if (mainMenuContainer != null)
            mainMenuContainer.style.display = DisplayStyle.None;

        cardsMenuUI?.Show();
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
        cardsButton?.Focus();
    }

    private void HandleQuitClicked()
    {
        pauseController.Quit();
    }
}
