using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CardsMenuUI : MonoBehaviour
{
    [SerializeField]
    private ActionCollection actionCollection;

    private VisualElement cardsSection;
    private Button cardsBackButton;
    private Action backCallback;
    private CardsCarousel carousel;
    private bool isVisible;

    public CardsCarousel Carousel => carousel;

    private void OnEnable()
    {
        if (actionCollection != null)
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

        carousel?.StopAnimation();
    }

    private void OnDestroy()
    {
        if (carousel != null)
        {
            carousel.Cleanup();
            carousel = null;
        }
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

        if (carousel == null)
        {
            carousel = new CardsCarousel(this);
        }

        carousel.Initialize(root, actionCollection);
    }

    private void UnbindUI()
    {
        if (cardsBackButton != null)
        {
            cardsBackButton.clicked -= HandleBackClicked;
        }

        cardsBackButton = null;
        cardsSection = null;
        backCallback = null;
    }

    public void OnPreviousCard(InputAction.CallbackContext context)
    {
        if (!context.performed || !isVisible || carousel == null)
        {
            return;
        }

        carousel.Previous();
    }

    public void OnNextCard(InputAction.CallbackContext context)
    {
        if (!context.performed || !isVisible || carousel == null)
        {
            return;
        }

        carousel.Next();
    }

    private void HandleActionCollectionChanged()
    {
        if (carousel == null)
        {
            return;
        }

        carousel.StopAnimation();
        carousel.Refresh();
    }

    public void Show()
    {
        isVisible = true;

        if (cardsSection != null)
        {
            cardsSection.style.display = DisplayStyle.Flex;
        }

        if (carousel != null)
        {
            carousel.StopAnimation();
            carousel.ResetIndex();
            carousel.Refresh();
        }

        cardsBackButton?.Focus();
    }

    public void Hide()
    {
        isVisible = false;

        carousel?.StopAnimation();

        if (cardsSection != null)
        {
            cardsSection.style.display = DisplayStyle.None;
        }
    }

    private void HandleBackClicked()
    {
        Hide();
        backCallback?.Invoke();
    }
}
