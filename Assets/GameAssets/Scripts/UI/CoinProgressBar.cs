using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoinProgressBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image fillImage;

    [Header("Fill Sprites")]
    [SerializeField] private Sprite roundEndsSprite;
    [SerializeField] private Sprite squareEndSprite;

    [Header("Settings")]
    public float animationDuration = 0.5f;
    [SerializeField][Range(0, 1)] private float spriteSwapThreshold = 0.9f;

    private float currentProgress = 0f;
    private readonly float stepValue = 0.2f;

    void Start()
    {
        progressBar.value = currentProgress;
        //UpdateProgressBar();
    }

    public bool IncreaseProgress()
    {
        float newProgress = currentProgress + stepValue;
        currentProgress = Mathf.Min(newProgress, 1f);
        UpdateProgressBar();
        return currentProgress >= 1f;
    }

    public void ResetProgress()
    {
        currentProgress = 0f;
        progressBar.DOValue(0, animationDuration).OnComplete(() =>
        {
            fillImage.sprite = squareEndSprite;
        });
    }

    private void UpdateProgressBar()
    {
        fillImage.sprite = squareEndSprite;

        progressBar.DOValue(currentProgress, animationDuration)
            .OnUpdate(() =>
            {
                if (currentProgress >= 1f && progressBar.value >= spriteSwapThreshold)
                {
                    fillImage.sprite = roundEndsSprite;
                }
            })
            .OnComplete(() => {
                if (currentProgress >= 1f)
                {
                    fillImage.sprite = roundEndsSprite;
                }
            });
    }
    public float CurrentProgress => currentProgress;
    public void SetProgress(float progress)
    {
        currentProgress = progress;
        progressBar.value = progress;
        if (currentProgress >= 1f)
        {
            fillImage.sprite = roundEndsSprite;
        }
        else
        {
            fillImage.sprite = squareEndSprite;
        }
    }

}