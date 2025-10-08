using UnityEngine;
using DG.Tweening;

public class ScalePingPong : MonoBehaviour
{
    [Header("Scale Settings")]
    public Vector3 minScale = Vector3.one * 0.8f;
    public Vector3 maxScale = Vector3.one * 1.2f;
    public float duration = 0.5f;

    [Header("Delay Settings")]
    public float initDelay = 0f; // Thời gian trễ khi bắt đầu

    private Tween scaleTween;

    void OnEnable()
    {
        PlayScaleAnimation();
    }

    void PlayScaleAnimation()
    {
        // Reset scale về min trước khi bắt đầu
        transform.localScale = minScale;

        // Tạo tween scale qua lại giữa min và max, delay theo initDelay
        scaleTween = transform
            .DOScale(maxScale, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(initDelay);
    }

    void OnDisable()
    {
        scaleTween?.Kill();
    }
}
