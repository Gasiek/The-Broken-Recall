using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }

    [SerializeField]
    private PauseMenuUI pauseMenuUI;

    public bool IsPaused { get; private set; }

    private int lastToggleFrame = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        TogglePause();
    }

    private void TogglePause()
    {
        if (Time.frameCount == lastToggleFrame)
        {
            return;
        }
        lastToggleFrame = Time.frameCount;

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

        Debug.Log("[PauseController] Game Paused!");
        IsPaused = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseMenuUI.Show();
    }

    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        Debug.Log("[PauseController] Game Resumed!");
        IsPaused = false;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseMenuUI.Hide();
    }

    public void Quit()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
