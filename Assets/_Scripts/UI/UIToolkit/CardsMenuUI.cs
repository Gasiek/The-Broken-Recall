using UnityEngine;
using UnityEngine.UIElements;

public class CardsMenuUI : MonoBehaviour
{
    [SerializeField]
    private ActionCollection actionCollection;

    private VisualElement cardsSection;
    private Button cardsBackButton;
    private System.Action backCallback;
    private CardsCarousel carousel;

    public CardsCarousel Carousel => carousel;

    private void Awake()
    {
        FindDependencies();
    }

    private void OnEnable()
    {
        FindDependencies();
        if (actionCollection != null)
        {
            actionCollection.Changed += HandleActionCollectionChanged;
        }
    }

    private void OnDisable()
    {
        if (carousel != null)
        {
            carousel.StopAnimation();
        }

        if (actionCollection != null)
        {
            actionCollection.Changed -= HandleActionCollectionChanged;
        }
    }

    private void FindDependencies()
    {
        if (actionCollection == null)
        {
            actionCollection = FindFirstObjectByType<ActionCollection>();
        }
    }

    private void HandleActionCollectionChanged()
    {
        if (carousel != null)
        {
            carousel.StopAnimation();
            carousel.Refresh();
        }
    }

    public void Initialize(VisualElement root, System.Action onBackClicked)
    {
        FindDependencies();
        backCallback = onBackClicked;

        cardsSection = root.Q<VisualElement>("CardsSection");
        cardsBackButton = root.Q<Button>("CardsBackButton");

        if (cardsBackButton != null)
        {
            cardsBackButton.clicked += HandleBackClicked;
        }

        if (carousel == null)
        {
            carousel = new CardsCarousel(this);
        }
        carousel.Initialize(root, actionCollection);

        if (cardsSection != null)
        {
            cardsSection.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.LeftArrow || evt.keyCode == KeyCode.A)
                {
                    carousel?.Previous();
                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.RightArrow || evt.keyCode == KeyCode.D)
                {
                    carousel?.Next();
                    evt.StopPropagation();
                }
            });
        }
    }

    public void Show()
    {
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

        if (cardsBackButton != null)
        {
            cardsBackButton.Focus();
        }
    }

    public void Hide()
    {
        if (carousel != null)
        {
            carousel.StopAnimation();
        }

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

    private void OnDestroy()
    {
        if (cardsBackButton != null)
        {
            cardsBackButton.clicked -= HandleBackClicked;
        }

        if (carousel != null)
        {
            carousel.Cleanup();
        }
    }
}
