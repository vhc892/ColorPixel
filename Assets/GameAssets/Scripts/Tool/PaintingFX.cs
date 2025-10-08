using System.Linq;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PaintingFX : MonoBehaviour
{
    public static PaintingFX Instance;

    public ParticleSystem fillEffectParticle;
    public ParticleSystem fireworkParticle;
    public ParticleSystem colorFinishParticle;
    private ParticleSystem subParticle;

    public SpriteRenderer background;
    public Color fadeToColor = new Color(0.2f, 0.2f, 0.2f);

    private Color originalBackgroundColor;
    private Coroutine fireworkCoroutine;



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            var allChildrenParticles = fireworkParticle.GetComponentsInChildren<ParticleSystem>();
            subParticle = allChildrenParticles.FirstOrDefault(p => p != fireworkParticle);
            if (subParticle == null)
            {
                Debug.LogWarning("can not find sub-particle for firework!");
            }
        }
        else { Destroy(gameObject); }
    }
    private void Start()
    {
        if (background != null)
        {
            originalBackgroundColor = background.color;
        }
    }

    public void PlayColorFinishEffect(Vector2Int pixelCoords)
    {
        if (colorFinishParticle == null) return;

        Vector3 worldPos = CoreGameManager.Instance.PixelToWorld(pixelCoords);
        colorFinishParticle.transform.position = worldPos;

        // Scale cả vfx
        float t = Mathf.InverseLerp(InputHandler.Instance.GetZoomInSize(), InputHandler.Instance.GetZoomOutSize(), Camera.main.orthographicSize);
        float newScale = Mathf.Lerp(0.05f, 0.4f, t);

        SetColorFinishEffectScale(newScale);

        colorFinishParticle.Play();
        AudioManager.Instance.CompleteColorSfx();
    }

    public void SetColorFinishEffectScale(float scale)
    {
        if (colorFinishParticle == null) return;
        foreach (Transform child in colorFinishParticle.transform)
        {
            child.localScale = Vector3.one * scale;
        }
    }


    public void PlayFillEffect(Vector2Int pixelCoords)
    {
        if (fillEffectParticle == null) return;

        Vector3 worldPos = CoreGameManager.Instance.PixelToWorld(pixelCoords);

        fillEffectParticle.transform.position = worldPos;
        fillEffectParticle.Emit(1);
    }
    public void PlayFirework(Color color)
    {
        if (background == null)
        {
            PlayActualParticles(color);
            return;
        }

        if (fireworkCoroutine != null)
        {
            StopCoroutine(fireworkCoroutine);
            background.color = originalBackgroundColor;
        }

        fireworkCoroutine = StartCoroutine(FireworkSequence(color));
    }

    private IEnumerator FireworkSequence(Color particleColor)
    {
        try
        {
            PlayerManager.Instance.IsPlayingAnim = true;

            float fadeDuration = 0.5f;
            //fade
            background.DOColor(fadeToColor, fadeDuration);

            yield return new WaitForSeconds(fadeDuration / 2);

            PlayActualParticles(particleColor);

            yield return new WaitForSeconds(GetFireworkDuration());
            fireworkParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            background.DOColor(originalBackgroundColor, fadeDuration);
        }
        finally
        {
            PlayerManager.Instance.IsPlayingAnim = false;
            fireworkCoroutine = null;
        }
    }

    //play
    private void PlayActualParticles(Color color)
    {
        if (fireworkParticle == null) return;
        var fireworkMain = fireworkParticle.main;
        fireworkMain.startColor = color;
        if (subParticle != null)
        {
            var subParticleMain = subParticle.main;
            subParticleMain.startColor = color;
        }
        fireworkParticle.Play();
    }
    // total time
    public float GetFireworkDuration()
    {
        if (fireworkParticle == null) return 0f;
        var fireworkMain = fireworkParticle.main;
        float totalDuration = fireworkMain.duration + fireworkMain.startLifetime.constantMax;
        if (subParticle != null)
        {
            var subParticleMain = subParticle.main;
            totalDuration += subParticleMain.startLifetime.constantMax;
        }
        return totalDuration;
    }
}