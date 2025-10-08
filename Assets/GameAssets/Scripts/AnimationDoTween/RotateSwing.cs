using UnityEngine;
using DG.Tweening;

public class RotateSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingAngle = 15f;         // Góc xoay trái/phải
    public float duration = 0.5f;          // Thời gian xoay 1 chiều
    public float delayAfterFullSwing = 0.3f; // Delay sau khi xoay trái-phải

    private Sequence rotateSequence;

    void OnEnable()
    {
        StartSwing();
    }

    void OnDisable()
    {
        rotateSequence?.Kill();
    }

    void StartSwing()
    {
        // Reset góc ban đầu
        transform.rotation = Quaternion.Euler(0, 0, -swingAngle);

        // Tạo sequence swing
        rotateSequence = DOTween.Sequence();

        // Xoay từ trái sang phải (liền nhau)
        rotateSequence.Append(
            transform.DORotate(new Vector3(0, 0, swingAngle), duration)
                .SetEase(Ease.InOutSine)
        );

        rotateSequence.Append(
            transform.DORotate(new Vector3(0, 0, -swingAngle), duration)
                .SetEase(Ease.InOutSine)
        );

        // Delay sau 1 cặp swing (nghiêng trái - nghiêng phải)
        rotateSequence.AppendInterval(delayAfterFullSwing);

        rotateSequence.SetLoops(-1);
    }
}
