using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelected : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private float scrollDuration = 0.4f;

    void Awake()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }

    public void CenterOnItem(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();
        float viewportWidth = scrollRect.viewport.rect.width;
        float contentWidth = contentRect.rect.width;

        if (contentWidth <= viewportWidth)
            return;
        // Lấy tâm item trong tọa độ content
        Vector3 itemWorldPos = target.TransformPoint(target.rect.center);
        Vector3 itemLocalPos = contentRect.InverseTransformPoint(itemWorldPos);
        float itemCenterX = itemLocalPos.x;

        // Lấy tâm viewport trong tọa độ content
        Vector3 viewportWorldPos = scrollRect.viewport.TransformPoint(scrollRect.viewport.rect.center);
        Vector3 viewportLocalPos = contentRect.InverseTransformPoint(viewportWorldPos);
        float viewportCenterX = viewportLocalPos.x;

        // delta để item về giữa
        float deltaX = viewportCenterX - itemCenterX;

        // new vị trí content
        float newX = contentRect.anchoredPosition.x + deltaX;

        // clamp để không hở trắng
        float minX = (viewportWidth - contentWidth)/2f;
        float maxX = -(viewportWidth - contentWidth)/2f;
        newX = Mathf.Clamp(newX, minX, maxX);

        // Animate
        contentRect.DOAnchorPosX(newX, scrollDuration).SetEase(Ease.OutCubic);
    }

}
