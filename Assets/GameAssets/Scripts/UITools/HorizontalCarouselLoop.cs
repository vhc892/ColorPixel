using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class HorizontalCarouselLoop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform[] items;     // danh sách item
    public int displayAmount = 1;     // số item hiển thị (1 = trung tâm, 3 = trái-trung-phải, ...)
    
    [Range(0f, 1f)]public float scrollSpeed = 1f;  // tốc độ scroll 
    public float snapSpeed = 10f;     // tốc độ snap
    private float snapVelocity = 0f;

    private float itemWidth;
    private Vector2 dragStartPos;

    private Vector2 lastDragPos;
    public bool snapping = false;
    private float snapOffset;
    public int currentItem;

    [HideInInspector] public bool isDragging;
    [HideInInspector] public float DragDelta;

    public static HorizontalCarouselLoop activeLoop = null;

    void Awake()
    {
        if (activeLoop == null && name == "ScrollArtBoxContainer") activeLoop = this;
        Debug.Log(activeLoop);
        if (displayAmount + 2 > items.Length) Debug.LogError("Cant display loop");
        if (items.Length == 0) return;
        itemWidth = items[0].rect.width;
        currentItem = 0;
        PositionItems();
    }

    void Update()
    {
        if (snapping)
        {
            float newOffset = Mathf.SmoothDamp(snapOffset, 0, ref snapVelocity, 0.08f);
            float move = snapOffset - newOffset; // phần delta cần scroll
            Scroll(move);
            snapOffset = newOffset;

            if (Mathf.Abs(snapOffset) <= 1f)
            {
                Scroll(snapOffset); // kéo nốt phần dư
                HandleLoopOnEnd();
                snapOffset = 0;
                snapping = false;
                if (activeLoop == this) DatabaseManager.Instance.UpdateBothSideArtBoxContainer();
                if (activeLoop == this) DatabaseManager.Instance.UpdateBothSideCategoryContainer();
            }
        }
    }

    void PositionItems()
    {
        // Bố trí item theo bề rộng
        for (int i = 0; i < items.Length; i++)
        {
            SetPos(items[i], i * itemWidth);
        }
        HandleLoop();
    }

    public void Scroll(float deltaX)
    {
        foreach (var rt in items)
            rt.anchoredPosition += new Vector2(deltaX, 0);

        HandleLoop();
    }

    void HandleLoop()
    {
        float halfWidth = (items.Length - 1) * itemWidth * 0.5f;

        foreach (var rt in items)
        {
            if (rt.anchoredPosition.x > halfWidth)
                rt.anchoredPosition -= new Vector2(itemWidth * items.Length, 0);
            else if (rt.anchoredPosition.x < -halfWidth)
                rt.anchoredPosition += new Vector2(itemWidth * items.Length, 0);
        }
    }

    void HandleLoopOnEnd()
    {
        if (items.Length == 0) return;

        float centerX = 0f; // luôn muốn currentItem ở giữa

        for (int i = 0; i < items.Length; i++)
        {
            // offset trong vòng lặp
            int offset = (i - currentItem + items.Length) % items.Length;

            // nếu offset lớn hơn nửa danh sách thì đi vòng ngược lại
            if (offset > items.Length / 2)
                offset -= items.Length;

            float x = centerX + offset * itemWidth;
            items[i].anchoredPosition = new Vector2(x, items[i].anchoredPosition.y);
        }
    }


    void SetPos(RectTransform rect, float x)
    {
        rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
    }

    // ----- DRAG -----
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (activeLoop.snapping) return;
        dragStartPos = eventData.position;
        lastDragPos = eventData.position;
        isDragging = true;
        DragDelta = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector2 delta = eventData.position - lastDragPos;
        Scroll(delta.x * scrollSpeed);   // chỉ di chuyển content
        DragDelta = delta.x * scrollSpeed;
        lastDragPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;
        DragDelta = 0;

        float dragDir = eventData.position.x - dragStartPos.x;

        // Ngưỡng swipe (30% chiều rộng item)
        float threshold = itemWidth * 0.3f;

        if (Mathf.Abs(dragDir) > threshold)
        {
            if (dragDir < 0)
            {
                if (activeLoop == this) DatabaseManager.Instance.currentConceptIndex++;
            }
            else
            {
                if (activeLoop == this) DatabaseManager.Instance.currentConceptIndex--;
            }
        }

        currentItem = ((DatabaseManager.Instance.currentConceptIndex % items.Length) + items.Length) % items.Length;
        SnapTo(DatabaseManager.Instance.currentConceptIndex);
    }


    public void SnapTo(int targetConceptIndex)
    {
        if (items.Length == 0) return;

        // update concept index gốc
        DatabaseManager.Instance.currentConceptIndex = targetConceptIndex;

        // tính index local để snap
        int targetIndex = ((targetConceptIndex % items.Length) + items.Length) % items.Length;
        RectTransform target = items[targetIndex];
        if (target == null) return;

        snapOffset = -target.anchoredPosition.x;
        snapping = true;

        currentItem = targetIndex;
    }

    public void SnapLeft()
    {
        if (snapping) return;
        if (items.Length == 0) return;

        // Giảm index (sang trái)
        if (activeLoop == this)DatabaseManager.Instance.currentConceptIndex--;
        currentItem = ((DatabaseManager.Instance.currentConceptIndex % items.Length) + items.Length) % items.Length;

        SnapTo(DatabaseManager.Instance.currentConceptIndex);
    }

    public void SnapRight()
    {
        if (snapping) return;
        if (items.Length == 0) return;

        // Tăng index (sang phải)
        if (activeLoop == this)DatabaseManager.Instance.currentConceptIndex++;
        currentItem = ((DatabaseManager.Instance.currentConceptIndex % items.Length) + items.Length) % items.Length;

        SnapTo(DatabaseManager.Instance.currentConceptIndex);
    }


    // ----- API phụ -----
    /// <summary>
    /// Lấy item đang ở giữa màn hình
    /// </summary>
    public RectTransform GetCenterItem()
    {
        RectTransform closest = null;
        float closestDist = float.MaxValue;

        foreach (var rt in items)
        {
            float d = Mathf.Abs(rt.anchoredPosition.x);
            if (d < closestDist)
            {
                closestDist = d;
                closest = rt;
            }
        }

        return closest;
    }
}
