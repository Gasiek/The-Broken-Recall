using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class ActionBarUI : PlayerHUDBase
{
    private PanelRenderer panelRenderer;
    private ActionMemory actionMemory;

    private VisualElement[] slotElements;
    private VisualElement[] iconElements;
    private Label[] actionNameLabels;
    private Label drawPileLabel;

    private const int SlotCount = ActionMemory.SlotCount;

    protected override void OnEnable()
    {
        base.OnEnable();

        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    protected override void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        ClearElementReferences();

        base.OnDisable();
    }

    protected override void OnPlayerBound(Player player)
    {
        actionMemory = player.ActionMemory;

        if (actionMemory != null)
            actionMemory.MemoryChanged += RefreshSlots;

        RefreshSlots();
        RefreshDrawPileCount();
    }

    protected override void OnPlayerUnbound(Player player)
    {
        if (actionMemory != null)
            actionMemory.MemoryChanged -= RefreshSlots;

        actionMemory = null;
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
        actionNameLabels = new Label[SlotCount];

        for (int i = 0; i < SlotCount; i++)
        {
            slotElements[i] = root.Q<VisualElement>($"ActionSlot{i}");

            if (slotElements[i] == null)
            {
                Debug.LogError($"Missing ActionSlot{i} in the HUD UXML.", this);
                continue;
            }

            iconElements[i] = slotElements[i].Q<VisualElement>("ActionIcon");
            actionNameLabels[i] = slotElements[i].Q<Label>("ActionName");
        }

        drawPileLabel = root.Q<Label>("DrawPileText");

        RefreshSlots();
        RefreshDrawPileCount();
    }

    private void RefreshSlots()
    {
        if (actionMemory == null || slotElements == null)
            return;

        for (int i = 0; i < SlotCount; i++)
        {
            if (slotElements[i] == null)
                continue;

            ActionCardInstance card = actionMemory.GetAction(i);
            ActionDefinition action = card?.Definition;

            if (action == null)
            {
                if (iconElements[i] != null)
                {
                    iconElements[i].style.backgroundImage = StyleKeyword.None;
                    iconElements[i].style.display = DisplayStyle.None;
                }

                if (actionNameLabels[i] != null)
                    actionNameLabels[i].text = string.Empty;

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
                actionNameLabels[i].text = action.DisplayName;
        }

        RefreshDrawPileCount();
    }

    private void RefreshDrawPileCount()
    {
        if (drawPileLabel == null || actionMemory == null)
            return;

        drawPileLabel.text = actionMemory.DrawPileCount.ToString();
    }

    private void ClearElementReferences()
    {
        slotElements = null;
        iconElements = null;
        actionNameLabels = null;
        drawPileLabel = null;
    }
}
