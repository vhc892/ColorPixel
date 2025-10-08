using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup loadingCanvasGroup;
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    [Header("Scene Settings")]
    public string sceneToLoad = "MainScene";

    [Header("Loading Settings")]
    public float fadeDuration = 0.5f;
    public float minLoadingTime = 1.5f;

    void Start()
    {
        loadingCanvasGroup.alpha = 1f;
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        float elapsedTime = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        operation.allowSceneActivation = false;

        float targetProgress = 0f;

        while (operation.progress < 0.9f || elapsedTime < minLoadingTime)
        {
            elapsedTime += Time.deltaTime;
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (elapsedTime < minLoadingTime)
            {
                targetProgress = Mathf.Min(targetProgress, elapsedTime / minLoadingTime);
            }
            progressBar.DOValue(targetProgress, 0.2f);
            progressText.text = (targetProgress * 100f).ToString("F0") + "%";

            yield return null; 
        }
        progressBar.DOValue(1f, 0.2f);
        progressText.text = "100%";

        yield return new WaitForSeconds(0.3f);

        operation.allowSceneActivation = true;

        yield return new WaitUntil(() => operation.isDone);

        loadingCanvasGroup.DOFade(0f, fadeDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                SceneManager.UnloadSceneAsync("LoadingScene");
            });
    }
}