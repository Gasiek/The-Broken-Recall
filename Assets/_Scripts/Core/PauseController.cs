using System;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField]
    private PauseMenuUI pauseMenuUI;

    public bool IsPaused { get; private set; }
    public bool IsModalOpen => modalOpen;
    public PauseMenuUI PauseMenuUI => pauseMenuUI;

    private InputManager inputManager;
    private bool modalOpen;
    private Action modalCancelAction;

    private void Awake()
    {
        if (pauseMenuUI == null)
            pauseMenuUI = GetComponent<PauseMenuUI>();

        RefreshInputManager();
    }

    public void BindPlayer()
    {
        RefreshInputManager();
        SubscribeToCancel();
    }

    public void UnbindPlayer()
    {
        CloseModal();

        if (IsPaused)
            Resume();

        UnsubscribeFromCancel();
        inputManager = null;
    }

    private void RefreshInputManager()
    {
        InputManager currentManager = InputManager.Instance;

        if (inputManager == currentManager)
            return;

        UnsubscribeFromCancel();
        inputManager = currentManager;
        SubscribeToCancel();
    }

    private void SubscribeToCancel()
    {
        if (inputManager != null)
            inputManager.CancelRequested -= HandleCancel;

        if (inputManager != null && isActiveAndEnabled)
            inputManager.CancelRequested += HandleCancel;
    }

    private void UnsubscribeFromCancel()
    {
        if (inputManager != null)
            inputManager.CancelRequested -= HandleCancel;
    }

    public void TogglePause()
    {
        if (modalOpen)
            return;

        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (IsPaused || modalOpen)
            return;

        RefreshInputManager();

        if (inputManager == null)
        {
            Debug.LogError("[PauseController] Cannot pause: InputManager is missing.", this);
            return;
        }

        IsPaused = true;
        Time.timeScale = 0f;

        inputManager.EnableUIMap();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseMenuUI?.Show();
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;

        pauseMenuUI?.Hide();

        RestoreGameplayState();
    }

    /// <summary>
    /// Opens a modal from gameplay. The modal supplies its own cancel callback.
    /// A modal cannot take over an already-open pause menu.
    /// </summary>
    public bool OpenModal(Action cancelAction = null)
    {
        if (modalOpen || IsPaused)
            return false;

        RefreshInputManager();

        if (inputManager == null)
        {
            Debug.LogError("[PauseController] Cannot open modal: InputManager is missing.", this);
            return false;
        }

        modalOpen = true;
        modalCancelAction = cancelAction;

        Time.timeScale = 0f;
        inputManager.EnableUIMap();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        return true;
    }

    /// <summary>
    /// Closes the current gameplay modal and restores gameplay input.
    /// </summary>
    public void CloseModal()
    {
        if (!modalOpen)
            return;

        modalOpen = false;
        modalCancelAction = null;

        RestoreGameplayState();
    }

    private void RestoreGameplayState()
    {
        Time.timeScale = 1f;

        RefreshInputManager();

        if (inputManager != null)
            inputManager.EnablePlayerMap();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void HandleCancel()
    {
        if (modalOpen)
        {
            Action cancelAction = modalCancelAction;
            cancelAction?.Invoke();
            return;
        }

        if (!IsPaused)
            return;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.HandleCancel();
            return;
        }

        Resume();
    }

    public void Quit()
    {
        modalOpen = false;
        modalCancelAction = null;
        IsPaused = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnEnable()
    {
        RefreshInputManager();
        SubscribeToCancel();
    }

    private void OnDisable()
    {
        UnsubscribeFromCancel();

        if (modalOpen)
        {
            modalOpen = false;
            modalCancelAction = null;
            Time.timeScale = 1f;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromCancel();

        if (IsPaused || modalOpen)
        {
            IsPaused = false;
            modalOpen = false;
            modalCancelAction = null;

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
