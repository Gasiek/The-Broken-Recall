using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CardsCarousel
{
    private const float SlideDuration = 0.22f;
    private const float CardSlideDistance = 155f;

    private readonly MonoBehaviour coroutineRunner;
    private ActionCollection actionCollection;

    private Button prevCardButton;
    private Button nextCardButton;
    private Label cardCounter;
    private Label noCardsLabel;

    // Left Card
    private VisualElement leftCard;
    private Label leftCardTitle;
    private Image leftCardIcon;

    // Middle Card
    private VisualElement middleCard;
    private Label middleCardTitle;
    private Image middleCardIcon;

    // Right Card
    private VisualElement rightCard;
    private Label rightCardTitle;
    private Image rightCardIcon;

    private int currentCardIndex = 0;
    private bool isAnimating = false;
    private Coroutine slideCoroutine;

    public CardsCarousel(MonoBehaviour coroutineRunner)
    {
        this.coroutineRunner = coroutineRunner;
    }

    public void Initialize(VisualElement root, ActionCollection actionCollection)
    {
        this.actionCollection = actionCollection;

        prevCardButton = root.Q<Button>("PrevCardButton");
        nextCardButton = root.Q<Button>("NextCardButton");
        cardCounter = root.Q<Label>("CardCounter");
        noCardsLabel = root.Q<Label>("NoCardsLabel");

        // Query Left Card
        leftCard = root.Q<VisualElement>("LeftCard");
        if (leftCard != null)
        {
            leftCardTitle = leftCard.Q<Label>("LeftCardTitle");
            leftCardIcon = leftCard.Q<Image>("LeftCardIcon");
            leftCard.RegisterCallback<ClickEvent>(evt => Previous());
        }

        // Query Middle Card
        middleCard = root.Q<VisualElement>("MiddleCard");
        if (middleCard != null)
        {
            middleCardTitle = middleCard.Q<Label>("MiddleCardTitle");
            middleCardIcon = middleCard.Q<Image>("MiddleCardIcon");
        }

        // Query Right Card
        rightCard = root.Q<VisualElement>("RightCard");
        if (rightCard != null)
        {
            rightCardTitle = rightCard.Q<Label>("RightCardTitle");
            rightCardIcon = rightCard.Q<Image>("RightCardIcon");
            rightCard.RegisterCallback<ClickEvent>(evt => Next());
        }

        if (prevCardButton != null) prevCardButton.clicked += Previous;
        if (nextCardButton != null) nextCardButton.clicked += Next;
    }

    public void Cleanup()
    {
        StopAnimation();

        if (prevCardButton != null) prevCardButton.clicked -= Previous;
        if (nextCardButton != null) nextCardButton.clicked -= Next;
    }

    public void ResetIndex()
    {
        currentCardIndex = 0;
    }

    public void Previous()
    {
        if (isAnimating) return;
        if (actionCollection == null || actionCollection.Actions.Count <= 1) return;
        if (coroutineRunner != null)
        {
            slideCoroutine = coroutineRunner.StartCoroutine(AnimateCardSlide(1));
        }
    }

    public void Next()
    {
        if (isAnimating) return;
        if (actionCollection == null || actionCollection.Actions.Count <= 1) return;
        if (coroutineRunner != null)
        {
            slideCoroutine = coroutineRunner.StartCoroutine(AnimateCardSlide(-1));
        }
    }

    public void Refresh()
    {
        var actions = actionCollection != null ? actionCollection.Actions : null;
        int count = actions != null ? actions.Count : 0;

        if (count == 0)
        {
            if (leftCard != null) leftCard.style.display = DisplayStyle.None;
            if (middleCard != null) middleCard.style.display = DisplayStyle.None;
            if (rightCard != null) rightCard.style.display = DisplayStyle.None;
            if (noCardsLabel != null) noCardsLabel.style.display = DisplayStyle.Flex;
            if (cardCounter != null) cardCounter.text = "0 / 0";
            if (prevCardButton != null) prevCardButton.SetEnabled(false);
            if (nextCardButton != null) nextCardButton.SetEnabled(false);
            return;
        }

        if (noCardsLabel != null) noCardsLabel.style.display = DisplayStyle.None;

        currentCardIndex = ((currentCardIndex % count) + count) % count;

        if (cardCounter != null)
        {
            cardCounter.text = $"{currentCardIndex + 1} / {count}";
        }

        bool canNavigate = count > 1;
        if (prevCardButton != null) prevCardButton.SetEnabled(canNavigate);
        if (nextCardButton != null) nextCardButton.SetEnabled(canNavigate);

        // Populate Middle Card (Active card, ALWAYS in the center)
        if (middleCard != null)
        {
            middleCard.style.display = DisplayStyle.Flex;
            middleCard.style.visibility = Visibility.Visible;
            PopulateCard(middleCardTitle, middleCardIcon, actions[currentCardIndex]);
        }

        // Populate Left Card (Previous card in loop)
        if (leftCard != null)
        {
            if (count >= 2)
            {
                leftCard.style.display = DisplayStyle.Flex;
                leftCard.style.visibility = Visibility.Visible;
                int prevIndex = (currentCardIndex - 1 + count) % count;
                PopulateCard(leftCardTitle, leftCardIcon, actions[prevIndex]);
            }
            else
            {
                leftCard.style.display = DisplayStyle.None;
            }
        }

        // Populate Right Card (Next card in loop)
        if (rightCard != null)
        {
            if (count >= 2)
            {
                rightCard.style.display = DisplayStyle.Flex;
                rightCard.style.visibility = Visibility.Visible;
                int nextIndex = (currentCardIndex + 1) % count;
                PopulateCard(rightCardTitle, rightCardIcon, actions[nextIndex]);
            }
            else
            {
                rightCard.style.display = DisplayStyle.None;
            }
        }

        // Resting state: middle card is on top
        if (middleCard != null)
        {
            middleCard.BringToFront();
        }
    }

    private void PopulateCard(Label titleLabel, Image iconImage, ActionDefinition action)
    {
        if (action == null) return;

        if (titleLabel != null)
        {
            titleLabel.text = !string.IsNullOrEmpty(action.DisplayName) ? action.DisplayName : action.name;
        }

        if (iconImage != null)
        {
            if (action.Icon != null)
            {
                iconImage.sprite = action.Icon;
                iconImage.style.display = DisplayStyle.Flex;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.style.display = DisplayStyle.None;
            }
        }
    }

    private IEnumerator AnimateCardSlide(int direction)
    {
        isAnimating = true;

        float elapsed = 0f;
        float targetOffset = CardSlideDistance * direction;
        bool swappedLayers = false;

        VisualElement incomingCard = direction < 0 ? rightCard : leftCard;

        if (middleCard != null)
        {
            middleCard.BringToFront();
        }

        while (elapsed < SlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / SlideDuration);

            // In second half (t >= 0.5), incoming card takes the foreground over the exiting card
            if (!swappedLayers && t >= 0.5f)
            {
                swappedLayers = true;
                if (incomingCard != null)
                {
                    incomingCard.BringToFront();
                }
            }

            float ease = Mathf.SmoothStep(0f, 1f, t);
            float currentOffset = Mathf.Lerp(0f, targetOffset, ease);

            if (middleCard != null)
            {
                middleCard.style.translate = new StyleTranslate(new Translate(new Length(currentOffset, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
                float middleScale = Mathf.Lerp(1.08f, 0.85f, ease);
                middleCard.style.scale = new StyleScale(new Scale(new Vector2(middleScale, middleScale)));
                middleCard.style.opacity = Mathf.Lerp(1.0f, 0.65f, ease);
            }

            if (direction < 0) // Next card (cards slide left)
            {
                if (rightCard != null)
                {
                    rightCard.style.translate = new StyleTranslate(new Translate(new Length(currentOffset, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
                    float rightScale = Mathf.Lerp(0.85f, 1.08f, ease);
                    rightCard.style.scale = new StyleScale(new Scale(new Vector2(rightScale, rightScale)));
                    rightCard.style.opacity = Mathf.Lerp(0.65f, 1.0f, ease);
                }

                if (leftCard != null)
                {
                    leftCard.style.translate = new StyleTranslate(new Translate(new Length(currentOffset, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
                    leftCard.style.opacity = Mathf.Lerp(0.65f, 0.0f, ease);
                }
            }
            else // Prev card (cards slide right)
            {
                if (leftCard != null)
                {
                    leftCard.style.translate = new StyleTranslate(new Translate(new Length(currentOffset, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
                    float leftScale = Mathf.Lerp(0.85f, 1.08f, ease);
                    leftCard.style.scale = new StyleScale(new Scale(new Vector2(leftScale, leftScale)));
                    leftCard.style.opacity = Mathf.Lerp(0.65f, 1.0f, ease);
                }

                if (rightCard != null)
                {
                    rightCard.style.translate = new StyleTranslate(new Translate(new Length(currentOffset, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
                    rightCard.style.opacity = Mathf.Lerp(0.65f, 0.0f, ease);
                }
            }

            yield return null;
        }

        ResetCardTransformStyles();

        int count = actionCollection != null ? actionCollection.Actions.Count : 0;
        if (count > 0)
        {
            if (direction < 0)
            {
                currentCardIndex = (currentCardIndex + 1) % count;
            }
            else
            {
                currentCardIndex = (currentCardIndex - 1 + count) % count;
            }
        }

        Refresh();

        isAnimating = false;
        slideCoroutine = null;
    }

    private void ResetCardTransformStyles()
    {
        if (middleCard != null)
        {
            middleCard.style.translate = StyleKeyword.Null;
            middleCard.style.scale = StyleKeyword.Null;
            middleCard.style.opacity = StyleKeyword.Null;
        }

        if (leftCard != null)
        {
            leftCard.style.translate = StyleKeyword.Null;
            leftCard.style.scale = StyleKeyword.Null;
            leftCard.style.opacity = StyleKeyword.Null;
        }

        if (rightCard != null)
        {
            rightCard.style.translate = StyleKeyword.Null;
            rightCard.style.scale = StyleKeyword.Null;
            rightCard.style.opacity = StyleKeyword.Null;
        }
    }

    public void StopAnimation()
    {
        if (slideCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }
        isAnimating = false;
        ResetCardTransformStyles();

        if (middleCard != null)
        {
            middleCard.BringToFront();
        }
    }
}
