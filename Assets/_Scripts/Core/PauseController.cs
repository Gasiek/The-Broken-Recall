using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [SerializeField]
    private PauseMenuUI pauseMenuUI;

    private PlayerInput playerInput;

    public bool IsPaused { get; private set; }
    public PauseMenuUI PauseMenuUI => pauseMenuUI;

    public void BindPlayer(PlayerInput input)
    {
        if (input == null)
        {
            Debug.LogError("PauseController cannot bind: PlayerInput is null.", this);
            return;
        }

        if (playerInput != null && playerInput != input)
        {
            UnbindPlayer();
        }

        playerInput = input;
    }

    public void UnbindPlayer(PlayerInput input)
    {
        if (playerInput != input)
        {
            return;
        }

        UnbindPlayer();
    }

    public void UnbindPlayer()
    {
        if (IsPaused)
        {
            Resume();
        }

        playerInput = null;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (IsPaused)
        {
            return;
        }

        IsPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }

        pauseMenuUI?.Show();
    }

    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;
        Time.timeScale = 1f;

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseMenuUI?.Hide();
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (IsPaused)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
