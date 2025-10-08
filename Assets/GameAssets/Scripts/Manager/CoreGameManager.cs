using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CoreGameManager : MonoBehaviour
{
    public static CoreGameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            spriteRendererGrayScale = spriteRenderer.transform.GetChild(0).GetComponent<SpriteRenderer>();
            initialScale = spriteRenderer.transform.localScale;
#if !UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private Vector3 initialScale;
    private int paintRadiusPixel = 1;
    private ArtBoxSO currentArtSO;
    public SpriteRenderer spriteRenderer;
    private SpriteRenderer spriteRendererGrayScale;

    private Texture2D bwTexture;
    private Material grayScaleMaterial;
    private int[,] artArray;
    private bool[,] showedPixelArea;
    private bool isGameWon = false;
    private Color32[] colorArray;
    public ColorBox currentColorBox;
    public Texture2D sparkleTexture;

    public static float minAlpha = 0.01f;

    public static event Action<Color> OnColorSelected;
    public void StartGame(ArtBoxSO ArtBoxSO)
    {
        InputHandler.Instance.SwitchToCoreInput();
        InputHandler.Instance.SetUpCamera();
        isGameWon = false;
        ReplaySystem.Instance.StartRecording(ArtBoxSO.sprite.name);

        currentArtSO = ArtBoxSO;
        spriteRenderer.sprite = currentArtSO.sprite;

        int spriteSize = Mathf.RoundToInt(currentArtSO.sprite.rect.height);
        spriteRenderer.transform.localScale = initialScale * (64f / spriteSize);

        InputHandler.Instance.SwitchZoomInSize(spriteSize);
        SetUpGrayScaleSpriteRenderer();
        InitArray();
        UpdateBorderByScale();
        InitColorBoxes();

        var indexTex = ArtArrayToIndexTexture(artArray);
        grayScaleMaterial.SetTexture("_IndexTex", indexTex);
        grayScaleMaterial.SetTexture("_NumberAtlas", GameAssets.i.NumberAtlas());
        grayScaleMaterial.SetVector("_AtlasSize", new Vector4(10, 10, 0, 0));

        grayScaleMaterial.SetFloat("_SelectedIndex", -1f);
        grayScaleMaterial.SetFloat("_BlinkOn", 0f);

        var artworkMaskTexture = CreateArtworkMaskTexture(showedPixelArea);
        grayScaleMaterial.SetTexture("_ArtworkMask", artworkMaskTexture);

        PlayerManager.Instance.DeselectAllBoosters();
        SelectedFirstColorBox();

        //Load Blinkblink
        UpdateSparkleEffect();
        UIManager.Instance.UpdateArtBoxSparkle();

        if (PlayerPrefs.GetInt("HasSeenTutorial", 0) == 0)
        {
            UIManager.Instance.ShowTutorialPopup();
            PlayerPrefs.SetInt("HasSeenTutorial", 1);
            PlayerPrefs.Save();
        }

        if (!currentArtSO.isDone)
        {
            CheckWinCondition();
        }

    }

    private void InitArray()
    {
        colorArray = GetUniqueColorsSorted();

        // Tạo map index để Set Color Index cho ArtArray
        Dictionary<Color32, int> colorIndexMap = new Dictionary<Color32, int>();
        for (int i = 0; i < colorArray.Length; i++)
        {
            Color32 c = colorArray[i];
            c.a = 255;
            colorIndexMap[c] = i + 1;
        }

        // Duyệt từng pix 64x64
        Texture2D originalTexture = spriteRenderer.sprite.texture;
        Color32[] pixels = originalTexture.GetPixels32();
        int width = originalTexture.width;
        int height = originalTexture.height;

        // Init Array
        int[,] artArrayTmp = new int[width, height];
        bool[,] showedPixelAreaTmp = new bool[width, height];


        Parallel.For(0, width, px =>
        {
            for (int py = 0; py < height; py++)
            {
                Color32 original = pixels[XYToIndex(px, py, width)];
                if (original.a > 0.5)
                {
                    showedPixelAreaTmp[px, py] = true;

                    Color32 fixedOriginal = original;
                    fixedOriginal.a = 255;
                    // Set Color Index trên ArtArray
                    artArrayTmp[px, py] = colorIndexMap.TryGetValue(fixedOriginal, out int index) ? index : 0;
                }
                else
                {
                    showedPixelAreaTmp[px, py] = false;
                    artArrayTmp[px, py] = 0;
                }

            }
        });

        artArray = artArrayTmp;
        showedPixelArea = showedPixelAreaTmp;
    }

    public Color32[] GetUniqueColorsSorted()
    {
        Texture2D originalTexture = spriteRenderer.sprite.texture;
        Color32[] pixels = originalTexture.GetPixels32();

        HashSet<int> uniqueRGB = new HashSet<int>();
        List<Color32> uniqueColors = new List<Color32>();

        foreach (var p in pixels)
        {
            if (p.a > 25 && (p.r < 240 || p.g < 240 || p.b < 240)) // bỏ pixel trong suốt và trắng
            {
                int rgb = (p.r << 16) | (p.g << 8) | p.b;
                if (uniqueRGB.Add(rgb))
                {
                    uniqueColors.Add(new Color32(p.r, p.g, p.b, 255));
                }
            }
        }

        // Sort theo Hue rồi Value
        uniqueColors.Sort((a, b) =>
        {
            // Check nếu là đen
            bool isBlackA = (a.r == 0 && a.g == 0 && a.b == 0);
            bool isBlackB = (b.r == 0 && b.g == 0 && b.b == 0);
            if (isBlackA && !isBlackB) return -1;
            if (!isBlackA && isBlackB) return 1;

            // Convert sang HSV
            Color.RGBToHSV(a, out float hA, out float sA, out float vA);
            Color.RGBToHSV(b, out float hB, out float sB, out float vB);

            // Ưu tiên Hue trước
            int cmp = hA.CompareTo(hB);
            if (cmp != 0) return cmp;

            // Nếu Hue giống thì sort theo Value (độ sáng)
            return vA.CompareTo(vB);
        });

        return uniqueColors.ToArray();
    }

    private void InitColorBoxes()
    {
        ColorBoxPool.Instance.ReturnAllColorBoxes();

        for (int i = 0; i < colorArray.Length; i++)
        {
            ColorBox colorBox = ColorBoxPool.Instance.GetColorBox();
            colorBox.SetData(i + 1, colorArray[i]);
        }

        // Tạo targetPixel để Set Target Pixel cho ColorBox
        Texture2D originalTexture = spriteRenderer.sprite.texture;
        int width = originalTexture.width;
        int height = originalTexture.height;
        Dictionary<int, int> targetPixelsByColorIndex = new Dictionary<int, int>();
        Dictionary<int, int> currentPixelsByColorIndex = new Dictionary<int, int>();
        for (int px = 0; px < width; px++)
        {
            for (int py = 0; py < height; py++)
            {
                int colorIndex = artArray[px, py];
                if (colorIndex != 0)
                {
                    if (!targetPixelsByColorIndex.ContainsKey(colorIndex))
                        targetPixelsByColorIndex[colorIndex] = 0;

                    if (!currentPixelsByColorIndex.ContainsKey(colorIndex))
                        currentPixelsByColorIndex[colorIndex] = 0;

                    targetPixelsByColorIndex[colorIndex]++;
                    if (bwTexture.GetPixel(px, py).a == 0)
                        currentPixelsByColorIndex[colorIndex]++;
                }
            }
        }

        foreach (ColorBox colorBox in ColorBoxPool.Instance.GetActiveColorBoxs())
        {
            colorBox.SetTargetPixel(targetPixelsByColorIndex[colorBox.number]);
            colorBox.SetCurrentPaintedPixel(currentPixelsByColorIndex[colorBox.number]);
            if (!colorBox.CanInteract())
            {
                colorBox.SetCompletedState();
            }
        }

    }
    private void SelectedFirstColorBox()
    {
        List<ColorBox> activeBoxes = ColorBoxPool.Instance.GetActiveColorBoxs();
        ColorBox smallestNumberBox = null;

        foreach (ColorBox box in activeBoxes)
        {
            if (box.CanInteract())
            {
                if (smallestNumberBox == null || box.number < smallestNumberBox.number)
                {
                    smallestNumberBox = box;
                }
            }
        }
        if (smallestNumberBox != null)
        {
            SelectColorBox(smallestNumberBox);
        }
    }
    public void SelectNextColorBox(ColorBox completedBox)
    {
        List<ColorBox> activeBoxes = ColorBoxPool.Instance.GetActiveColorBoxs();
        if (activeBoxes.Count == 0) return;

        int currentIndex = activeBoxes.IndexOf(completedBox);
        if (currentIndex == -1)
        {
            SelectSmallestAvailableBox();
            return;
        }
        int rightIndex = currentIndex + 1;
        if (rightIndex < activeBoxes.Count)
        {
            ColorBox nextBox = activeBoxes[rightIndex];
            if (nextBox.CanInteract())
            {
                SelectColorBox(nextBox);
                return;
            }
        }
        int leftIndex = currentIndex - 1;
        if (leftIndex >= 0)
        {
            ColorBox prevBox = activeBoxes[leftIndex];
            if (prevBox.CanInteract())
            {
                SelectColorBox(prevBox);
                return;
            }
        }
        SelectSmallestAvailableBox();
    }
    private void SelectSmallestAvailableBox()
    {
        List<ColorBox> activeBoxes = ColorBoxPool.Instance.GetActiveColorBoxs();
        ColorBox nextBoxToSelect = null;

        foreach (ColorBox box in activeBoxes)
        {
            if (box.CanInteract())
            {
                if (nextBoxToSelect == null || box.number < nextBoxToSelect.number)
                {
                    nextBoxToSelect = box;
                }
            }
        }
        if (nextBoxToSelect != null)
        {
            SelectColorBox(nextBoxToSelect);
        }
    }

    public void SelectColorBox(ColorBox selectedBox)
    {
        if (currentColorBox != null && currentColorBox != selectedBox)
        {
            currentColorBox.Deselect();
        }

        currentColorBox = selectedBox;
        currentColorBox.Select();
        SetHighlightIndex(currentColorBox.number, true);
        if (currentColorBox.colorImage != null)
        {
            OnColorSelected?.Invoke(currentColorBox.colorImage.color);
        }
        var scroller = selectedBox.GetComponentInParent<ScrollToSelected>();
        if (scroller != null)
        {
            scroller.CenterOnItem(selectedBox.transform as RectTransform);
        }
    }

    private void SetUpGrayScaleSpriteRenderer()
    {
        Sprite loadSprite = SaveLoadImage.LoadSpriteProgress(spriteRenderer.sprite);
        if (loadSprite == null)
        {
            spriteRendererGrayScale.sprite = CreateGrayScaleSprite(spriteRenderer.sprite);
        }
        else
        {
            spriteRendererGrayScale.sprite = loadSprite;
        }

        Sprite sprite = spriteRendererGrayScale.sprite;
        bwTexture = sprite.texture;

        grayScaleMaterial = spriteRendererGrayScale.material;
        grayScaleMaterial.SetVector("_TextureSize", new Vector4(bwTexture.width, bwTexture.height, 0, 0));
    }

    public Sprite CreateGrayScaleSprite(Sprite sprite)
    {
        // Tạo texture mới 
        Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        tex.filterMode = FilterMode.Point; // giữ đúng pixel khi scale
        tex.wrapMode = TextureWrapMode.Clamp;

        // Lấy pixel từ sprite gốc
        Color[] pixels = sprite.texture.GetPixels(
            (int)sprite.rect.x,
            (int)sprite.rect.y,
            (int)sprite.rect.width,
            (int)sprite.rect.height);

        // Convert sang grayscale
        float minGray = 0.55f;
        float maxGray = 0.98f;
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if ((c.r < 240f / 255f || c.g < 240f / 255f || c.b < 240f / 255f) && c.a > 0.5f)
            {
                // Công thức tạo sang đen trắng 
                float gray = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;

                gray = Mathf.Lerp(minGray, maxGray, gray);

                pixels[i] = new Color(gray, gray, gray, c.a);
            }
            else
            {
                pixels[i] = new Color(c.r, c.g, c.b, 0);
            }

        }

        // Apply vào texture
        tex.SetPixels(pixels);
        tex.Apply();

        // Tạo Sprite mới từ bwTexture
        Sprite bwSprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), // pivot center
            sprite.pixelsPerUnit
        );
        bwSprite.name = sprite.name;
        return bwSprite;
    }

    public bool PaintAtPosition(Vector2 worldPos)
    {
        Vector2 pixelPos = WorldToPixel(worldPos);
        bool isPainting = false;

        int px = (int)pixelPos.x;
        int py = (int)pixelPos.y;

        if (PlayerManager.Instance.PaintBooster == Helper.PaintBooster.None)
        {
            ReplaySystem.Instance.RecordAction(ReplayActionType.Paint, px, py, currentColorBox.number);
        }

        if (px >= 0 && px < bwTexture.width && py >= 0 && py < bwTexture.height)
        {
            if (PlayerManager.Instance.PaintBooster == Helper.PaintBooster.Fill)
            {
                if (artArray[px, py] == 0)
                {
                    return false;
                }
                FillAtPixel(px, py);
                return false;
            }
            else if (PlayerManager.Instance.PaintBooster == Helper.PaintBooster.Boom)
            {
                int radius = BoomManager.Instance.GetExplosionRadius();
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
                            if (original.a > minAlpha)
                            {
                                BoomAtPixel(px, py);
                                return false;
                            }
                        }
                    }
                }
                return false;
            }
        }

        for (int x = -paintRadiusPixel; x <= paintRadiusPixel; x++)
        {
            for (int y = -paintRadiusPixel; y <= paintRadiusPixel; y++)
            {
                px = (int)pixelPos.x + x;
                py = (int)pixelPos.y + y;
                if (px >= 0 && px < artArray.GetLength(0) && py >= 0 && py < artArray.GetLength(1))
                {
                    Color original = bwTexture.GetPixel(px, py);
                    if (original.a > minAlpha)
                    {
                        if (artArray[px, py] != currentColorBox.number) continue;
                        original.a = 0;
                        bwTexture.SetPixel(px, py, original);
                        PaintingFX.Instance.PlayFillEffect(new Vector2Int(px, py));
                        currentColorBox.UpdateCurrentPixel(px, py);
                        QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Daily_100_Blocks);
                        QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Achievement_Pixel);
                        isPainting = true;
                    }
                }
            }
        }
        bwTexture.Apply();

        return isPainting;
    }

    public void UpdateBorderByScale()
    {
        if (grayScaleMaterial == null) return;
        float minScale = InputHandler.Instance.GetZoomInSize() + 2;
        float maxScale = InputHandler.Instance.GetZoomInSize();

        float curScale = Camera.main.orthographicSize;

        // Gửi dữ liệu sang shader
        grayScaleMaterial.SetFloat("_CurScale", -curScale);
        grayScaleMaterial.SetFloat("_MinScale", -minScale);
        grayScaleMaterial.SetFloat("_MaxScale", -maxScale);
    }


    /// <summary>
    /// Lấy đúng pixel theo tọa độ (x, y) từ texture BW
    /// </summary>
    public Color GetPixel(int x, int y)
    {
        return bwTexture.GetPixel(x, y);
    }

    int XYToIndex(int x, int y, int width)
    {
        return y * width + x;
    }

    Vector2Int IndexToXY(int index, int width)
    {
        int y = index / width;
        int x = index % width;
        return new Vector2Int(x, y);
    }

    public Vector2 WorldToPixel(Vector3 worldPos)
    {
        Vector3 localPos = spriteRenderer.transform.InverseTransformPoint(worldPos);
        float ppu = spriteRenderer.sprite.pixelsPerUnit;
        Vector2 pivot = spriteRenderer.sprite.pivot;

        float pixelX = pivot.x + localPos.x * ppu;
        float pixelY = pivot.y + localPos.y * ppu;

        return new Vector2(pixelX, pixelY);
    }

    public Vector3 PixelToWorld(Vector2 pixelPos)
    {
        float ppu = spriteRenderer.sprite.pixelsPerUnit;
        Vector2 pivot = spriteRenderer.sprite.pivot;

        // Tính vị trí local từ pixel
        float localX = (pixelPos.x - pivot.x) / ppu;
        float localY = (pixelPos.y - pivot.y) / ppu;
        Vector3 localPos = new Vector3(localX, localY, 0);

        // Chuyển local → world
        Vector3 worldPos = spriteRenderer.transform.TransformPoint(localPos);

        return worldPos;
    }


    Texture2D ArtArrayToIndexTexture(int[,] artArray)
    {
        int width = artArray.GetLength(0);
        int height = artArray.GetLength(1);

        Texture2D indexTex = new Texture2D(width, height, TextureFormat.R8, false);
        indexTex.filterMode = FilterMode.Point;
        indexTex.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                int val = artArray[x, y]; // 0..99
                pixels[idx] = new Color32((byte)val, 0, 0, 255);
            }
        }

        indexTex.SetPixels32(pixels);
        indexTex.Apply();
        return indexTex;
    }

    public int[,] GetArtArray() { return artArray; }
    public Texture2D GetBWTexture() { return bwTexture; }
    void FillAtPixel(int x, int y)
    {
        FillManager.Instance.StartFill(x, y);
    }

    void BoomAtPixel(int x, int y)
    {
        BoomManager.Instance.StartBoom(x, y);
    }

    public void ZoomAtValidPixel()
    {
        Vector2 pixelPos = FindRandomPixelPosition();
        Vector3 worldPos = PixelToWorld(pixelPos);
        if (pixelPos != -Vector2.one)
        {
            Camera.main.DOOrthoSize(InputHandler.Instance.GetZoomInSize(), 0.8f).SetEase(Ease.InOutSine)
            .OnComplete(() => UpdateBorderByScale());

            Vector3 cameraPos = new Vector3(worldPos.x, worldPos.y, Camera.main.transform.position.z);
            Camera.main.transform.DOMove(cameraPos, 0.5f).SetEase(Ease.InOutSine);
        }
    }
    public Vector2Int FindRandomPixelPosition()
    {
        if (!currentColorBox.CanInteract()) return -Vector2Int.one;

        List<int> availableRows = new List<int>(artArray.GetLength(0));
        for (int i = 0; i < artArray.GetLength(0); i++)
            availableRows.Add(i);
        while (availableRows.Count > 0)
        {
            // Random 1 row chưa dùng
            int idx = UnityEngine.Random.Range(0, availableRows.Count);

            int row = availableRows[idx];

            // Xóa row này khỏi danh sách (không random lại lần sau)
            availableRows.RemoveAt(idx);

            // Duyệt row để tìm col phù hợp
            List<int> cols = new List<int>();
            int totalCols = artArray.GetLength(1);
            for (int j = 0; j < totalCols; j++)
            {
                if (artArray[row, j] == currentColorBox.number)
                {
                    Color color = bwTexture.GetPixel(row, j);
                    if (color.a > 0.1f) cols.Add(j);
                }
            }

            // Nếu row có target
            if (cols.Count > 0)
            {
                int col = cols[UnityEngine.Random.Range(0, cols.Count)];
                return new Vector2Int(row, col);
            }

            // Nếu row không có target → lặp lại vòng while, chọn row khác
        }
        return -Vector2Int.one;
    }
    public void SetHighlightIndex(int colorIndex, bool enable = true)
    {
        if (grayScaleMaterial == null) return;
        grayScaleMaterial.SetFloat("_SelectedIndex", enable ? colorIndex : -1);
        grayScaleMaterial.SetFloat("_BlinkOn", enable ? 1f : 0f);
        // tùy chọn tinh chỉnh:
        grayScaleMaterial.SetFloat("_BlinkSpeed", 5f);
        grayScaleMaterial.SetFloat("_BlinkMin", 0.2f);
        grayScaleMaterial.SetFloat("_BlinkMax", 0.8f);
    }

    public SpriteRenderer GetSpriteRendererGrayScale()
    {
        return spriteRendererGrayScale;
    }
    public void CheckWinCondition()
    {
        if (isGameWon) return;
        var allColorBoxes = ColorBoxPool.Instance.GetActiveColorBoxs();

        bool allCompleted = true;

        foreach (ColorBox box in allColorBoxes)
        {
            if (box.CanInteract())
            {
                allCompleted = false;
                break;
            }
        }
        if (allCompleted)
        {
            isGameWon = true;
            if (ReplaySystem.isReplaying) return;

            Debug.Log("Win");
            SaveLoadImage.SaveSpriteProgress(spriteRendererGrayScale.sprite);
            ReplaySystem.Instance.StopAndSaveRecording();
            AnimateCameraAndShowWinPopup();
        }
    }
    private void AnimateCameraAndShowWinPopup()
    {
        UIManager.Instance.SetInteractBackButton(false);
        float particleDuration = PaintingFX.Instance.fireworkParticle.isPlaying ? PaintingFX.Instance.GetFireworkDuration() : 0f;
        float duration = 1.2f;
        Sequence winSequence = DOTween.Sequence();
        winSequence.AppendInterval(particleDuration);
        winSequence.Append(Camera.main.transform.DOMove(InputHandler.Instance.GetWinCameraPosition(), duration).SetEase(Ease.InOutCubic));
        winSequence.Join(Camera.main.DOOrthoSize(InputHandler.Instance.GetInitialCameraSize(), duration).SetEase(Ease.InOutCubic)
            .OnUpdate(() =>
            {
                UpdateBorderByScale();
            }));

        winSequence.OnComplete(() =>
        {
            UIManager.Instance.ShowWinpopup();
        });
    }

    public ArtBoxSO GetCurrentArtBoxSO()
    {
        return currentArtSO;
    }

    public bool ToggleSparkleEffect()
    {
        if (currentArtSO == null) return false;
        if (!currentArtSO.canBlink)
        {
            Debug.Log("Watch Ads to Get Blinkblink");
            currentArtSO.canBlink = true;
        }
        currentArtSO.isBlink = !currentArtSO.isBlink;

        if (grayScaleMaterial != null)
        {
            grayScaleMaterial.SetFloat("_SparkleOn", currentArtSO.isBlink ? 1.0f : 0.0f);
            if (sparkleTexture != null)
            {
                grayScaleMaterial.SetTexture("_SparkleTex", sparkleTexture);
            }
        }
        return currentArtSO.isBlink;
    }

    public void UpdateSparkleEffect()
    {
        if (currentArtSO == null) return;
        if (grayScaleMaterial != null)
        {
            grayScaleMaterial.SetFloat("_SparkleOn", currentArtSO.isBlink ? 1.0f : 0.0f);
            if (sparkleTexture != null)
            {
                grayScaleMaterial.SetTexture("_SparkleTex", sparkleTexture);
            }
        }
    }

    private Texture2D CreateArtworkMaskTexture(bool[,] artworkArea)
    {
        int width = artworkArea.GetLength(0);
        int height = artworkArea.GetLength(1);

        Texture2D maskTex = new Texture2D(width, height, TextureFormat.R8, false);
        maskTex.filterMode = FilterMode.Point;

        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Nếu pixel này thuộc vùng ảnh, tô màu trắng (giá trị 255). Ngược lại tô đen (giá trị 0).
                byte value = artworkArea[x, y] ? (byte)255 : (byte)0;
                pixels[y * width + x] = new Color32(value, value, value, 255);
            }
        }

        maskTex.SetPixels32(pixels);
        maskTex.Apply();
        return maskTex;
    }


    //for replay
    public void ResetForReplay()
    {
        spriteRendererGrayScale.sprite = CreateGrayScaleSprite(spriteRenderer.sprite);
        bwTexture = spriteRendererGrayScale.sprite.texture;
    }

    public void PaintForReplay(int pixelX, int pixelY, int colorNumber)
    {
        for (int x = -paintRadiusPixel; x <= paintRadiusPixel; x++)
        {
            for (int y = -paintRadiusPixel; y <= paintRadiusPixel; y++)
            {
                int px = pixelX + x;
                int py = pixelY + y;
                if (px >= 0 && px < artArray.GetLength(0) && py >= 0 && py < artArray.GetLength(1))
                {
                    Color original = bwTexture.GetPixel(px, py);
                    if (original.a > minAlpha && artArray[px, py] == colorNumber)
                    {
                        original.a = 0;
                        bwTexture.SetPixel(px, py, original);
                    }
                }
            }
        }
    }

    public void OnReplayButtonClicked()
    {
        string artName = currentArtSO.sprite.name;
        ReplaySystem.Instance.PlayReplay(artName);
    }
    public void GetWin()
    {
        UIManager.Instance.winUI.SetActive(false);
        UIManager.Instance.ShowPreviewUI();
        UIManager.Instance.SetInteractBackButton(true);
        OnReplayButtonClicked();
        if (currentArtSO != null)
        {
            currentArtSO.isDone = true;
            DatabaseManager.Instance.AddToMyWork(currentArtSO);
            QuestManager.Instance.CheckImportAndPaintPictureQuest(currentArtSO);
            QuestManager.Instance.CheckAllColorPictureAchievements();
            QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Daily_1_Picture);
            QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Daily_3_Picture);
            QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Achievement_Picture);
            QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Achievement_Pictures_Same_Day);
        }
        TutorialManager.Instance.HideRealtimeTutorialIfActive();
    }
    public void ClearCurrentArtBoxSO()
    {
        this.currentArtSO = null;
    }

    public void CheatWin()
    {
        if (isGameWon || currentArtSO == null)
        {
            Debug.LogWarning("CheatWin can only be called during an active game.");
            return;
        }

        Debug.Log("--- CHEAT: AUTO-COMPLETING ARTWORK ---");

        var allColorBoxes = ColorBoxPool.Instance.GetActiveColorBoxs();
        if (allColorBoxes == null || allColorBoxes.Count == 0) return;

        Dictionary<int, ColorBox> colorBoxMap = new Dictionary<int, ColorBox>();
        foreach (var box in allColorBoxes)
        {
            colorBoxMap[box.number] = box;
        }

        int width = artArray.GetLength(0);
        int height = artArray.GetLength(1);

        for (int px = 0; px < width; px++)
        {
            for (int py = 0; py < height; py++)
            {
                if (bwTexture.GetPixel(px, py).a > minAlpha)
                {
                    int colorIndex = artArray[px, py];
                    if (colorIndex == 0) continue;

                    ReplaySystem.Instance.RecordAction(ReplayActionType.Paint, px, py, colorIndex);

                    Color pixelColor = bwTexture.GetPixel(px, py);
                    pixelColor.a = 0;
                    bwTexture.SetPixel(px, py, pixelColor);
                }
            }
        }
        bwTexture.Apply();

        foreach (var box in allColorBoxes)
        {
            if (box.CanInteract())
            {
                box.SetCurrentPaintedPixel(box.targetPixel);
                box.SetCompletedState();
            }
        }
        isGameWon = false;
        CheckWinCondition();
    }

    public bool CanUseBoosterAtPosition(Vector2 worldPos)
    {
        Vector2 pixelPos = WorldToPixel(worldPos);
        int px = (int)pixelPos.x;
        int py = (int)pixelPos.y;

        if (px >= 0 && px < bwTexture.width && py >= 0 && py < bwTexture.height)
        {
            if (PlayerManager.Instance.PaintBooster == Helper.PaintBooster.Fill)
            {
                if (artArray[px, py] == 0)
                {
                    return false;
                }
                FillAtPixel(px, py);
                return true;
            }
            else if (PlayerManager.Instance.PaintBooster == Helper.PaintBooster.Boom)
            {
                int radius = BoomManager.Instance.GetExplosionRadius();
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
                            if (original.a > minAlpha)
                            {
                                BoomAtPixel(px, py);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

        }
        return false;
    }
}
