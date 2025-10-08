using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomManager : MonoBehaviour
{
    public static BoomManager Instance;

    [Tooltip("Tốc độ lan")]
    [SerializeField] private int ringsPerFrame = 1;
    [SerializeField] private int explosionRadius = 12;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void StartBoom(int startX, int startY)
    {
        var gm = CoreGameManager.Instance;
        var bwTexture = gm.GetBWTexture();
        int w = bwTexture.width, h = bwTexture.height;

        if (startX < 0 || startX >= w || startY < 0 || startY >= h) return;

        ReplaySystem.Instance.RecordAction(ReplayActionType.Boom, startX, startY);
        PlayerManager.Instance.PaintBooster = Helper.PaintBooster.None;
        PlayerManager.Instance.boomBoosterNumber--;
        PlayerManager.Instance.UpdateBoosterUI();
        PlayerManager.Instance.DeselectAllBoosters();
        QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Daily_3_Boosters);
        QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Achievement_Boom);
        AudioManager.Instance.BoomBoosterSfx();

        StopAllCoroutines();
        StartCoroutine(AnimatedBoom(gm, startX, startY));
    }

    public void StartBoom(Vector3 worldPos)
    {
        var gm = CoreGameManager.Instance;
        var bwTexture = gm.GetBWTexture();
        var artArray = gm.GetArtArray();
        
        Vector2 pixelPos = gm.WorldToPixel(worldPos);
        int px = (int)pixelPos.x;
        int py = (int)pixelPos.y;

        int radius = GetExplosionRadius();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (Mathf.Pow(Mathf.Abs(x), 2) + Mathf.Pow(Mathf.Abs(y), 2) > Mathf.Pow(radius - 0.5f, 2))
                    continue;
                int p_x = (int)pixelPos.x + x;
                int p_y = (int)pixelPos.y + y;
                if (p_x >= 0 && p_x < artArray.GetLength(0) && p_y >= 0 && p_y < artArray.GetLength(1))
                {
                    Color original = bwTexture.GetPixel(p_x, p_y);
                    if (original.a > CoreGameManager.minAlpha)
                    {
                        StartBoom(px, py);
                        return;
                    }
                }
            }
        }
    }

    private IEnumerator AnimatedBoom(CoreGameManager gm, int startX, int startY)
    {
        var artArray = gm.GetArtArray();
        var bwTexture = gm.GetBWTexture();
        int w = bwTexture.width, h = bwTexture.height;

        List<Vector2Int> targets = new List<Vector2Int>();
        for (int offsetX = -explosionRadius; offsetX <= explosionRadius; offsetX++)
        {
            for (int offsetY = -explosionRadius; offsetY <= explosionRadius; offsetY++)
            {
                if (Mathf.Pow(Mathf.Abs(offsetX), 2) + Mathf.Pow(Mathf.Abs(offsetY), 2) > Mathf.Pow(explosionRadius - 0.5f, 2))
                    continue;

                int px = startX + offsetX;
                int py = startY + offsetY;

                if (px >= 0 && px < w && py >= 0 && py < h)
                {
                    var c = bwTexture.GetPixel(px, py);
                    if (c.a > 0.1f)
                    {
                        targets.Add(new Vector2Int(px, py));
                    }
                }
            }
        }
        if (targets.Count == 0) yield break;

        Dictionary<int, List<Vector2Int>> buckets = new Dictionary<int, List<Vector2Int>>();
        int maxD = 0;
        foreach (var p in targets)
        {
            int dx = p.x - startX;
            int dy = p.y - startY;
            int d = Mathf.RoundToInt(Mathf.Sqrt(dx * dx + dy * dy));

            if (!buckets.TryGetValue(d, out var list))
            {
                list = new List<Vector2Int>();
                buckets[d] = list;
            }
            list.Add(p);
            if (d > maxD) maxD = d;
        }

        // 3) Vẽ theo “vành”, lan từ tâm 
        for (int d = 0; d <= maxD; d += Mathf.Max(1, ringsPerFrame))
        {
            for (int step = 0; step < ringsPerFrame && d + step <= maxD; step++)
            {
                int ring = d + step;
                if (buckets.TryGetValue(ring, out var list))
                {
                    foreach (var p in list)
                    {
                        var c = bwTexture.GetPixel(p.x, p.y);
                        if (c.a > 0.1f)
                        {
                            int colorNumber = artArray[p.x, p.y];
                            if (colorNumber > 0)
                            {
                                var colorBox = ColorBoxPool.Instance.GetColorBoxByNumber(colorNumber);
                                if (colorBox != null)
                                {
                                    c.a = 0f;
                                    bwTexture.SetPixel(p.x, p.y, c);
                                    PaintingFX.Instance.PlayFillEffect(p);
                                    colorBox.UpdateCurrentPixel(p.x, p.y);
                                }
                            }
                        }
                    }
                }
            }
            bwTexture.Apply();
            yield return null;
        }
        if (!ReplaySystem.isReplaying)
        {
            //gm.CheckWinCondition();
        }
    }
    public Coroutine StartBoomForReplay(int startX, int startY)
    {
        var gm = CoreGameManager.Instance;
        // Bỏ qua logic game, chỉ chạy coroutine hiệu ứng
        return StartCoroutine(AnimatedBoom(gm, startX, startY));
    }

    public int GetExplosionRadius()
    {
        return explosionRadius;
    }
}