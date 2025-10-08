using UnityEngine;
using UnityEngine.UI;

public class AutoScrollPingPong : MonoBehaviour
{
    private RectTransform content; 
    private ScrollRect scrollRect; 
    [SerializeField] private float speed = 100f;

    private float viewportWidth;
    private int direction = 1; // -1 = trái, +1 = phải

    void Start()
    {
        scrollRect = transform.parent.parent.GetComponent<ScrollRect>();
        content = GetComponent<RectTransform>();
        viewportWidth = ((RectTransform)content.parent).rect.width;
    }

    void OnEnable()
    {
        if (content != null)
        {
            content.anchoredPosition = Vector2.zero;
            direction = 1;
        }
        if (scrollRect != null)
        {
            scrollRect.velocity = Vector2.zero;
        }
    }

    void Update()
    {
        // Luôn cập nhật lại width (nếu content thay đổi)
        float contentWidth = content.rect.width;

        if (contentWidth <= viewportWidth)
        {
            content.anchoredPosition = Vector2.zero;
            scrollRect.velocity = Vector2.zero;
            return;
        }

        // Di chuyển content
        content.anchoredPosition += Vector2.right * direction * speed * Time.deltaTime;

        // Giới hạn biên theo content mới
        float minX = -(contentWidth - viewportWidth);
        float maxX = 0;

        if (content.anchoredPosition.x <= minX)
        {
            content.anchoredPosition = new Vector2(minX, content.anchoredPosition.y);
            direction = 1;
        }
        else if (content.anchoredPosition.x >= maxX)
        {
            content.anchoredPosition = new Vector2(maxX, content.anchoredPosition.y);
            direction = -1;
        }

        if (content.anchoredPosition.x <= minX + 1f || content.anchoredPosition.x >= maxX - 1f)
        {
            scrollRect.velocity = Vector2.zero;
        }
    }
}
