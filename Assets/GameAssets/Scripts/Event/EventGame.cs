using DG.Tweening;
using System.Linq;
using UnityEngine;

public class EventGame : MonoBehaviour
{
    public SpriteRenderer event_BG;
    public EventArt[] eventArts;
    public bool fitToScreen = true;

    [Header("Tutorial")]
    public GameObject tutorialHand;
    public GameObject tutorialBackground;
    public int tutorialTargetIndex = 2;
    public Vector3 tutorialHandOffset;
    private Sequence tutorialHandSequence;

    private bool isTutorialActive = false;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnEnable()
    {
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (tutorialBackground != null) tutorialBackground.SetActive(false);

        FitToScreen();
        UpdateAllEventArts();
    }

    public void UpdateAllEventArts()
    {
        foreach (EventArt eventArt in eventArts)
        {
            eventArt.UpdateEventArt();
        }
    }

    void FitToScreen()
    {
        if (event_BG == null) return;
        Camera cam = Camera.main;
        cam.orthographicSize = InputHandler.Instance.GetZoomOutSize();
        cam.transform.position = InputHandler.Instance.GetInitialCameraPosition();
        // Kích thước sprite gốc (chưa scale) trong world
        Vector2 spriteSize = event_BG.sprite.bounds.size;

        // Kích thước camera trong world
        float worldScreenHeight = cam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * cam.aspect;

        // Tỉ lệ scale theo mỗi chiều
        float scaleX = worldScreenWidth / spriteSize.x;
        float scaleY = worldScreenHeight / spriteSize.y;

        // Chọn cách fit
        float finalScale = fitToScreen ? Mathf.Max(scaleX, scaleY) : Mathf.Min(scaleX, scaleY);

        // Apply scale (dựa trên originalScale)
        transform.localScale = originalScale * finalScale;
    }
    
    public bool IsFinished()
    {
        return eventArts
            .AsParallel()            
            .All(eventArt => eventArt.artBoxSO.isDone);
    }
    public void StartTutorial()
    {
        if (tutorialHand == null || tutorialBackground == null || tutorialTargetIndex < 0 || tutorialTargetIndex >= eventArts.Length)
        {
            Debug.LogError("Tutorial objects or target index are not set up correctly!");
            return;
        }

        isTutorialActive = true;
        tutorialBackground.SetActive(true);
        tutorialHand.SetActive(true);

        tutorialHand.transform.position = eventArts[tutorialTargetIndex].transform.position + tutorialHandOffset;
        if (tutorialHandSequence != null)
        {
            tutorialHandSequence.Kill();
        }
        Vector3 originalHandScale = tutorialHand.transform.localScale;
        tutorialHandSequence = DOTween.Sequence();
        tutorialHandSequence.Append(tutorialHand.transform.DOScale(originalHandScale * 0.85f, 0.6f).SetEase(Ease.InOutQuad))
                          .Append(tutorialHand.transform.DOScale(originalHandScale, 0.6f).SetEase(Ease.InOutQuad))
                          .SetLoops(-1);

        for (int i = 0; i < eventArts.Length; i++)
        {
            var collider = eventArts[i].GetComponent<Collider2D>();
            if (collider != null)
            {
                if (i != tutorialTargetIndex)
                {
                    collider.enabled = false;
                }
                else
                {
                    eventArts[i].SetupForTutorial(EndTutorial);
                }
            }
        }
    }
    private void EndTutorial()
    {
        if (!isTutorialActive) return;

        isTutorialActive = false;
        if (tutorialHandSequence != null)
        {
            tutorialHandSequence.Kill();
        }
        tutorialHand.SetActive(false);
        tutorialBackground.SetActive(false);

        foreach (var eventArt in eventArts)
        {
            var collider = eventArt.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }
}
