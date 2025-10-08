using UnityEngine;
using DG.Tweening;

public class BoxAnimation : MonoBehaviour
{
    [Header("Object References")]
    public RectTransform wholeBox;
    public RectTransform boxLid;
    public RectTransform boxBody;
    public RectTransform coin;

    [Header("Shake Settings")]
    public float shakeDuration = 1f;
    public float shakeStrength = 10f;
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f;

    [Header("Lid Open Settings")]
    public float lidUpAmount = 50f;
    public float lidRightAmount = 150f;
    public float lidDownAmount = 20f;
    public float lidRotation = 45f;
    public float lidUpDuration = 0.3f;
    public float lidSideDuration = 0.4f;
    public float lidSettleDuration = 0.5f;

    [Header("Coin Settings")]
    public float coinFlyHeight = 200f;
    public float coinFlyDuration = 0.5f;
    public float coinArcHeight = 50f;

    private Vector3 lidStartPosition;
    private Vector3 coinStartPosition;
    private Sequence winSequence;

    void Awake()
    {
        lidStartPosition = boxLid.localPosition;
        coinStartPosition = coin.localPosition;
    }

    void OnEnable()
    {
        PlayWinAnimation();
    }

    void ResetAnimationState()
    {
        if (winSequence != null && winSequence.IsActive())
        {
            winSequence.Kill();
        }

        wholeBox.gameObject.SetActive(true);
        boxLid.gameObject.SetActive(false);
        boxBody.gameObject.SetActive(false);
        coin.gameObject.SetActive(false);

        // Reset 
        boxLid.localPosition = lidStartPosition;
        boxLid.localRotation = Quaternion.identity;
        coin.localPosition = coinStartPosition;
        coin.localRotation = Quaternion.identity;
        wholeBox.localRotation = Quaternion.identity;
    }

    public void PlayWinAnimation()
    {
        ResetAnimationState();

        winSequence = DOTween.Sequence();

        // 1. Lắc hộp
        winSequence.Append(
            wholeBox.DOShakeRotation(
                shakeDuration, new Vector3(0, 0, shakeStrength),
                shakeVibrato, shakeRandomness, false, ShakeRandomnessMode.Harmonic)
        );

        // Chuẩn bị mở hộp
        winSequence.AppendCallback(() => {
            wholeBox.gameObject.SetActive(false);
            boxLid.gameObject.SetActive(true);
            boxBody.gameObject.SetActive(true);
        });

        // 2. Nắp hộp bay lên
        winSequence.Append(
            boxLid.DOLocalMoveY(lidStartPosition.y + lidUpAmount, lidUpDuration).SetEase(Ease.OutSine)
        );

        // 3. Nắp hộp di chuyển sang phải
        winSequence.Append(
            boxLid.DOLocalMoveX(lidStartPosition.x + lidRightAmount, lidSideDuration).SetEase(Ease.InOutSine)
        );

        // 4. CÙNG LÚC xoay và di chuyển xuống
        winSequence.Append(
            boxLid.DOLocalMoveY(boxLid.localPosition.y - lidDownAmount, lidSettleDuration).SetEase(Ease.InQuad)
        );
        winSequence.Join(
            boxLid.transform.DORotate(new Vector3(0, 0, lidRotation), lidSettleDuration).SetEase(Ease.InQuad)
        );

        // 5. Coin bay lên
        winSequence.AppendCallback(() => coin.gameObject.SetActive(true));
        Vector3 endPosition = coin.localPosition + new Vector3(0, coinFlyHeight, 0);
        winSequence.Append(
            coin.DOLocalJump(endPosition, coinArcHeight, 1, coinFlyDuration).SetEase(Ease.OutQuad)
        );
        winSequence.Join(
            coin.transform.DORotate(new Vector3(0, 360, 0), coinFlyDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear)
        );

        // Kết thúc
        winSequence.OnComplete(() => {
            Debug.Log("Anim Completed.");
        });
    }
}