using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Step3 : TutorialStep
{
    [Header("UI Elements")]
    [SerializeField] private Image mainImage;
    [SerializeField] private Image handIcon;
    [SerializeField] private GameObject highlightImage;

    [Header("Sprites for this Step")]
    [SerializeField] private List<Sprite> stepSprites;

    [Header("Positions")]
    [SerializeField] private RectTransform pos1;
    [SerializeField] private RectTransform pos2;
    [SerializeField] private Vector2 handOffset;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private float pressDuration = 0.2f;
    [SerializeField] private float pressScale = 0.85f;

    protected override void CreateAnimation()
    {
        Vector3 targetPos1 = pos1.position + (Vector3)handOffset;
        Vector3 targetPos2 = pos2.position + (Vector3)handOffset;

        animationSequence = DOTween.Sequence();

        animationSequence
            .AppendCallback(() => {
                highlightImage.SetActive(false);
                mainImage.sprite = stepSprites[0];
            })

             // move to Pos1 + Highlight
            .Append(handIcon.rectTransform.DOMove(targetPos1, moveDuration))
            .Append(handIcon.transform.DOScale(pressScale, pressDuration).SetLoops(2, LoopType.Yoyo))
            .AppendCallback(() => highlightImage.SetActive(true))
            .AppendInterval(0.8f)

            // move to Pos2 + Highlight
            .Append(handIcon.rectTransform.DOMove(targetPos2, moveDuration))
            .AppendInterval(0.3f)
            .Append(handIcon.transform.DOScale(pressScale, pressDuration).SetLoops(2, LoopType.Yoyo))

            .AppendCallback(() => {
                if (stepSprites.Count > 0 && stepSprites[1] != null)
                {
                    mainImage.sprite = stepSprites[1];
                }
            })
            .AppendInterval(0.5f)

            .SetLoops(-1, LoopType.Restart);
    }
    public override void EndStep()
    {
        Debug.Log("endstep3");
        base.EndStep();
        handIcon.transform.localScale = Vector3.one;
        if (highlightImage != null)
        {
            highlightImage.SetActive(false);
        }
    }
}
