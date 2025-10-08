using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillManager : MonoBehaviour
{
    public static FillManager Instance;

    [SerializeField] private int ringsPerFrame = 1;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }


    public void StartFill(int startX, int startY)
    {
        var gm = CoreGameManager.Instance;
        var artArray = gm.GetArtArray();                   
        var bwTexture = gm.GetBWTexture();                  
        int w = bwTexture.width, h = bwTexture.height;

        if (startX < 0 || startX >= w || startY < 0 || startY >= h) return;

        int colorNumber = artArray[startX, startY];
        var colorBox = ColorBoxPool.Instance.GetColorBoxByNumber(colorNumber);
        if (!colorBox.CanInteract()) return;

        ReplaySystem.Instance.RecordAction(ReplayActionType.Fill, startX, startY);
        PlayerManager.Instance.PaintBooster = Helper.PaintBooster.None;
        PlayerManager.Instance.fillBoosterNumber--;
        PlayerManager.Instance.UpdateBoosterUI();
        PlayerManager.Instance.DeselectAllBoosters();
        QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Daily_3_Boosters);
        QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Achievement_Fill);
        AudioManager.Instance.FillBoosterSfx();

        StopAllCoroutines();
        StartCoroutine(AnimatedGlobalWaveFill(gm, startX, startY, colorNumber, colorBox));
    }

    public void StartFill(Vector3 worldPos)
    {
        var gm = CoreGameManager.Instance;
        var artArray = gm.GetArtArray();                   
        var bwTexture = gm.GetBWTexture();                  

        Vector2 pixelPos = gm.WorldToPixel(worldPos);
        int px = (int)pixelPos.x;
        int py = (int)pixelPos.y;

        if (px >= 0 && px < bwTexture.width && py >= 0 && py < bwTexture.height)
        {
            if (PlayerManager.Instance.PaintBooster == Helper.PaintBooster.Fill)
            {
                if (artArray[px, py] == 0)
                {
                    return;
                }
                StartFill(px, py);
            }
        }
    }

    /// <summary>
    /// Thuật toán “sóng toàn cục”:
    /// lan theo vành khoảng cách (Manhattan) từ điểm bắt đầu tới TẤT CẢ pixel cùng colorNumber.
    /// Không bị chặn bởi màu khác.
    /// </summary>
    private IEnumerator AnimatedGlobalWaveFill(CoreGameManager gm, int startX, int startY, int colorNumber, ColorBox colorBox)
    {
        var artArray = gm.GetArtArray();
        var bwTexture = gm.GetBWTexture();
        int w = bwTexture.width, h = bwTexture.height;

        // 1) gom toàn bộ mục tiêu (đúng màu & chưa tô) 
        List<Vector2Int> targets = new List<Vector2Int>();
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (artArray[x, y] == colorNumber)
                {
                    var c = bwTexture.GetPixel(x, y);
                    if (c.a > 0.1f) targets.Add(new Vector2Int(x, y));
                }
            }
        if (targets.Count == 0) yield break;

        // 2) bucket theo khoảng cách Manhattan |dx|+|dy|
        Dictionary<int, List<Vector2Int>> buckets = new Dictionary<int, List<Vector2Int>>();
        int maxD = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            var p = targets[i];
            int d = Mathf.Abs(p.x - startX) + Mathf.Abs(p.y - startY);
            if (!buckets.TryGetValue(d, out var list)) { list = new List<Vector2Int>(); buckets[d] = list; }
            list.Add(p);
            if (d > maxD) maxD = d;
        }

        for (int d = 0; d <= maxD; d += Mathf.Max(1, ringsPerFrame))
        {
            for (int step = 0; step < ringsPerFrame && d + step <= maxD; step++)
            {
                int ring = d + step;
                if (buckets.TryGetValue(ring, out var list))
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        var p = list[j];
                        var c = bwTexture.GetPixel(p.x, p.y);
                        if (c.a > 0.1f)
                        {
                            c.a = 0f;
                            bwTexture.SetPixel(p.x, p.y, c);
                            PaintingFX.Instance.PlayFillEffect(p);
                            colorBox.UpdateCurrentPixel(p.x, p.y);
                        }
                    }
                }
            }
            bwTexture.Apply();
            yield return null;
        }
        AudioManager.Instance.StopFillBoosterSfx();
        if (!ReplaySystem.isReplaying)
        {
            //gm.CheckWinCondition();
        }
    }
    public Coroutine StartFillForReplay(int startX, int startY)
    {
        var gm = CoreGameManager.Instance;
        var artArray = gm.GetArtArray();
        var bwTexture = gm.GetBWTexture();
        int w = bwTexture.width, h = bwTexture.height;

        if (startX < 0 || startX >= w || startY < 0 || startY >= h) return null;

        int colorNumber = artArray[startX, startY];
        var colorBox = ColorBoxPool.Instance.GetColorBoxByNumber(colorNumber);
        if (colorBox == null) return null;

        // Bỏ qua logic game, chỉ chạy coroutine hiệu ứng
        return StartCoroutine(AnimatedGlobalWaveFill(gm, startX, startY, colorNumber, colorBox));
    }
}
