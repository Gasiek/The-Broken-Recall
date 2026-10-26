using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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

        // The session may be created after this manager.
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

        SceneSpawnPoint spawn;

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

        if (!TryGetSpawnPoint(context, initialDestination, out spawn))
        {
            if (initialDestination != null)
            {
                Debug.LogError(
                    $"Could not find a valid spawn point for initial destination "
                        + $"'{initialDestination.name}' in '{activeScene.name}'.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    $"No default spawn point found in '{activeScene.name}'. "
                        + "Assign an Initial Destination or configure a default spawn.",
                    this
                );
            }

            yield break;
        }

        SpawnPlayer(context, spawn, session.PlayerData);
    }

    public bool TransitionTo(SceneDestination destination)
    {
        if (!initialized)
            return false;

        if (destination == null)
        {
            Debug.LogError("Cannot transition: destination asset is null.", this);
            return false;
        }

        if (loading)
            return false;

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
        // Acquire the session's transition state before doing destructive work.
        if (session == null || !session.BeginTransition())
        {
            pendingDestination = null;
            yield break;
        }

        loading = true;

        Player oldPlayer = session.CurrentPlayer;

        if (oldPlayer != null)
        {
            // Optional player systems may not exist on every configuration.
            ActionController controller = oldPlayer.GetComponent<ActionController>();

            if (controller != null)
                controller.CancelCurrentAction();

            session.SetPlayerData(oldPlayer.CaptureState());
            session.UnregisterPlayer(oldPlayer);

            playerSceneBinder.Unbind();

            Destroy(oldPlayer.gameObject);
        }

        session.SetDestination(destination.SceneName, destination.name);

        // Scene loading must work even if the game was paused.
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
        // Defensive boundary checks: this method must not spawn with invalid inputs.
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
                "Player spawning failed. Check the player prefab, scene camera, "
                    + "PlayerInput, Look action, and scene UI references."
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

        // Commit successful transition state.
        pendingDestination = null;
        loading = false;
    }

    private bool ValidatePlayerPrefab()
    {
        if (playerPrefab == null)
            return Fail("Assign Player Prefab.");

        if (!playerPrefab.TryGetComponent<Player>(out _))
            return Fail("Player prefab has no Player component.");

        if (!playerPrefab.TryGetComponent<PlayerInput>(out PlayerInput input))
            return Fail("Player prefab has no PlayerInput component.");

        if (input.actions == null || input.actions.FindAction("Look", false) == null)
        {
            return Fail(
                "Player prefab must have an Input Actions asset containing " + "a 'Look' action."
            );
        }

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

        if (session != null)
            session.CancelTransition();
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

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;

        playerSceneBinder?.Unbind();

        Instance = null;
    }
}
