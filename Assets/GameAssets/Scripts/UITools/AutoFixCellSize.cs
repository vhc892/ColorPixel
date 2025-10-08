using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AutoFixCellSize : MonoBehaviour
{
    private GridLayoutGroup grid;

    [Header("Config")]
    public Vector2 baseResolution = new Vector2(1080, 1920);
    public Vector2 baseCellSize = new Vector2(323, 333);

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
    }

    void Start()
    {
        UpdateCellSize();
    }

    // void OnRectTransformDimensionsChange()
    // {
    //     // Cập nhật khi thay đổi kích thước màn hình
    //     UpdateCellSize();
    // }

    void UpdateCellSize()
    {
        // Lấy chiều rộng hiện tại của parent (thường là Canvas/Panel)
        RectTransform rt = transform as RectTransform;
        float width = rt.rect.width;

        // Tính scale theo tỉ lệ màn hình hiện tại so với base resolution
        float scale = width / baseResolution.x;

        // Áp dụng cellSize mới
        Vector2 newCellSize = baseCellSize * scale;
        //Debug.Log(width + " / " + baseResolution.x + " = " + scale);
        grid.cellSize = newCellSize;
    }
}
