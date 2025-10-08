using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class Step1 : TutorialStep
{
    [Header("UI Elements")]
    [SerializeField] private Image mainImage;
    [SerializeField] private Image handIcon;
    [SerializeField] private GameObject highlightImage;
    [SerializeField] private Transform paintImage;

    [Header("Sprites for this Step")]
    [SerializeField] private List<Sprite> stepSprites;

    [Header("Positions")]
    [SerializeField] private RectTransform pos1;
    [SerializeField] private RectTransform pos2;
    [SerializeField] private RectTransform pos3;
    [SerializeField] private RectTransform pos4;
    [SerializeField] private Vector2 handOffset;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private float pressDuration = 0.2f;
    [SerializeField] private float pressScale = 0.85f;

    protected override void CreateAnimation()
    {
        Vector3 targetPos1 = pos1.position + (Vector3)handOffset;
        Vector3 targetPos2 = pos2.position + (Vector3)handOffset;
        Vector3 targetPos3 = pos3.position + (Vector3)handOffset;
        Vector3 targetPos4 = pos4.position + (Vector3)handOffset;

        handIcon.rectTransform.position = targetPos4;

        animationSequence = DOTween.Sequence();

        animationSequence
            .AppendCallback(() =>
            {
                highlightImage.SetActive(false);
                paintImage.gameObject.SetActive(false);
                mainImage.sprite = stepSprites[0];
            })

            // move Pos1 and Highlight
            .Append(handIcon.rectTransform.DOMove(targetPos1, moveDuration))
            .Append(handIcon.transform.DOScale(pressScale, pressDuration).SetLoops(2, LoopType.Yoyo))
            .AppendCallback(() =>
            {
                highlightImage.SetActive(true);
            })
            .AppendInterval(0.8f)

            // move Pos2 + change sprite
            .Append(handIcon.rectTransform.DOMove(targetPos2, moveDuration))
            .AppendInterval(0.3f)
            .JoinCallback(() =>
            {
                paintImage.transform.position = pos2.position;
                paintImage.gameObject.SetActive(true);
                paintImage.transform.DOScale(1f, 0.2f).From(0).SetEase(Ease.OutBack);
            })
            .AppendCallback(() =>
            {
                if (stepSprites.Count > 0 && stepSprites[1] != null)
                {
                    mainImage.sprite = stepSprites[1];
                }
            })

            // move Pos3 + change sprite
            .Append(handIcon.rectTransform.DOMove(targetPos3, 0.5f))
            .Join(paintImage.transform.DOMove(pos3.position, 0.5f))
            .AppendCallback(() =>
            {
                if (stepSprites.Count > 0 && stepSprites[2] != null)
                {
                    mainImage.sprite = stepSprites[2];
                }
            })

            // move Pos4 + change sprite
            .Append(handIcon.rectTransform.DOMove(targetPos4, 0.5f))
            .Join(paintImage.transform.DOMove(pos4.position, 0.5f))
            .AppendCallback(() => {
                if (stepSprites.Count > 1 && stepSprites[3] != null)
                {
                    mainImage.sprite = stepSprites[3];
                }
            })
            .AppendInterval(0.5f)

            .SetLoops(-1, LoopType.Restart);
    }

    public override void EndStep()
    {
        Debug.Log("endstep1");
        base.EndStep();
        handIcon.transform.localScale = Vector3.one;
        if (highlightImage != null)
        {
            highlightImage.SetActive(false);
        }
    }
}