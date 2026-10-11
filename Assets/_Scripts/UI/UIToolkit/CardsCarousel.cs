using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CardsCarousel
{
    private const float SlideDuration = 0.22f;
    private const float CardSlideDistance = 155f;
    private const float FlipHalfDuration = 0.12f;
    private const float ActiveCardScale = 1.08f;
    private const float SideCardScale = 0.85f;

    private readonly MonoBehaviour coroutineRunner;

    private ActionCollection actionCollection;

    private Button prevCardButton;
    private Button nextCardButton;
    private Label cardCounter;
    private Label noCardsLabel;

    // Left card
    private VisualElement leftCard;
    private Label leftCardTitle;
    private Image leftCardIcon;

    // Middle card
    private VisualElement middleCard;
    private Label middleCardTitle;
    private Image middleCardIcon;
    private VisualElement middleCardFront;
    private VisualElement middleCardBack;
    private Label middleCardBackTitle;
    private Label middleCardDescription;
    private Button middleCardFlipButton;

    // Right card
    private VisualElement rightCard;
    private Label rightCardTitle;
    private Image rightCardIcon;

    private int currentCardIndex;
    private bool isAnimating;
    private bool isFlipped;
    private bool isFlipping;

    private Coroutine slideCoroutine;
    private Coroutine flipCoroutine;

    public CardsCarousel(MonoBehaviour coroutineRunner)
    {
        this.coroutineRunner = coroutineRunner;
    }

    public void Initialize(VisualElement root, ActionCollection collection)
    {
        StopAnimation();
        UnbindUI();

        actionCollection = collection;

        if (root == null)
        {
            Debug.LogError("CardsCarousel.Initialize received a null root.");

            return;
        }

        prevCardButton = root.Q<Button>("PrevCardButton");
        nextCardButton = root.Q<Button>("NextCardButton");
        cardCounter = root.Q<Label>("CardCounter");
        noCardsLabel = root.Q<Label>("NoCardsLabel");

        // Left card
        leftCard = root.Q<VisualElement>("LeftCard");

        if (leftCard != null)
        {
            leftCardTitle = leftCard.Q<Label>("LeftCardTitle");
            leftCardIcon = leftCard.Q<Image>("LeftCardIcon");

            leftCard.RegisterCallback<ClickEvent>(HandleLeftCardClicked);
        }

        // Middle card
        middleCard = root.Q<VisualElement>("MiddleCard");

        if (middleCard != null)
        {
            middleCardTitle = middleCard.Q<Label>("MiddleCardTitle");
            middleCardIcon = middleCard.Q<Image>("MiddleCardIcon");
            middleCardFront = middleCard.Q<VisualElement>("MiddleCardFront");
            middleCardBack = middleCard.Q<VisualElement>("MiddleCardBack");
            middleCardBackTitle = middleCard.Q<Label>("MiddleCardBackTitle");
            middleCardDescription = middleCard.Q<Label>("MiddleCardDescription");
            middleCardFlipButton = middleCard.Q<Button>("MiddleCardFlipButton");

            if (middleCardFlipButton != null)
            {
                middleCardFlipButton.clicked += Flip;
            }
        }

        // Right card
        rightCard = root.Q<VisualElement>("RightCard");

        if (rightCard != null)
        {
            rightCardTitle = rightCard.Q<Label>("RightCardTitle");
            rightCardIcon = rightCard.Q<Image>("RightCardIcon");

            rightCard.RegisterCallback<ClickEvent>(HandleRightCardClicked);
        }

        if (prevCardButton != null)
        {
            prevCardButton.clicked += Previous;
        }

        if (nextCardButton != null)
        {
            nextCardButton.clicked += Next;
        }

        ResetFlipVisuals();
        ResetCardTransformStyles();
        Refresh();
    }

    public void Cleanup()
    {
        StopAnimation();
        UnbindUI();

        actionCollection = null;

        leftCard = null;
        middleCard = null;
        rightCard = null;
    }

    private void UnbindUI()
    {
        if (leftCard != null)
        {
            leftCard.UnregisterCallback<ClickEvent>(HandleLeftCardClicked);
        }

        if (rightCard != null)
        {
            rightCard.UnregisterCallback<ClickEvent>(HandleRightCardClicked);
        }

        if (middleCardFlipButton != null)
        {
            middleCardFlipButton.clicked -= Flip;
        }

        if (prevCardButton != null)
        {
            prevCardButton.clicked -= Previous;
        }

        if (nextCardButton != null)
        {
            nextCardButton.clicked -= Next;
        }

        prevCardButton = null;
        nextCardButton = null;
        cardCounter = null;
        noCardsLabel = null;

        leftCardTitle = null;
        leftCardIcon = null;

        middleCardTitle = null;
        middleCardIcon = null;
        middleCardFront = null;
        middleCardBack = null;
        middleCardBackTitle = null;
        middleCardDescription = null;
        middleCardFlipButton = null;

        rightCardTitle = null;
        rightCardIcon = null;
    }

    private void HandleLeftCardClicked(ClickEvent evt)
    {
        Previous();
    }

    private void HandleRightCardClicked(ClickEvent evt)
    {
        Next();
    }

    public void ResetIndex()
    {
        currentCardIndex = 0;
        ResetFlipState();
    }

    public void Flip()
    {
        if (isAnimating || isFlipping || middleCard == null)
        {
            return;
        }

        if (coroutineRunner == null)
        {
            isFlipped = !isFlipped;
            UpdateFlipVisuals();
            return;
        }

        flipCoroutine = coroutineRunner.StartCoroutine(AnimateCardFlip());
    }

    private IEnumerator AnimateCardFlip()
    {
        isFlipping = true;

        float elapsed = 0f;

        // Shrink horizontally to simulate turning the card.
        while (elapsed < FlipHalfDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / FlipHalfDuration);
            float scaleX = Mathf.Lerp(ActiveCardScale, 0f, t);

            if (middleCard != null)
            {
                middleCard.style.scale = new StyleScale(
                    new Scale(new Vector2(scaleX, ActiveCardScale))
                );
            }

            yield return null;
        }

        isFlipped = !isFlipped;
        UpdateFlipVisuals();

        // Expand the card again.
        elapsed = 0f;

        while (elapsed < FlipHalfDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / FlipHalfDuration);
            float scaleX = Mathf.Lerp(0f, ActiveCardScale, t);

            if (middleCard != null)
            {
                middleCard.style.scale = new StyleScale(
                    new Scale(new Vector2(scaleX, ActiveCardScale))
                );
            }

            yield return null;
        }

        if (middleCard != null)
        {
            middleCard.style.scale = new StyleScale(
                new Scale(new Vector2(ActiveCardScale, ActiveCardScale))
            );
        }

        isFlipping = false;
        flipCoroutine = null;
    }

    private void ResetFlipState()
    {
        if (flipCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }

        ResetFlipVisuals();
    }

    private void ResetFlipVisuals()
    {
        isFlipped = false;
        isFlipping = false;
        UpdateFlipVisuals();
    }

    private void UpdateFlipVisuals()
    {
        if (middleCardFront != null)
        {
            middleCardFront.style.display = isFlipped ? DisplayStyle.None : DisplayStyle.Flex;
        }

        if (middleCardBack != null)
        {
            middleCardBack.style.display = isFlipped ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void Previous()
    {
        if (isAnimating || isFlipping)
        {
            return;
        }

        if (
            actionCollection == null
            || actionCollection.Actions == null
            || actionCollection.Actions.Count <= 1
        )
        {
            return;
        }

        if (coroutineRunner != null)
        {
            slideCoroutine = coroutineRunner.StartCoroutine(AnimateCardSlide(1));
        }
    }

    public void Next()
    {
        if (isAnimating || isFlipping)
        {
            return;
        }

        if (
            actionCollection == null
            || actionCollection.Actions == null
            || actionCollection.Actions.Count <= 1
        )
        {
            return;
        }

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
            ResetFlipState();
            ResetCardTransformStyles();

            if (leftCard != null)
                leftCard.style.display = DisplayStyle.None;

            if (middleCard != null)
                middleCard.style.display = DisplayStyle.None;

            if (rightCard != null)
                rightCard.style.display = DisplayStyle.None;

            if (noCardsLabel != null)
                noCardsLabel.style.display = DisplayStyle.Flex;

            if (cardCounter != null)
                cardCounter.text = "0 / 0";

            if (prevCardButton != null)
                prevCardButton.SetEnabled(false);

            if (nextCardButton != null)
                nextCardButton.SetEnabled(false);

            return;
        }

        if (noCardsLabel != null)
            noCardsLabel.style.display = DisplayStyle.None;

        currentCardIndex = ((currentCardIndex % count) + count) % count;

        if (cardCounter != null)
        {
            cardCounter.text = $"{currentCardIndex + 1} / {count}";
        }

        bool canNavigate = count > 1;

        if (prevCardButton != null)
            prevCardButton.SetEnabled(canNavigate);

        if (nextCardButton != null)
            nextCardButton.SetEnabled(canNavigate);

        // Middle card
        if (middleCard != null)
        {
            middleCard.style.display = DisplayStyle.Flex;
            middleCard.style.visibility = Visibility.Visible;

            ActionDefinition currentAction = actions[currentCardIndex];

            PopulateCard(middleCardTitle, middleCardIcon, currentAction);

            string title = GetActionTitle(currentAction);

            if (middleCardBackTitle != null)
                middleCardBackTitle.text = title;

            if (middleCardDescription != null)
            {
                middleCardDescription.text =
                    currentAction != null ? currentAction.GetDescription() : string.Empty;
            }
        }

        // Left card
        if (leftCard != null)
        {
            if (count >= 2)
            {
                leftCard.style.display = DisplayStyle.Flex;
                leftCard.style.visibility = Visibility.Visible;

                int previousIndex = (currentCardIndex - 1 + count) % count;

                PopulateCard(leftCardTitle, leftCardIcon, actions[previousIndex]);
            }
            else
            {
                leftCard.style.display = DisplayStyle.None;
            }
        }

        // Right card
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

        middleCard?.BringToFront();
        UpdateFlipVisuals();
    }

    private static string GetActionTitle(ActionDefinition action)
    {
        if (action == null)
            return string.Empty;

        return !string.IsNullOrEmpty(action.DisplayName) ? action.DisplayName : action.name;
    }

    private void PopulateCard(Label titleLabel, Image iconImage, ActionDefinition action)
    {
        if (titleLabel != null)
        {
            titleLabel.text = GetActionTitle(action);
        }

        if (iconImage == null)
            return;

        Sprite icon = action != null ? action.Icon : null;

        iconImage.sprite = icon;
        iconImage.style.display = icon != null ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private IEnumerator AnimateCardSlide(int direction)
    {
        ResetFlipState();
        isAnimating = true;

        float elapsed = 0f;
        float targetOffset = CardSlideDistance * direction;
        bool swappedLayers = false;

        VisualElement incomingCard = direction < 0 ? rightCard : leftCard;

        middleCard?.BringToFront();

        while (elapsed < SlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / SlideDuration);

            if (!swappedLayers && t >= 0.5f)
            {
                swappedLayers = true;
                incomingCard?.BringToFront();
            }

            float ease = Mathf.SmoothStep(0f, 1f, t);
            float currentOffset = Mathf.Lerp(0f, targetOffset, ease);

            if (middleCard != null)
            {
                middleCard.style.translate = new StyleTranslate(
                    new Translate(
                        new Length(currentOffset, LengthUnit.Pixel),
                        new Length(0, LengthUnit.Pixel)
                    )
                );

                float scale = Mathf.Lerp(ActiveCardScale, SideCardScale, ease);

                middleCard.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));

                middleCard.style.opacity = Mathf.Lerp(1f, 0.65f, ease);
            }

            if (direction < 0)
            {
                if (rightCard != null)
                {
                    rightCard.style.translate = new StyleTranslate(
                        new Translate(
                            new Length(currentOffset, LengthUnit.Pixel),
                            new Length(0, LengthUnit.Pixel)
                        )
                    );

                    float scale = Mathf.Lerp(SideCardScale, ActiveCardScale, ease);

                    rightCard.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));

                    rightCard.style.opacity = Mathf.Lerp(0.65f, 1f, ease);
                }

                if (leftCard != null)
                {
                    leftCard.style.translate = new StyleTranslate(
                        new Translate(
                            new Length(currentOffset, LengthUnit.Pixel),
                            new Length(0, LengthUnit.Pixel)
                        )
                    );

                    leftCard.style.opacity = Mathf.Lerp(0.65f, 0f, ease);
                }
            }
            else
            {
                if (leftCard != null)
                {
                    leftCard.style.translate = new StyleTranslate(
                        new Translate(
                            new Length(currentOffset, LengthUnit.Pixel),
                            new Length(0, LengthUnit.Pixel)
                        )
                    );

                    float scale = Mathf.Lerp(SideCardScale, ActiveCardScale, ease);

                    leftCard.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));

                    leftCard.style.opacity = Mathf.Lerp(0.65f, 1f, ease);
                }

                if (rightCard != null)
                {
                    rightCard.style.translate = new StyleTranslate(
                        new Translate(
                            new Length(currentOffset, LengthUnit.Pixel),
                            new Length(0, LengthUnit.Pixel)
                        )
                    );

                    rightCard.style.opacity = Mathf.Lerp(0.65f, 0f, ease);
                }
            }

            yield return null;
        }

        ResetCardTransformStyles();

        int count =
            actionCollection != null && actionCollection.Actions != null
                ? actionCollection.Actions.Count
                : 0;

        if (count > 0)
        {
            currentCardIndex =
                direction < 0
                    ? (currentCardIndex + 1) % count
                    : (currentCardIndex - 1 + count) % count;
        }

        isAnimating = false;
        slideCoroutine = null;

        Refresh();
    }

    private void ResetCardTransformStyles()
    {
        ResetCardTransform(middleCard);
        ResetCardTransform(leftCard);
        ResetCardTransform(rightCard);
    }

    private static void ResetCardTransform(VisualElement card)
    {
        if (card == null)
            return;

        card.style.translate = StyleKeyword.Null;
        card.style.scale = StyleKeyword.Null;
        card.style.opacity = StyleKeyword.Null;
    }

    public void StopAnimation()
    {
        if (slideCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }

        if (flipCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }

        isAnimating = false;
        ResetFlipVisuals();
        ResetCardTransformStyles();

        middleCard?.BringToFront();
    }
}
