using UnityEngine;

public class CarouselScaler : MonoBehaviour
{
    private HorizontalCarouselLoop carousel;
    [SerializeField] private ScrollRectLocker scrollRectLocker;
    public float minScale = 0.8f;   // scale nhỏ nhất ở rìa
    public float maxScale = 1.2f;   // scale lớn nhất ở trung tâm
    public float falloff = 2f;      // độ dốc thu nhỏ theo khoảng cách (càng lớn thì càng gắt)

    void Awake()
    {
        carousel = GetComponent<HorizontalCarouselLoop>();
    }

    void Start()
    {
        UpdateScale();
    }

    void Update()
    {
        if (carousel == null || carousel.items == null) return;
        if (!carousel.snapping && !scrollRectLocker.draggingHorizontal && !carousel.isDragging) return;
        UpdateScale();
    }

    private void UpdateScale()
    {
        foreach (var item in carousel.items)
        {
            // khoảng cách từ item đến tâm (0)
            float dist = Mathf.Abs(item.anchoredPosition.x);

            // chuẩn hóa khoảng cách (0 = tâm, 1 = nửa màn)
            float t = Mathf.Clamp01(dist / (carousel.items[0].rect.width));

            // scale giảm dần từ max -> min
            float scale = Mathf.Lerp(maxScale, minScale, Mathf.Pow(t, falloff));

            item.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
