using UnityEngine;
using UnityEngine.EventSystems;

public class StickerScaleHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]private Sticker stickerScript;               
    private float startScale;
    private Vector2 startDragPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!stickerScript.CanMove()) return;
        startScale = stickerScript.GetStickerData().scale; 
        startDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!stickerScript.CanMove()) return;

        // vector di chuyển của chuột trên màn hình
        Vector2 screenDelta = eventData.position - startDragPos;

        // đổi sang vector trong local space của sticker
        Vector3 localDelta = stickerScript.transform.InverseTransformVector(screenDelta);

        // lấy thành phần dọc theo trục local Y (trục “lên” của sticker)
        float dragDelta = localDelta.y / 500f;

        float newScale = Mathf.Clamp(startScale - dragDelta, 0.2f, 1f);
        if (!Mathf.Approximately(stickerScript.GetStickerData().scale, newScale))
        {
            stickerScript.GetStickerData().scale = newScale;
            stickerScript.UpdateScale();
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("End scale drag");
    }
}
