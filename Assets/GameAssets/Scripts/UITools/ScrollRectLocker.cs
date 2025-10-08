using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectLocker : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public static ScrollRectLocker Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public ScrollRect horizontalScroll;
    public HorizontalCarouselLoop[] horizontalCarouselLoops;
    public ScrollRect verticalScroll;




    [HideInInspector] public bool draggingHorizontal;
    [HideInInspector] public bool draggingVertical;

    void Start()
    {
        verticalScroll = horizontalCarouselLoops[0].items[horizontalCarouselLoops[0].currentItem].GetComponent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        float absX = Mathf.Abs(eventData.delta.x);
        float absY = Mathf.Abs(eventData.delta.y);

        // Xác định hướng vuốt ban đầu
        if (absX > absY)
        {
            draggingHorizontal = true;
            draggingVertical = false;
            foreach (var horizontalCarouselLoop in horizontalCarouselLoops)
            {
                horizontalCarouselLoop.OnBeginDrag(eventData);
            }

            horizontalScroll?.OnBeginDrag(eventData);
        }
        else
        {
            draggingHorizontal = false;
            draggingVertical = true;
            verticalScroll.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingHorizontal)
        {
            foreach (var horizontalCarouselLoop in horizontalCarouselLoops)
            {
                horizontalCarouselLoop.OnDrag(eventData);
            }
            horizontalScroll?.OnDrag(eventData);
        }
        else if (draggingVertical)
            verticalScroll.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingHorizontal)
        {
            foreach (var horizontalCarouselLoop in horizontalCarouselLoops)
            {
                horizontalCarouselLoop.OnEndDrag(eventData);
            }
            horizontalScroll?.OnEndDrag(eventData);
        }
        else if (draggingVertical)
            verticalScroll.OnEndDrag(eventData);

        // Reset trạng thái
        draggingHorizontal = draggingVertical = false;

        verticalScroll = horizontalCarouselLoops[0].items[horizontalCarouselLoops[0].currentItem].GetComponent<ScrollRect>();
    }

    public void OnNextCategory()
    {
        AudioManager.Instance.PressButtonSfx();
        foreach (var horizontalCarouselLoop in horizontalCarouselLoops)
        {
            horizontalCarouselLoop.SnapRight();
        }
    }

    public void OnPrevCategory()
    {
        AudioManager.Instance.PressButtonSfx();
        foreach (var horizontalCarouselLoop in horizontalCarouselLoops)
        {
            horizontalCarouselLoop.SnapLeft();
        }
    }
}
