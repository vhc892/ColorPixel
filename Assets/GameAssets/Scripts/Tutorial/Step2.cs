using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class Step2 : TutorialStep
{
    [Header("UI Elements")]
    [SerializeField] private Image mainImage;
    [SerializeField] private Image handIcon;
    [SerializeField] private Image handIcon2;

    [Header("Sprites for this Step")]
    [SerializeField] private List<Sprite> stepSprites;

    [Header("Positions")]
    [SerializeField] private RectTransform pos1;
    [SerializeField] private RectTransform pos2;
    [SerializeField] private Vector2 handOffset;

    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 200f;
    [SerializeField] private float moveDuration = 1.5f;

    private Vector2 originalMainImagePos;
    private bool isOriginalPosStored = false;

    protected override void CreateAnimation()
    {
        if (!isOriginalPosStored)
        {
            originalMainImagePos = mainImage.rectTransform.anchoredPosition;
            isOriginalPosStored = true;
        }
        if (stepSprites.Count > 0 && stepSprites[0] != null)
        {
            mainImage.sprite = stepSprites[0];
        }

        handIcon.rectTransform.position = pos1.position + (Vector3)handOffset;
        handIcon2.rectTransform.position = pos2.position + (Vector3)handOffset;

        animationSequence = DOTween.Sequence();

        animationSequence
            .AppendCallback(() => {

                handIcon.rectTransform.position = pos1.position + (Vector3)handOffset;
                handIcon2.rectTransform.position = pos2.position + (Vector3)handOffset;

                mainImage.rectTransform.anchoredPosition = originalMainImagePos + new Vector2(0, -100);
                mainImage.transform.localScale = Vector3.one * 0.8f;
            })

            // hand1
            .Append(handIcon.rectTransform.DOAnchorPos(
                handIcon.rectTransform.anchoredPosition + new Vector2(moveDistance, 0), moveDuration)
                .SetEase(Ease.Linear))

            // hand2
            .Join(handIcon2.rectTransform.DOAnchorPos(
                handIcon2.rectTransform.anchoredPosition + new Vector2(-moveDistance, 0), moveDuration)
                .SetEase(Ease.Linear))

            // scale
            .Join(mainImage.transform.DOScale(1.2f, moveDuration)
                .SetEase(Ease.Linear))

            .AppendInterval(1f)
            .SetLoops(-1, LoopType.Restart);
    }

    public override void EndStep()
    {
        Debug.Log("endstep2");
        base.EndStep();
        if (isOriginalPosStored)
        {
            mainImage.rectTransform.anchoredPosition = originalMainImagePos;
        }
        mainImage.transform.localScale = Vector3.one;
        handIcon.transform.localScale = Vector3.one;
    }
}