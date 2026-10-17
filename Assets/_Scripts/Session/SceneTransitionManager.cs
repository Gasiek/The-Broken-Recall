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
    private bool loading;
    private bool initialSpawnStarted;

    private SceneDestination pendingDestination;

    private PlayerInput boundPlayerInput;
    private InputAction boundLookAction;
    private CameraFollow boundCamera;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private IEnumerator Start()
    {
        // Let scene objects finish Awake and OnEnable.
        yield return null;

        if (initialSpawnStarted)
            yield break;

        initialSpawnStarted = true;

        if (session == null)
            session = GameSession.Instance;

        if (session == null)
        {
            Debug.LogError("[SceneTransitionManager] No GameSession exists.", this);
            yield break;
        }

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
        }

        // If no initial destination was assigned, use the scene's default.
        if (spawn == null && initialDestination == null)
            spawn = context.GetDefaultSpawnPoint();

        if (spawn == null)
        {
            Debug.LogError(
                $"No valid initial spawn point found in '{activeScene.name}'. "
                    + "Assign the Initial Destination or configure a default spawn point.",
                this
            );
            yield break;
        }

        SpawnPlayer(context, spawn, null, false);
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
                $"Destination scene '{sceneName}' is empty or not included " + "in Build Settings.",
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
        loading = true;
        session.BeginTransition();

        Player oldPlayer = session.CurrentPlayer;

        // Capture and disconnect the old player before destroying it.
        if (oldPlayer != null)
        {
            ActionController controller = oldPlayer.GetComponent<ActionController>();

            if (controller != null)
                controller.CancelCurrentAction();

            if (oldPlayer.WeaponBuffs != null)
                oldPlayer.WeaponBuffs.PauseBuffTimer();

            session.SetPlayerData(oldPlayer.CaptureState());
            session.UnregisterPlayer(oldPlayer);

            UnbindLookInput();

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

        // OnSceneLoaded should spawn and register the new player.
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

        SceneContext context = GetSceneContext(scene);

        if (context == null)
        {
            AbortTransition($"No root SceneContext found in '{scene.name}'.");
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
        $"Destination '{pendingDestination.name}' targets scene " +
        $"'{pendingDestination.SceneName}', but '{scene.name}' was loaded."
    );
    return;
}

SceneSpawnPoint spawn = context.FindSpawnPoint(pendingDestination);

if (spawn == null)
{
    AbortTransition(
        $"No spawn point in '{scene.name}' uses destination " +
        $"'{pendingDestination.name}'. Check the SceneDestination assignments."
    );
    return;
}

        if (spawn == null)
        {
            AbortTransition($"No usable spawn point found in '{scene.name}'.");
            return;
        }

        SpawnPlayer(context, spawn, session.PlayerData, true);
    }

    private void SpawnPlayer(
        SceneContext context,
        SceneSpawnPoint spawn,
        PlayerSaveData saveData,
        bool isTransition
    )
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

        GameObject instance = Instantiate(
            playerPrefab,
            spawn.transform.position,
            spawn.transform.rotation
        );

        Player player = instance.GetComponent<Player>();

        if (player == null)
        {
            Destroy(instance);
            AbortTransition("Player Prefab has no Player component.");
            return;
        }

        // Validate and establish required camera/input connections
        // before registering the player with the session.
        if (!ConnectPlayerToScene(player, context))
        {
            UnbindLookInput();
            Destroy(instance);

            AbortTransition(
                "Player setup failed. Check the scene camera, PlayerInput, "
                    + "and the Look input action."
            );
            return;
        }

        // Bind scene-specific UI references.
        context.BindPlayerUIReferences(player);

        InteractionController interactionController = player.GetComponent<InteractionController>();

        if (interactionController != null)
        {
            interactionController.SetInteractionPrompt(context.InteractionPrompt);
        }

        // Restore state before making the player active in the session.
        player.RestoreState(saveData);

        if (player.WeaponBuffs != null)
            player.WeaponBuffs.ResumeBuffTimer();

        if (!session.CompletePlayerSpawn(player))
        {
            UnbindLookInput();
            Destroy(player.gameObject);

            AbortTransition("GameSession rejected the newly spawned player.");
            return;
        }

        pendingDestination = null;
        loading = false;
    }

    private bool ConnectPlayerToScene(Player player, SceneContext context)
    {
        if (player == null || context == null)
            return false;

        CameraFollow cameraFollow = context.CameraFollow;

        if (cameraFollow == null)
        {
            Debug.LogError(
                $"SceneContext in '{player.gameObject.scene.name}' has no camera.",
                context
            );
            return false;
        }

        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("Player Prefab has no PlayerInput component.", player);
            return false;
        }

        if (playerInput.actions == null)
        {
            Debug.LogError("PlayerInput has no Input Actions asset assigned.", player);
            return false;
        }

        InputAction lookAction = playerInput.actions.FindAction("Look", false);

        if (lookAction == null)
        {
            Debug.LogError("The PlayerInput actions asset has no 'Look' action.", player);
            return false;
        }

        // All required references are valid. Connect the player.
        UnbindLookInput();

        cameraFollow.SetTarget(player.transform);

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
            movement.SetMovementReference(cameraFollow.transform);

        boundPlayerInput = playerInput;
        boundLookAction = lookAction;
        boundCamera = cameraFollow;

        // This script owns the Look callback.
        // Remove duplicate Look callbacks from PlayerInput's Inspector Unity Events.
        boundLookAction.performed += boundCamera.OnLook;

        return true;
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
            Debug.LogError("[SceneTransitionManager] Player Prefab has no Player component.", this);
            return false;
        }

        PlayerInput playerInput = playerPrefab.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError(
                "[SceneTransitionManager] Player Prefab has no PlayerInput component.",
                this
            );
            return false;
        }

        if (playerInput.actions == null || playerInput.actions.FindAction("Look", false) == null)
        {
            Debug.LogError(
                "[SceneTransitionManager] Player Prefab must have an "
                    + "Input Actions asset containing a 'Look' action.",
                this
            );
            return false;
        }

        return true;
    }

    private void UnbindLookInput()
    {
        if (boundLookAction != null && boundCamera != null)
            boundLookAction.performed -= boundCamera.OnLook;

        boundLookAction = null;
        boundPlayerInput = null;
        boundCamera = null;
    }

    private SceneContext GetSceneContext(Scene scene)
    {
        // SceneContext is expected on a root GameObject.
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

        UnbindLookInput();

        loading = false;
        pendingDestination = null;

        if (session != null)
            session.CancelTransition();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        UnbindLookInput();

        SceneManager.sceneLoaded -= OnSceneLoaded;

        Instance = null;
    }
}
