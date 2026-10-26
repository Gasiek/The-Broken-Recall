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
    private InputManager inputManager;
    private PlayerSceneBinder playerSceneBinder;
    private PlayerSpawner playerSpawner;

    private bool initialized;
    private bool loading;

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

        // Keep the input owner on this persistent GameObject.
        inputManager = GetComponent<InputManager>();

        if (inputManager == null)
            inputManager = gameObject.AddComponent<InputManager>();

        if (!ValidatePlayerPrefab())
            return;

        playerSceneBinder = new PlayerSceneBinder();
        playerSpawner = new PlayerSpawner(playerPrefab, playerSceneBinder);

        SceneManager.sceneLoaded += OnSceneLoaded;
        initialized = true;
    }

    private IEnumerator Start()
    {
        if (!initialized)
            yield break;

        // Allow scene objects to finish Awake and OnEnable.
        yield return null;

        while (session == null)
        {
            session = GameSession.Instance;

            if (session == null)
                yield return null;
        }

        if (session.CurrentPlayer != null)
            yield break;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneContext context = GetSceneContext(activeScene);

        if (context == null)
        {
            Debug.LogError($"No root SceneContext found in '{activeScene.name}'.", this);
            yield break;
        }

        if (initialDestination != null && initialDestination.SceneName != activeScene.name)
        {
            Debug.LogError(
                $"Initial destination '{initialDestination.name}' targets "
                    + $"'{initialDestination.SceneName}', but the active scene is "
                    + $"'{activeScene.name}'.",
                this
            );
            yield break;
        }

        if (!TryGetSpawnPoint(context, initialDestination, out SceneSpawnPoint spawn))
        {
            Debug.LogError(
                initialDestination != null
                    ? $"Could not find a valid spawn point for initial destination "
                        + $"'{initialDestination.name}' in '{activeScene.name}'."
                    : $"No default spawn point found in '{activeScene.name}'. "
                        + "Assign an Initial Destination or configure a default spawn.",
                this
            );
            yield break;
        }

        SpawnPlayer(context, spawn, session.PlayerData);
    }

    public void RestartFromBeginning()
    {
        if (!initialized || loading)
            return;

        if (session == null)
            session = GameSession.Instance;

        if (session == null)
        {
            Debug.LogError("Cannot restart: no GameSession exists.", this);
            return;
        }

        if (initialDestination == null)
        {
            Debug.LogError("Cannot restart: assign an Initial Destination.", this);
            return;
        }

        StartCoroutine(RestartRoutine());
    }

    public bool TransitionTo(SceneDestination destination)
    {
        if (!initialized || destination == null || loading)
        {
            if (destination == null)
                Debug.LogError("Cannot transition: destination asset is null.", this);

            return false;
        }

        if (session == null)
            session = GameSession.Instance;

        if (session == null)
        {
            Debug.LogError("Cannot transition: no GameSession exists.", this);
            return false;
        }

        if (session.TransitionInProgress)
            return false;

        string sceneName = destination.SceneName;

        if (
            string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName)
        )
        {
            Debug.LogError(
                $"Destination scene '{sceneName}' is empty or isn't included "
                    + "in Build Settings.",
                this
            );
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
            pendingDestination = null;
            yield break;
        }

        loading = true;

        Player oldPlayer = session.CurrentPlayer;

        if (oldPlayer != null)
        {
            ActionController controller = oldPlayer.GetComponent<ActionController>();

            if (controller != null)
                controller.CancelCurrentAction();

            session.SetPlayerData(oldPlayer.CaptureState());
            session.UnregisterPlayer(oldPlayer);

            // Unbind input before destroying the old player.
            playerSceneBinder.Unbind();

            Destroy(oldPlayer.gameObject);
        }

        session.SetDestination(destination.SceneName, destination.name);

        // Scene transitions must work even if gameplay was paused.
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

        // Allow OnSceneLoaded to finish player setup.
        yield return null;

        if (loading)
        {
            AbortTransition("The destination scene loaded, but player setup did not complete.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!loading)
            return;

        if (session == null)
        {
            AbortTransition("GameSession is missing during scene loading.");
            return;
        }

        if (pendingDestination == null)
        {
            AbortTransition("No destination was set for the scene transition.");
            return;
        }

        if (pendingDestination.SceneName != scene.name)
        {
            AbortTransition(
                $"Destination '{pendingDestination.name}' targets "
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

        if (!TryGetSpawnPoint(context, pendingDestination, out SceneSpawnPoint spawn))
        {
            AbortTransition(
                $"Could not find a valid spawn point for destination "
                    + $"'{pendingDestination.name}' in '{scene.name}'. "
                    + "Check the SceneContext configuration and SceneDestination assignments."
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

        Player player = playerSpawner.TrySpawn(context, spawn, saveData);

        if (player == null)
        {
            AbortTransition(
                "Player spawning failed. Check the player prefab, InputManager, "
                    + "scene camera, and scene UI references."
            );
            return;
        }

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
            return Fail("Assign Player Prefab.");

        if (!playerPrefab.TryGetComponent<Player>(out _))
            return Fail("Player prefab has no Player component.");

        return true;
    }

    private bool Fail(string message)
    {
        Debug.LogError($"[SceneTransitionManager] {message}", this);
        return false;
    }

    private SceneContext GetSceneContext(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.TryGetComponent<SceneContext>(out SceneContext context))
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

        Time.timeScale = 1f;

        if (session != null)
            session.CancelTransition();

        if (inputManager != null)
            inputManager.EnablePlayerMap();
    }

    private bool TryGetSpawnPoint(
        SceneContext context,
        SceneDestination destination,
        out SceneSpawnPoint spawn
    )
    {
        spawn = null;

        if (context == null || !context.ValidateConfiguration())
            return false;

        if (destination != null)
        {
            spawn = context.FindSpawnPoint(destination);
            return spawn != null;
        }

        spawn = context.GetDefaultSpawnPoint();
        return spawn != null;
    }

    private IEnumerator RestartRoutine()
    {
        if (session == null || !session.BeginTransition())
            yield break;

        loading = true;
        pendingDestination = initialDestination;

        Player oldPlayer = session.CurrentPlayer;

        if (oldPlayer != null)
        {
            ActionController controller = oldPlayer.GetComponent<ActionController>();

            if (controller != null)
                controller.CancelCurrentAction();

            session.UnregisterPlayer(oldPlayer);
            playerSceneBinder.Unbind();

            Destroy(oldPlayer.gameObject);
        }

        session.ResetForNewGame();
        session.SetDestination(initialDestination.SceneName, initialDestination.name);

        Time.timeScale = 1f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            initialDestination.SceneName,
            LoadSceneMode.Single
        );

        if (operation == null)
        {
            AbortTransition("Could not reload the starting scene.");
            yield break;
        }

        while (!operation.isDone)
            yield return null;

        // OnSceneLoaded handles spawning the player.
        yield return null;

        if (loading)
        {
            AbortTransition("The starting scene loaded, but player spawning did not complete.");
        }
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;

        playerSceneBinder?.Unbind();

        Instance = null;
    }
}
