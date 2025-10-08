using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class SlidingTaskbarController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private List<Button> taskbarButtons;
    [SerializeField] private Image questNoti;
    [Tooltip("icon button")]
    [SerializeField] private List<Image> buttonIcons;
    [SerializeField] private Image selectedImageIndicator;
    [SerializeField] private List<CanvasGroup> iconTexts;

    [SerializeField] private HorizontalLayoutGroup buttonLayoutGroup;

    [Header("Initial State")]
    [SerializeField] private int defaultSelectedButtonIndex = 2;

    [Header("Animation Settings")]
    [SerializeField] private float expandedScaleMultiplier = 1.2f;
    [Tooltip("Icon scale")]
    [SerializeField] private float iconScaleMultiplier = 1.2f;
    [Tooltip("Icon pop height")]
    [SerializeField] private float iconHopHeight = 20f;
    [SerializeField] private Color unselectedIconColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private float resizeDuration = 0.25f;
    [SerializeField] private float slideStepDuration = 0.08f;
    [SerializeField] private Ease slideEase = Ease.Linear;
    [SerializeField] private float finalSlideDuration = 0.2f;
    [SerializeField] private Ease finalSlideEase = Ease.OutQuad;
    [SerializeField] private float indicatorFixedYPosition = -100f;
    [SerializeField] private float iconYOffset = 100f;

    private List<RectTransform> buttonRects = new List<RectTransform>();
    private RectTransform indicatorRect;
    private List<Color> originalIconColors = new List<Color>();
    private float initialButtonWidth, expandedWidth, contractedWidth;
    private int currentSelectedIndex = -1;
    private bool isAnimating = false;
    private Sequence animationSequence;

    private void Start()
    {
        if (selectedImageIndicator != null)
        {
            indicatorRect = selectedImageIndicator.GetComponent<RectTransform>();
        }
        if (buttonIcons.Count != taskbarButtons.Count)
        {
            Debug.LogError("Số lượng Button và Icon không khớp!");
        }

        foreach (var icon in buttonIcons)
        {
            originalIconColors.Add(icon.color);
        }

        for (int i = 0; i < taskbarButtons.Count; i++)
        {
            buttonRects.Add(taskbarButtons[i].GetComponent<RectTransform>());
            int buttonIndex = i;
            taskbarButtons[i].onClick.AddListener(() => OnButtonClicked(buttonIndex));
        }

        StartCoroutine(InitializeLayout());
    }

    private IEnumerator InitializeLayout()
    {
        yield return new WaitForEndOfFrame();
        if (buttonRects.Count == 0 || buttonLayoutGroup == null) yield break;

        initialButtonWidth = buttonRects[0].rect.width;
        expandedWidth = initialButtonWidth * expandedScaleMultiplier;
        float totalWidth = initialButtonWidth * taskbarButtons.Count;
        float remainingWidth = totalWidth - expandedWidth;
        contractedWidth = remainingWidth / (taskbarButtons.Count - 1);
        buttonLayoutGroup.childControlWidth = false;

        SelectButton(defaultSelectedButtonIndex, false);
    }

    private void OnButtonClicked(int index)
    {
        if (Input.touchCount > 1) return;
        if (isAnimating || index == currentSelectedIndex) return;
        SelectButton(index, true);
        if (UIManager.Instance != null)
        {
            switch (index)
            {
                case 0: // Event UI
                    UIManager.Instance.ShowEventUI();
                    break;
                case 1: // My Work UI
                    UIManager.Instance.ShowMyWorkUI();
                    break;
                case 2: // Gallery UI
                    UIManager.Instance.ShowGallary();
                    break;
                case 3: // Quest UI
                    UIManager.Instance.ShowQuestUI();
                    break;
                case 4: // Camera UI
                    UIManager.Instance.ShowCameraUI();
                    break;
                default:
                    Debug.LogWarning("Taskbar button index " + index + " không có chức năng tương ứng trong UIManager.");
                    break;
            }
        }
        else
        {
            Debug.LogError("UIManager not found");
        }
    }

    private void SelectButton(int index, bool animate)
    {
        Vector2[] targetButtonPositions = GetButtonTargetPositions(index);

        if (!animate)
        {
            currentSelectedIndex = index;
            for (int i = 0; i < buttonRects.Count; i++)
            {
                float targetWidth = (i == index) ? expandedWidth : contractedWidth;
                buttonRects[i].sizeDelta = new Vector2(targetWidth, buttonRects[i].sizeDelta.y);
                if (buttonIcons.Count > i)
                {
                    buttonIcons[i].transform.localScale = Vector3.one * (i == index ? iconScaleMultiplier : 1f);

                    Vector2 iconPos = targetButtonPositions[i];
                    if (i == index) iconPos.y += iconHopHeight;
                    buttonIcons[i].rectTransform.anchoredPosition = iconPos;
                    buttonIcons[i].color = (i == index) ? originalIconColors[i] : unselectedIconColor;
                    iconTexts[i].alpha = (i == index) ? 1f : 0f;
                }
            }
            if (indicatorRect != null)
            {
                indicatorRect.gameObject.SetActive(true);
                indicatorRect.anchoredPosition = new Vector2(targetButtonPositions[index].x, indicatorFixedYPosition);
                indicatorRect.sizeDelta = new Vector2(expandedWidth, indicatorRect.sizeDelta.y);
            }
            return;
        }

        isAnimating = true;
        int previousIndex = currentSelectedIndex;
        currentSelectedIndex = index;

        if (animationSequence != null && animationSequence.IsActive()) animationSequence.Kill();
        animationSequence = DOTween.Sequence();

        if (indicatorRect != null)
        {
            int direction = (previousIndex < index) ? 1 : -1;

            for (int i = previousIndex; i != index; i += direction)
            {
                int fromIndex = i;
                int toIndex = i + direction;
                bool isFinalStep = (toIndex == index);

                float moveDuration = isFinalStep ? finalSlideDuration : slideStepDuration;
                Ease moveEase = isFinalStep ? finalSlideEase : slideEase;

                // 1. Đổi màu
                animationSequence.AppendCallback(() =>
                {
                    buttonIcons[fromIndex].color = unselectedIconColor;
                    buttonIcons[toIndex].color = originalIconColors[toIndex];
                });

                // 2. Di chuyển indicator
                animationSequence.Append(indicatorRect.DOAnchorPosX(targetButtonPositions[toIndex].x, moveDuration).SetEase(moveEase));

                // 3. Di chuyển và scale icon, text cũ 
                animationSequence.Join(buttonIcons[fromIndex].rectTransform.DOAnchorPos(targetButtonPositions[fromIndex], moveDuration));
                animationSequence.Join(buttonIcons[fromIndex].transform.DOScale(1f, moveDuration));
                animationSequence.Join(iconTexts[fromIndex].DOFade(0f, moveDuration));


                // 4. Di chuyển và scale icon, text mới
                Vector2 hopPosition = targetButtonPositions[toIndex];
                hopPosition.y += iconHopHeight;
                animationSequence.Join(buttonIcons[toIndex].rectTransform.DOAnchorPos(hopPosition, moveDuration));
                animationSequence.Join(buttonIcons[toIndex].transform.DOScale(iconScaleMultiplier, moveDuration));
                animationSequence.Join(iconTexts[toIndex].DOFade(1f, moveDuration));
            }

            animationSequence.Join(indicatorRect.DOSizeDelta(new Vector2(expandedWidth, indicatorRect.sizeDelta.y), resizeDuration).SetEase(Ease.OutBack));
            animationSequence.Join(buttonRects[previousIndex].DOSizeDelta(new Vector2(contractedWidth, buttonRects[previousIndex].sizeDelta.y), resizeDuration).SetEase(slideEase));
            animationSequence.Join(buttonRects[index].DOSizeDelta(new Vector2(expandedWidth, buttonRects[index].sizeDelta.y), resizeDuration).SetEase(slideEase));
        }

        animationSequence.OnComplete(() => isAnimating = false);
    }

    private Vector2[] GetButtonTargetPositions(int selectedIndex)
    {
        int buttonCount = buttonRects.Count;
        Vector2[] positions = new Vector2[buttonCount];
        float currentX = buttonLayoutGroup.padding.left;
        float constantY = buttonRects.Count > 0 ? buttonRects[0].anchoredPosition.y : 0;

        for (int i = 0; i < buttonCount; i++)
        {
            float buttonWidth = (i == selectedIndex) ? expandedWidth : contractedWidth;
            float posX = currentX + (buttonWidth / 2f);
            positions[i] = new Vector2(posX, constantY + iconYOffset);
            currentX += buttonWidth + buttonLayoutGroup.spacing;
        }

        float holderOffsetX = -buttonLayoutGroup.GetComponent<RectTransform>().rect.width * buttonLayoutGroup.GetComponent<RectTransform>().pivot.x;
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i].x += holderOffsetX;
        }

        return positions;
    }
    public void ResetToDefaultState(bool animate)
    {
        SelectButton(defaultSelectedButtonIndex, animate);
    }

    public void SetActiveQuestNoti(bool isActive)
    {
        questNoti.gameObject.SetActive(isActive);
    }
}