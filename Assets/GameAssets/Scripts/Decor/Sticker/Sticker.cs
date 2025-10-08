using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticker : MonoBehaviour
{
    public static readonly Vector2 INIT_SELECTION_FRAME_SIZE = Vector2.one * 2.56f;
    public const float DEFAULT_SELECTION_FRAME_SIZE = 0.4f;

    [SerializeField] private SpriteRenderer selectionFrame;
    [SerializeField] private SpriteRenderer sticker;

    private StickerData stickerData = new StickerData("name", 0);

    [SerializeField] private Transform topLeft;
    [SerializeField] private Transform topRight;
    [SerializeField] private Transform bottomLeft;
    [SerializeField] private Transform bottomRight;

    public void SetData(StickerData data, bool isLoad = false)
    {

        stickerData = data;
        name = stickerData.name;
        ChangeSprite(DecorManager.Instance
                        .GetDecorDatabaseByType(Helper.DecorType.Sticker)
                        .decorSOs[stickerData.index].sprite);
        transform.position = stickerData.pos;
        UpdateScale();
        UpdateRotation();
        UpdateLayer(GetLayerNumberBySticker());


        if (isLoad) selectionFrame.transform.parent.gameObject.SetActive(false);
    }

    public void UpdateScale()
    {
        selectionFrame.size = INIT_SELECTION_FRAME_SIZE * stickerData.scale;
        sticker.transform.localScale = Vector3.one * stickerData.scale;

        BoxCollider2D box = selectionFrame.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = selectionFrame.size;
        }

        UpdateCornerPositionByScale();
    }

    public void UpdateRotation()
    {
        transform.localEulerAngles = new Vector3(0, 0, stickerData.rotationAngle);
    }

    public void SavePosition()
    {
        stickerData.pos = transform.position;
    }

    private void UpdateCornerPositionByScale()
    {
        Vector2 size = selectionFrame.size;
        Vector2 halfSize = size * 0.5f;

        if (topLeft != null) topLeft.localPosition = new Vector3(-halfSize.x, halfSize.y, topLeft.localPosition.z);
        if (topRight != null) topRight.localPosition = new Vector3(halfSize.x, halfSize.y, topRight.localPosition.z);
        if (bottomLeft != null) bottomLeft.localPosition = new Vector3(-halfSize.x, -halfSize.y, bottomLeft.localPosition.z);
        if (bottomRight != null) bottomRight.localPosition = new Vector3(halfSize.x, -halfSize.y, bottomRight.localPosition.z);

        if (topLeft != null) topLeft.localScale = Vector3.one * stickerData.scale;
        if (topRight != null) topRight.localScale = Vector3.one * stickerData.scale;
        if (bottomLeft != null) bottomLeft.localScale = Vector3.one * stickerData.scale;
        if (bottomRight != null) bottomRight.localScale = Vector3.one * stickerData.scale;
    }

    public void UpdateLayer(int index)
    {
        transform.position =
        new Vector3(transform.position.x,
                    transform.position.y,
                    -0.01f * index);
    }

    private void ChangeSprite(Sprite newSprite)
    {
        sticker.sprite = newSprite;

        // Lấy physics shape từ sprite mới
        PolygonCollider2D poly = sticker.GetComponent<PolygonCollider2D>();
        poly.pathCount = sticker.sprite.GetPhysicsShapeCount();

        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < poly.pathCount; i++)
        {
            path.Clear();
            sticker.sprite.GetPhysicsShape(i, path);
            poly.SetPath(i, path);
        }
    }

    public bool CanMove()
    {
        return selectionFrame.transform.parent.gameObject.activeInHierarchy;
    }

    public int GetLayerNumberBySticker()
    {
        ArtBoxSO artBoxSO = CoreGameManager.Instance.GetCurrentArtBoxSO();
        int index = artBoxSO.stickerDatas.IndexOf(stickerData);
        return index;
    }

    public void MoveStickerToEnd()
    {
        ArtBoxSO artBoxSO = CoreGameManager.Instance.GetCurrentArtBoxSO();
        // tìm vị trí hiện tại
        int index = artBoxSO.stickerDatas.IndexOf(stickerData);

        if (index == -1) return;          // không có trong list thì thoát

        // nếu đã ở cuối thì không cần làm gì
        if (index == artBoxSO.stickerDatas.Count - 1) return;

        // xoá khỏi vị trí cũ và thêm lại ở cuối
        artBoxSO.stickerDatas.RemoveAt(index);
        artBoxSO.stickerDatas.Add(stickerData);
        UpdateAllStickersLayer();
    }

    public void UpdateAllStickersLayer()
    {
        foreach (var sticker in StickerPool.Instance.GetActiveStickers())
        {
            sticker.UpdateLayer(sticker.GetLayerNumberBySticker());
        }
    }

    public void OnSelectButton()
    {
        SetActiveFrame(true);
        MoveStickerToEnd();
    }

    public void OnFinishButton()
    {
        SetActiveFrame(false);
    }

    public void SetActiveFrame(bool isActive)
    {
        selectionFrame.transform.parent.gameObject.SetActive(isActive);
    }

    public void OnDeleteButton()
    {
        ArtBoxSO artBoxSO = CoreGameManager.Instance.GetCurrentArtBoxSO();
        if (artBoxSO != null)
        {
            artBoxSO.stickerDatas.Remove(stickerData);
        }
        StickerPool.Instance.ReturnSticker(this);
    }
    public StickerData GetStickerData()
    {
        return stickerData;
    }
    
    public SpriteRenderer GetStickerSprite()
    {
        return sticker;
    }
}
