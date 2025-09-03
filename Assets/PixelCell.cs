using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PixelCell : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public int colorID { get; private set; }
    public bool isColored = false;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshPro numberText;

    private static PixelCell lastDraggedOverCell; // Biến static để tối ưu hóa

    public void Initialize(int id)
    {
        colorID = id;
        numberText.text = id.ToString();
        spriteRenderer.color = Color.white; 
        isColored = false;
        numberText.gameObject.SetActive(true);
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
        isColored = true;
        numberText.gameObject.SetActive(false);
    }
    public void ResetHighlight()
    {
        if (!isColored)
        {
            spriteRenderer.color = Color.white;
        }
    }


    // 1. Khi bắt đầu nhấn xuống
    public void OnPointerDown(PointerEventData eventData)
    {
        // Báo cho GameManager để bắt đầu quá trình kéo-tô
        GameManager.instance.StartDragColoring(this);
        // Reset lại ô cuối cùng đã kéo qua
        lastDraggedOverCell = null;
    }

    // 2. Khi đang kéo
    public void OnDrag(PointerEventData eventData)
    {
        // Dùng Raycast để tìm xem con trỏ đang ở trên đối tượng UI nào
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Thử lấy component PixelCell từ đối tượng tìm thấy
            PixelCell cellUnderPointer = result.gameObject.GetComponent<PixelCell>();

            // Nếu tìm thấy một ô pixel và nó không phải là ô mình vừa kéo qua
            if (cellUnderPointer != null && cellUnderPointer != lastDraggedOverCell)
            {
                // Báo cho GameManager thử tô màu ô này
                GameManager.instance.DragOverCell(cellUnderPointer);
                // Cập nhật ô cuối cùng đã kéo qua
                lastDraggedOverCell = cellUnderPointer;
                // Thoát khỏi vòng lặp sau khi tìm thấy ô đầu tiên
                break;
            }
        }
    }

    // 3. Khi nhả tay ra
    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.instance.StopDragColoring();
    }
}