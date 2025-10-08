using UnityEngine;
using UnityEngine.EventSystems;

public class StickerRotateHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]private Sticker stickerScript;
    private float startRotationZ;
    private float startAngle;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startRotationZ = stickerScript.GetStickerData().rotationAngle;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 dir = worldPos - stickerScript.transform.position;
        startAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 dir = worldPos - stickerScript.transform.position;
        float currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float angleDelta = currentAngle - startAngle;
        stickerScript.GetStickerData().rotationAngle = startRotationZ + angleDelta;

        stickerScript.UpdateRotation();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }
}
