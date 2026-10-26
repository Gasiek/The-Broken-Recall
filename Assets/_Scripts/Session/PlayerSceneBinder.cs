using UnityEngine;

public sealed class PlayerSceneBinder
{
    private InputManager inputManager;
    private PauseController boundPauseController;

    private CardsMenuUI boundCardsMenuUI;
    private ActionCollection boundActionCollection;

    private SkillLearnUI boundSkillLearnUI;
    private SkillRemovalUI boundSkillRemovalUI;

    private CameraFollow boundCamera;

    public bool TryBind(Player player, SceneContext context)
    {
        if (player == null || context == null)
        {
            Debug.LogError("[PlayerSceneBinder] Player and SceneContext are required.");
            return false;
        }

        CameraFollow cameraFollow = context.CameraFollow;
        PauseController pauseController = context.PauseController;
        CardsMenuUI cardsMenuUI = context.CardsMenuUI;
        SkillLearnUI skillLearnUI = context.SkillLearnUI;
        SkillRemovalUI skillRemovalUI = context.SkillRemovalUI;

        if (cameraFollow == null)
        {
            Debug.LogError(
                $"[PlayerSceneBinder] Scene '{player.gameObject.scene.name}' "
                    + "has no CameraFollow."
            );
            return false;
        }

        if (pauseController == null || cardsMenuUI == null)
        {
            Debug.LogError(
                $"[PlayerSceneBinder] Scene '{player.gameObject.scene.name}' "
                    + "must provide PauseController and CardsMenuUI."
            );
            return false;
        }

        ActionCollection actionCollection = player.GetComponent<ActionCollection>();

        if (actionCollection == null)
        {
            Debug.LogError("[PlayerSceneBinder] Player has no ActionCollection.", player);
            return false;
        }

        InputManager manager = InputManager.Instance;

        if (manager == null)
        {
            Debug.LogError(
                "[PlayerSceneBinder] No InputManager exists. "
                    + "Ensure the persistent InputManager is created before spawning.",
                player
            );
            return false;
        }

        // Clear bindings left over from a previous player or scene.
        Unbind();

        inputManager = manager;
        boundCamera = cameraFollow;
        boundPauseController = pauseController;
        boundCardsMenuUI = cardsMenuUI;
        boundActionCollection = actionCollection;
        boundSkillLearnUI = skillLearnUI;
        boundSkillRemovalUI = skillRemovalUI;

        try
        {
            if (!inputManager.BindPlayer(player, cameraFollow, pauseController))
            {
                Unbind();
                return false;
            }
            boundPauseController.BindPlayer();

            boundCardsMenuUI.BindActionCollection(boundActionCollection);

            if (boundSkillLearnUI != null)
            {
                boundSkillLearnUI.BindRuntimeReferences(boundPauseController);
            }

            if (boundSkillRemovalUI != null)
            {
                boundSkillRemovalUI.BindRuntimeReferences(boundPauseController);
            }

            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);

            Unbind();
            return false;
        }
    }

    public void Unbind()
    {
        // Clear UI references before disconnecting the pause controller.
        if (boundSkillLearnUI != null)
            boundSkillLearnUI.BindRuntimeReferences(null);

        if (boundSkillRemovalUI != null)
            boundSkillRemovalUI.BindRuntimeReferences(null);

        if (boundCardsMenuUI != null && boundActionCollection != null)
        {
            boundCardsMenuUI.UnbindActionCollection(boundActionCollection);
        }

        if (boundPauseController != null)
            boundPauseController.UnbindPlayer();

        if (inputManager != null)
            inputManager.UnbindPlayer();

        if (boundCamera != null)
            boundCamera.SetTarget(null);

        inputManager = null;
        boundCamera = null;
        boundPauseController = null;
        boundCardsMenuUI = null;
        boundActionCollection = null;
        boundSkillLearnUI = null;
        boundSkillRemovalUI = null;
    }
}
