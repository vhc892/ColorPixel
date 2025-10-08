using UnityEngine;
using DG.Tweening;

public class RotateInPlace2D : MonoBehaviour
{
    [Header("Rotate Settings")]
    public float duration = 1f;       // Thời gian xoay 1 vòng (giây)
    public bool rotateLeft = true;   // Xoay trái (counter-clockwise) hoặc phải

    private Tween rotateTween;

    void OnEnable()
    {
        StartRotate();
    }

    void OnDisable()
    {
        rotateTween?.Kill();
    }

    void StartRotate()
    {
        float endValue = rotateLeft ? 360f : -360f;

        // Reset góc xoay về 0 (nếu muốn)
        transform.rotation = Quaternion.identity;

        // Xoay vòng tại chỗ quanh trục Z
        rotateTween = transform.DORotate(
            new Vector3(0, 0, endValue),
            duration,
            RotateMode.FastBeyond360
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);
    }
}
