using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [SerializeField]
    private PauseMenuUI pauseMenuUI;

    [SerializeField]
    private CameraFollow cameraFollow;
    [SerializeField]
    private PlayerInput playerInput;

    public bool IsPaused { get; private set; }


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

        playerInput.SwitchCurrentActionMap("UI");

        cameraFollow.OnPause();

        pauseMenuUI.Show();
    }

    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;

        Time.timeScale = 1f;

        playerInput.SwitchCurrentActionMap("Player");

        cameraFollow.OnResume();

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
