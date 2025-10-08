using UnityEngine;
using UnityEngine.UI;

public class CarouselFade : MonoBehaviour
{
    private HorizontalCarouselLoop carousel;
    [SerializeField] private ScrollRectLocker scrollRectLocker;
    public float minAlpha = 0f;
    public float maxAlpha = 1f;
    public Color selectTextColor;
    public Color defaultTextColor;
    public Color selectOutlineColor;
    public Color defaultTextOutlineColor;
    public float falloff = 2f;
    private float width = 0;

    void Awake()
    {
        carousel = GetComponent<HorizontalCarouselLoop>();
        width = carousel.items[0].rect.width;
    }

    void Start()
    {
        UpdateFade();
    }

    void Update()
    {
        if (carousel == null || carousel.items == null) return;
        if (!carousel.snapping && !scrollRectLocker.draggingHorizontal && !carousel.isDragging) return;

        UpdateFade();
    }

    private void UpdateFade()
    {
        foreach (var item in carousel.items)
        {
            CategoryBarUI categoryBarUI = item.GetComponent<CategoryBarUI>();
            // khoảng cách từ item đến tâm (0)
            float dist = Mathf.Abs(item.anchoredPosition.x);

            // chuẩn hóa khoảng cách (0 = tâm, 1 = nửa màn)
            float t = Mathf.Clamp01(dist / width);

            // scale giảm dần từ max -> min
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);
            // Debug.Log(alpha);
            categoryBarUI.selectedBar.color = new Color(categoryBarUI.selectedBar.color.r,
                                                        categoryBarUI.selectedBar.color.g,
                                                        categoryBarUI.selectedBar.color.b, alpha);

            categoryBarUI.selectedTMP.color = new Color(categoryBarUI.selectedTMP.color.r,
                                                categoryBarUI.selectedTMP.color.g,
                                                categoryBarUI.selectedTMP.color.b, alpha);

            categoryBarUI.selectedShadowTMP.color = new Color(categoryBarUI.selectedShadowTMP.color.r,
                                                    categoryBarUI.selectedShadowTMP.color.g,
                                                    categoryBarUI.selectedShadowTMP.color.b, alpha);
        }
    }
}
