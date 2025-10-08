using UnityEngine;
using UnityEngine.EventSystems;

public class StickerMoveHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]private Sticker stickerScript;
    private Vector3 startPos;
    private Vector2 startDragPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        stickerScript.OnSelectButton();
        stickerScript.MoveStickerToEnd();
        startPos = stickerScript.transform.position;
        startDragPos = eventData.position; // vị trí chuột ban đầu (pixel screen space)
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 dragDelta = eventData.position - startDragPos;

        Vector3 worldDelta = Camera.main.ScreenToWorldPoint(new Vector3(dragDelta.x, dragDelta.y, 0f))
                        - Camera.main.ScreenToWorldPoint(Vector3.zero);

        Vector3 targetPos = startPos + worldDelta;

        Bounds stickerBounds = stickerScript.GetStickerSprite().GetComponent<PolygonCollider2D>().bounds;
        Bounds areaBounds = DecorManager.Instance.GetBackgroundArea().bounds;

        Vector3 size = stickerBounds.size;

        // Clamp CENTER của sticker sao cho luôn nằm trong vùng chứa
        float minX = areaBounds.min.x + size.x / 2f;
        float maxX = areaBounds.max.x - size.x / 2f;
        float minY = areaBounds.min.y + size.y / 2f;
        float maxY = areaBounds.max.y - size.y / 2f;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        stickerScript.transform.position = targetPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        stickerScript.SavePosition();
    }
}
