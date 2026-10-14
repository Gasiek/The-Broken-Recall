using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class ActionBarUI : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private ActionMemory actionMemory;

    [SerializeField]
    private ActionController actionController;

    private PanelRenderer panelRenderer;

    private VisualElement[] slotElements;
    private VisualElement[] iconElements;
    private RadialCooldownElement[] cooldownElements;
    private Label[] actionNameLabels;

    private Label drawPileLabel;

    private Coroutine cooldownRoutine;

    private const int SlotCount = ActionMemory.SlotCount;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();

        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        if (actionMemory != null)
        {
            actionMemory.MemoryChanged += RefreshSlots;
        }

        if (actionController != null)
        {
            actionController.ActionStarted += StartCooldown;
        }
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        if (actionMemory != null)
        {
            actionMemory.MemoryChanged -= RefreshSlots;
        }

        if (actionController != null)
        {
            actionController.ActionStarted -= StartCooldown;
        }

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        ClearElementReferences();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        if (root == null)
        {
            Debug.LogError("ActionBarUI received a null UI root.", this);
            return;
        }

        slotElements = new VisualElement[SlotCount];
        iconElements = new VisualElement[SlotCount];
        cooldownElements = new RadialCooldownElement[SlotCount];
        actionNameLabels = new Label[SlotCount];

        for (int i = 0; i < SlotCount; i++)
        {
            slotElements[i] = root.Q<VisualElement>($"ActionSlot{i}");

            if (slotElements[i] == null)
            {
                Debug.LogError($"ActionBarUI could not find ActionSlot{i}.", this);

                continue;
            }

            iconElements[i] = slotElements[i].Q<VisualElement>("ActionIcon");

            cooldownElements[i] = slotElements[i].Q<RadialCooldownElement>("CooldownOverlay");

            actionNameLabels[i] = slotElements[i].Q<Label>("ActionName");
        }

        drawPileLabel = root.Q<Label>("DrawPileText");

        RefreshSlots();
        RefreshCooldown(0f);
        RefreshDrawPileCount();
    }

    private void RefreshSlots()
    {
        if (actionMemory == null || slotElements == null)
        {
            return;
        }

        for (int i = 0; i < SlotCount; i++)
        {
            if (slotElements[i] == null)
            {
                continue;
            }

            // ActionMemory returns the card instance occupying this slot.
            ActionCardInstance card = actionMemory.GetAction(i);

            // The definition contains the shared display data.
            ActionDefinition action = card?.Definition;

            if (action == null)
            {
                if (iconElements[i] != null)
                {
                    iconElements[i].style.backgroundImage = StyleKeyword.None;

                    iconElements[i].style.display = DisplayStyle.None;
                }

                if (actionNameLabels[i] != null)
                {
                    actionNameLabels[i].text = string.Empty;
                }

                continue;
            }

            if (iconElements[i] != null)
            {
                Sprite icon = action.Icon;

                iconElements[i].style.backgroundImage =
                    icon != null ? new StyleBackground(icon) : StyleKeyword.None;

                iconElements[i].style.display =
                    icon != null ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (actionNameLabels[i] != null)
            {
                actionNameLabels[i].text = action.DisplayName;
            }
        }

        RefreshDrawPileCount();
    }

    private void RefreshDrawPileCount()
    {
        if (drawPileLabel == null || actionMemory == null)
        {
            return;
        }

        drawPileLabel.text = actionMemory.DrawPileCount.ToString();
    }

    private void StartCooldown(float duration)
    {
        if (duration <= 0f || cooldownElements == null)
        {
            return;
        }

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        cooldownRoutine = StartCoroutine(CooldownRoutine(duration));
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = 1f - Mathf.Clamp01(elapsed / duration);

            RefreshCooldown(progress);

            yield return null;
        }

        RefreshCooldown(0f);
        cooldownRoutine = null;
    }

    private void RefreshCooldown(float progress)
    {
        if (cooldownElements == null)
        {
            return;
        }

        foreach (RadialCooldownElement element in cooldownElements)
        {
            if (element != null)
            {
                element.Progress = progress;
            }
        }
    }

    private void ClearElementReferences()
    {
        slotElements = null;
        iconElements = null;
        cooldownElements = null;
        actionNameLabels = null;
        drawPileLabel = null;
    }
}
