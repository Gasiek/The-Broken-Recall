using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CardsMenuUI : MonoBehaviour
{
    private ActionCollection actionCollection;

    [SerializeField]
    private VisualTreeAsset cardTemplate; // Assign your ActionlistItem.uxml template here

    private VisualElement cardsSection;
    private VisualElement cardContainer;
    private Button cardsBackButton;
    private Action backCallback;
    private bool isVisible;
    public bool IsVisible => isVisible;

    public void BindActionCollection(ActionCollection collection)
    {
        if (actionCollection == collection)
        {
            return;
        }

        if (actionCollection != null)
        {
            actionCollection.Changed -= HandleActionCollectionChanged;
        }

        actionCollection = collection;

        if (isVisible && actionCollection != null)
        {
            actionCollection.Changed += HandleActionCollectionChanged;
            RefreshCards();
        }
    }

    public void UnbindActionCollection(ActionCollection collection)
    {
        if (actionCollection != collection)
        {
            return;
        }

        actionCollection.Changed -= HandleActionCollectionChanged;
        actionCollection = null;

        if (cardContainer != null)
        {
            cardContainer.Clear();
        }
    }

    private void OnEnable()
    {
        if (isVisible && actionCollection != null)
        {
            actionCollection.Changed += HandleActionCollectionChanged;
        }
    }

    private void OnDisable()
    {
        isVisible = false;

        if (actionCollection != null)
        {
            actionCollection.Changed -= HandleActionCollectionChanged;
        }

        UnbindUI();
    }

    public void Initialize(VisualElement root, Action onBackClicked)
    {
        UnbindUI();

        backCallback = onBackClicked;

        if (root == null)
        {
            Debug.LogError("CardsMenuUI.Initialize received a null root.", this);
            return;
        }

        cardsSection = root.Q<VisualElement>("CardsSection");
        cardContainer = root.Q<VisualElement>("CardContainer");
        cardsBackButton = root.Q<Button>("CardsBackButton");

        if (cardsBackButton != null)
        {
            cardsBackButton.clicked += HandleBackClicked;
        }
        else
        {
            Debug.LogWarning("CardsBackButton was not found in the UI.", this);
        }

        if (cardsSection == null)
        {
            Debug.LogWarning("CardsSection was not found in the UI.", this);
        }
    }

    private void UnbindUI()
    {
        if (cardsBackButton != null)
        {
            cardsBackButton.clicked -= HandleBackClicked;
        }

        cardsBackButton = null;
        cardsSection = null;
        cardContainer = null;
        backCallback = null;
    }

    private void HandleActionCollectionChanged()
    {
        if (isVisible)
        {
            RefreshCards();
        }
    }

    public void Show()
    {
        if (!isVisible && actionCollection != null && isActiveAndEnabled)
        {
            actionCollection.Changed += HandleActionCollectionChanged;
        }

        isVisible = true;

        if (cardsSection != null)
        {
            cardsSection.style.display = DisplayStyle.Flex;
        }

        RefreshCards();
        cardsBackButton?.Focus();
    }

    private void RefreshCards()
    {
        if (cardContainer == null || actionCollection == null || cardTemplate == null)
        {
            return;
        }

        cardContainer.Clear();

        foreach (var cardInstance in actionCollection.Actions)
        {
            if (cardInstance == null || cardInstance.Definition == null)
            {
                continue;
            }

            ActionDefinition definition = cardInstance.Definition;

            // Instantiate card template
            VisualElement cardElement = cardTemplate.Instantiate();
            Button button = cardElement.Q<Button>("ActionButton");
            Label nameLabel = cardElement.Q<Label>("Name");
            Image iconImage = cardElement.Q<Image>("Icon");
            Label descLabel = cardElement.Q<Label>("Description");

            if (nameLabel != null)
            {
                nameLabel.text = definition.DisplayName;
            }

            if (iconImage != null)
            {
                iconImage.image = definition.Icon != null ? definition.Icon.texture : null;
            }

            if (descLabel != null)
            {
                descLabel.text = definition.GetDescription();
            }

            // Optional: disable button clicking if this menu is strictly view-only
            if (button != null)
            {
                button.focusable = false; // or leave clickable if you want card interaction in the menu
            }

            cardContainer.Add(cardElement);
        }
    }

    public void Hide()
    {
        if (isVisible && actionCollection != null)
        {
            actionCollection.Changed -= HandleActionCollectionChanged;
        }

        isVisible = false;

        if (cardsSection != null)
        {
            cardsSection.style.display = DisplayStyle.None;
        }

        cardContainer?.Clear();
    }

    private void HandleBackClicked()
    {
        Hide();
        backCallback?.Invoke();
    }
}
