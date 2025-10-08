using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public class InfiniteScrollRect : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform viewportTransform;
    private RectTransform contentPanelTransform;
    private HorizontalLayoutGroup hlg;

    private List<RectTransform> items = new List<RectTransform>();

    private bool isUpdate = false;
    private Vector2 currentVelocity = Vector2.zero;

    // ----- SCALE SETTINGS -----
    public float maxScale = 1.2f;
    public float minScale = 0.8f;
    public float scaleDistance = 200f;
    public bool smoothScale = false;
    public float scaleSpeed = 10f;

    private RectTransform closestItem;
    private Vector3[] viewportCorners = new Vector3[4];

    // ----- SNAP SETTINGS -----
    public bool enableSnap = true;
    public float snapSpeed = 10f;
    public float velocityThreshold = 50f; // ngưỡng để coi là "quét sang item mới"

    private bool isDragging = false;
    private RectTransform targetSnapItem;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        viewportTransform = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();
        contentPanelTransform = scrollRect.content;
        hlg = contentPanelTransform.GetComponent<HorizontalLayoutGroup>();

        foreach (RectTransform child in contentPanelTransform)
        {
            items.Add(child);
        }
    }

    void Start()
    {
        // duplicate items trước & sau để loop
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform RT = Instantiate(items[i % items.Count], contentPanelTransform);
            RT.SetAsLastSibling();
        }

        for (int i = 0; i < items.Count; i++)
        {
            int num = items.Count - i - 1;
            RectTransform RT = Instantiate(items[num % items.Count], contentPanelTransform);
            RT.SetAsFirstSibling();
        }

        // set vị trí ban đầu
        contentPanelTransform.localPosition = new Vector3(
            -(items[0].rect.width + hlg.spacing) * items.Count,
            contentPanelTransform.localPosition.y,
            contentPanelTransform.localPosition.z
        );

        ForceInitScale();
    }

    void Update()
    {
        HandleLoop();
        UpdateScale();

        if (enableSnap)
            HandleSnap();

        // detect drag start
        if (Input.GetMouseButtonDown(0))
            isDragging = true;

        // detect drag end
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            DecideSnapTarget();
        }
    }

    private void HandleLoop()
    {
        if (isUpdate)
        {
            isUpdate = false;
            scrollRect.velocity = currentVelocity;
        }

        float itemWidth = items[0].rect.width + hlg.spacing;
        float threshold = items.Count * itemWidth;

        if (contentPanelTransform.localPosition.x > 0)
        {
            Canvas.ForceUpdateCanvases();
            currentVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition -= new Vector3(threshold, 0, 0);
            isUpdate = true;
            targetSnapItem = null; // huỷ snap nếu bị teleport
            ForceInitScale();
        }
        else if (contentPanelTransform.localPosition.x < -threshold)
        {
            Canvas.ForceUpdateCanvases();
            currentVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition += new Vector3(threshold, 0, 0);
            isUpdate = true;
            targetSnapItem = null; // huỷ snap nếu bị teleport
            ForceInitScale();
        }
    }


    private void ForceInitScale()
    {
        Vector3 viewportCenter = GetViewportCenterWorld();
        int childCount = scrollRect.content.childCount;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform item = (RectTransform)scrollRect.content.GetChild(i);
            float d = Mathf.Abs(item.position.x - viewportCenter.x);

            float t = Mathf.Clamp01(d / scaleDistance);
            float targetScale = Mathf.Lerp(maxScale, minScale, t);
            item.localScale = Vector3.one * targetScale;
        }
    }

    private void UpdateScale()
    {
        Vector3 viewportCenter = GetViewportCenterWorld();
        closestItem = null;
        float closestDist = float.MaxValue;

        int childCount = scrollRect.content.childCount;
        for (int i = 0; i < childCount; i++)
        {
            RectTransform item = (RectTransform)scrollRect.content.GetChild(i);
            float d = Mathf.Abs(item.position.x - viewportCenter.x);

            float t = Mathf.Clamp01(d / scaleDistance);
            float targetScale = Mathf.Lerp(maxScale, minScale, t);

            if (smoothScale)
                item.localScale = Vector3.Lerp(item.localScale, Vector3.one * targetScale, Time.deltaTime * scaleSpeed);
            else
                item.localScale = Vector3.one * targetScale;

            if (d < closestDist)
            {
                closestDist = d;
                closestItem = item;
            }
        }
    }

    private void DecideSnapTarget()
    {
        if (Mathf.Abs(scrollRect.velocity.x) > velocityThreshold)
        {
            bool toRight = scrollRect.velocity.x < 0; 
            targetSnapItem = FindNextItem(toRight);
        }
        else
        {
            targetSnapItem = closestItem;
        }

        scrollRect.velocity = Vector2.zero; // reset sau khi chọn target
    }


    private void HandleSnap()
    {
        if (targetSnapItem == null) return;

        Vector3 viewportCenter = GetViewportCenterWorld();
        float offset = viewportCenter.x - targetSnapItem.position.x;

        Vector3 targetPos = contentPanelTransform.position + new Vector3(offset, 0, 0);
        contentPanelTransform.position = Vector3.Lerp(
            contentPanelTransform.position,
            targetPos,
            Time.deltaTime * snapSpeed
        );

        // khi đã rất gần thì kết thúc snap
        if (Mathf.Abs(offset) < 0.5f)
        {
            contentPanelTransform.position = targetPos;
            scrollRect.velocity = Vector2.zero;   // reset velocity
            targetSnapItem = null;                // clear snap target
        }
    }



    private RectTransform FindNextItem(bool toRight)
    {
        if (closestItem == null) return null;

        int index = closestItem.GetSiblingIndex();
        int nextIndex = toRight ? index + 1 : index - 1;
        nextIndex = Mathf.Clamp(nextIndex, 0, scrollRect.content.childCount - 1);

        return (RectTransform)scrollRect.content.GetChild(nextIndex);
    }

    private Vector3 GetViewportCenterWorld()
    {
        viewportTransform.GetWorldCorners(viewportCorners);
        return (viewportCorners[0] + viewportCorners[2]) * 0.5f;
    }

    public RectTransform GetClosestItem()
    {
        return closestItem;
    }
}
