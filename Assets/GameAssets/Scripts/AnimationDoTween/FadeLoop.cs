using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class UIFadeLoop : MonoBehaviour
{
    public float minAlpha = 0.1f;
    public float maxAlpha = 1f;
    public float duration = 1f;

    private Image image;
    private SpriteRenderer spriteRenderer;
    private Tween fadeTween;

    void OnEnable()
    {
        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartFadeLoop();
    }

    void OnDisable()
    {
        fadeTween?.Kill();
    }

    void OnDestroy()
    {
        fadeTween?.Kill();
    }

    void StartFadeLoop()
    {
        if (image)
        {
            Color color = image.color;
            color.a = minAlpha;
            image.color = color;
            fadeTween = image.DOFade(maxAlpha, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        }
        else if (spriteRenderer)
        {
            Color color = spriteRenderer.color;
            color.a = minAlpha;
            spriteRenderer.color = color;
            fadeTween = spriteRenderer.DOFade(maxAlpha, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        }
            
    }
}
