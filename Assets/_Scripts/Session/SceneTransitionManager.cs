using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public sealed class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField]
    private GameObject playerPrefab;

    [Header("Initial Spawn")]
    [SerializeField]
    private SceneDestination initialDestination;

    private GameSession session;
    private PlayerSceneBinder playerSceneBinder;
    private PlayerSpawner playerSpawner;

    private bool loading;
    private bool initialSpawnStarted;

    private SceneDestination pendingDestination;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        session = GetComponent<GameSession>();

        if (session == null)
            session = GameSession.Instance;

        playerSceneBinder = new PlayerSceneBinder();
        playerSpawner = new PlayerSpawner(playerPrefab, playerSceneBinder);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private IEnumerator Start()
    {
        // Allow scene objects to finish Awake and OnEnable.
        yield return null;

        if (initialSpawnStarted)
            yield break;

        if (session == null)
            session = GameSession.Instance;

        // Do not permanently consume the initial-spawn attempt if
        // the session has not been initialized yet.
        if (session == null)
        {
            Debug.LogError("[SceneTransitionManager] No GameSession exists.", this);
            yield break;
        }

        initialSpawnStarted = true;

        // Do not create a duplicate player.
        if (session.CurrentPlayer != null)
            yield break;

        if (!ValidatePlayerPrefab())
            yield break;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneContext context = GetSceneContext(activeScene);

        if (context == null)
        {
            Debug.LogError($"No root SceneContext found in '{activeScene.name}'.", this);
            yield break;
        }

        if (!context.ValidateConfiguration())
        {
            Debug.LogError($"SceneContext configuration is invalid in '{activeScene.name}'.", this);
            yield break;
        }

        SceneSpawnPoint spawn = null;

        if (initialDestination != null)
        {
            if (initialDestination.SceneName != activeScene.name)
            {
                Debug.LogError(
                    $"Initial destination '{initialDestination.name}' points "
                        + $"to scene '{initialDestination.SceneName}', but the active "
                        + $"scene is '{activeScene.name}'.",
                    this
                );
                yield break;
            }

            spawn = context.FindSpawnPoint(initialDestination);

            if (spawn == null)
            {
                Debug.LogError(
                    $"No spawn point in '{activeScene.name}' matches "
                        + $"initial destination '{initialDestination.name}'.",
                    this
                );
                yield break;
            }
        }
        else
        {
            spawn = context.GetDefaultSpawnPoint();
        }

        if (spawn == null)
        {
            Debug.LogError(
                $"No valid initial spawn point found in '{activeScene.name}'. "
                    + "Assign an Initial Destination or configure a default spawn.",
                this
            );
            yield break;
        }

        SpawnPlayer(context, spawn, session.PlayerData);
    }

    public bool TransitionTo(SceneDestination destination)
    {
        if (destination == null)
        {
            Debug.LogError("[SceneTransitionManager] Destination asset is null.", this);
            return false;
        }

        if (loading)
            return false;

        if (session == null)
            session = GameSession.Instance;

        if (session == null)
        {
            Debug.LogError("[SceneTransitionManager] No GameSession exists.", this);
            return false;
        }

        if (!ValidatePlayerPrefab())
            return false;

        string sceneName = destination.SceneName;

        if (
            string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName)
        )
        {
            Debug.LogError(
                $"Destination scene '{sceneName}' is empty or is not "
                    + "included in Build Settings.",
                this
            );
            return false;
        }

        if (session.TransitionInProgress)
        {
            Debug.LogWarning("[SceneTransitionManager] A transition is already in progress.", this);
            return false;
        }

        pendingDestination = destination;

        StartCoroutine(TransitionRoutine(destination));

        return true;
    }

    private IEnumerator TransitionRoutine(SceneDestination destination)
    {
        if (session == null || !session.BeginTransition())
        {
            loading = false;
            pendingDestination = null;
            yield break;
        }

        loading = true;

        Player oldPlayer = session.CurrentPlayer;

        // Capture state and detach the outgoing player before destroying it.
        if (oldPlayer != null)
        {
            ActionController controller = oldPlayer.GetComponent<ActionController>();

            if (controller != null)
                controller.CancelCurrentAction();

            if (oldPlayer.WeaponBuffs != null)
                oldPlayer.WeaponBuffs.PauseBuffTimer();

            session.SetPlayerData(oldPlayer.CaptureState());
            session.UnregisterPlayer(oldPlayer);

            playerSceneBinder.Unbind();

            Destroy(oldPlayer.gameObject);
        }

        session.SetDestination(destination.SceneName, destination.name);

        // Prevent a paused game from freezing scene loading.
        Time.timeScale = 1f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            destination.SceneName,
            LoadSceneMode.Single
        );

        if (operation == null)
        {
            AbortTransition("Unity could not start loading the destination scene.");
            yield break;
        }

        while (!operation.isDone)
            yield return null;

        // OnSceneLoaded initializes the player.
        yield return null;

        if (loading)
        {
            AbortTransition("The destination scene loaded, but player setup did not complete.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!loading || session == null)
            return;

        if (pendingDestination == null)
        {
            AbortTransition("No destination was set for the scene transition.");
            return;
        }

        if (pendingDestination.SceneName != scene.name)
        {
            AbortTransition(
                $"Destination '{pendingDestination.name}' targets scene "
                    + $"'{pendingDestination.SceneName}', but '{scene.name}' was loaded."
            );
            return;
        }

        SceneContext context = GetSceneContext(scene);

        if (context == null)
        {
            AbortTransition($"No root SceneContext found in '{scene.name}'.");
            return;
        }
        if (!context.ValidateConfiguration())
        {
            AbortTransition($"SceneContext configuration is invalid in '{scene.name}'.");
            return;
        }

        SceneSpawnPoint spawn = context.FindSpawnPoint(pendingDestination);

        if (spawn == null)
        {
            AbortTransition(
                $"No spawn point in '{scene.name}' uses destination "
                    + $"'{pendingDestination.name}'. Check SceneDestination assignments."
            );
            return;
        }

        SpawnPlayer(context, spawn, session.PlayerData);
    }

    private void SpawnPlayer(SceneContext context, SceneSpawnPoint spawn, PlayerSaveData saveData)
    {
        if (context == null || spawn == null)
        {
            AbortTransition(
                "Cannot spawn the player without a valid scene context and spawn point."
            );
            return;
        }

        if (!ValidatePlayerPrefab())
        {
            AbortTransition("The player prefab is invalid.");
            return;
        }

        Player player = playerSpawner.TrySpawn(context, spawn, saveData);

        if (player == null)
        {
            AbortTransition(
                "Player setup failed. Check the player prefab, scene camera, "
                    + "PlayerInput, Look action, and scene UI references."
            );
            return;
        }

        // Preserve the existing GameSession registration contract.
        if (session == null || !session.CompletePlayerSpawn(player))
        {
            playerSceneBinder.Unbind();
            Destroy(player.gameObject);

            AbortTransition("GameSession rejected the newly spawned player.");
            return;
        }

        pendingDestination = null;
        loading = false;
    }

    private bool ValidatePlayerPrefab()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[SceneTransitionManager] Assign Player Prefab.", this);
            return false;
        }

        if (playerPrefab.GetComponent<Player>() == null)
        {
            Debug.LogError("[SceneTransitionManager] Player prefab has no Player component.", this);
            return false;
        }

        var playerInput = playerPrefab.GetComponent<UnityEngine.InputSystem.PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError(
                "[SceneTransitionManager] Player prefab has no PlayerInput component.",
                this
            );
            return false;
        }

        if (playerInput.actions == null || playerInput.actions.FindAction("Look", false) == null)
        {
            Debug.LogError(
                "[SceneTransitionManager] Player prefab must have an "
                    + "Input Actions asset containing a 'Look' action.",
                this
            );
            return false;
        }

        return true;
    }

    private SceneContext GetSceneContext(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            SceneContext context = root.GetComponent<SceneContext>();

            if (context != null)
                return context;
        }

        return null;
    }

    private void AbortTransition(string reason)
    {
        Debug.LogError($"[SceneTransitionManager] {reason}", this);

        playerSceneBinder?.Unbind();

        loading = false;
        pendingDestination = null;

        if (session != null)
            session.CancelTransition();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        playerSceneBinder?.Unbind();

        SceneManager.sceneLoaded -= OnSceneLoaded;

        Instance = null;
    }
}
