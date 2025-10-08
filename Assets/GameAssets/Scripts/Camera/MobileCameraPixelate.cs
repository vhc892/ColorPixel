using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using NativeGalleryNamespace;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class MobileCameraPixelate : MonoBehaviour
{

    public static event Action<string> OnShotSaved;
    public static event Action<Sprite> OnShotSpriteReady;

    void Start()
    {
    }

    public async void TakePictureWithNativeCamera()
    {
        AudioManager.Instance.PressButtonSfx();
        if (NativeCamera.IsCameraBusy())
            return;

        NativeCamera.Permission permission = await NativeCamera.RequestPermissionAsync(true);

        if (permission == NativeCamera.Permission.Granted)
        {
            // Chụp ảnh và nhận đường dẫn của file ảnh qua callback
            NativeCamera.TakePicture(ProcessImageCallback, 1024); // Giới hạn kích thước ảnh tối đa là 1024px
        }
        else if (permission == NativeCamera.Permission.Denied)
        {
            Debug.LogError("User denied camera permission.");
        }
    }
    public async void GetPictureFromNativeGallery()
    {
        AudioManager.Instance.PressButtonSfx();
        // Tránh mở picker khi đang bận
        if (NativeGallery.IsMediaPickerBusy())
            return;

        var permission = await NativeGallery.RequestPermissionAsync(
            NativeGallery.PermissionType.Read,
            NativeGallery.MediaType.Image
        );

        if (permission == NativeGallery.Permission.Granted)
        {
            NativeGallery.GetImageFromGallery(
                (path) =>
                {
                    ProcessImageCallback(path);
                },
                "Pick image",
                "image/*"
            );
        }
        else if (permission == NativeGallery.Permission.Denied)
        {
            Debug.LogError("User denied Photos/Gallery permission.");
        }
        else
        {
            Debug.LogWarning("Photos/Gallery permission not granted yet (ShouldAsk/Unknown).");
        }
    }



    private void ProcessImageCallback(string path)
    {
        if (path != null)
        {
            Debug.Log("Image path: " + path);

            // Tải ảnh từ đường dẫn vào một Texture2D
            Texture2D texture = NativeCamera.LoadImageAtPath(path, 1024, false);
            if (texture == null)
            {
                Debug.LogError("Couldn't load texture from " + path);
                return;
            }
            QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Daily_Import_Picture);
            StartCoroutine(ProcessImageRoutine(texture));
        }
        else
        {
            Debug.Log("Operation cancelled.");
        }
    }

    private IEnumerator ProcessImageRoutine(Texture2D sourceTexture)
    {
        const int TARGET_SIZE = 128;

        //  Tạo uid duy nhất cho mỗi lần chụp
        string uid = System.DateTime.Now.ToString("yyyyMMdd_HHmmssfff");

        // 1) Resize về 128x128 bằng RenderTexture (Point filter để khỏi mờ)
        RenderTexture rt = RenderTexture.GetTemporary(TARGET_SIZE, TARGET_SIZE, 0, RenderTextureFormat.ARGB32);
        rt.filterMode = FilterMode.Point;
        RenderTexture prev = RenderTexture.active;

        Graphics.Blit(sourceTexture, rt);
        RenderTexture.active = rt;

        // 2) Đọc pixel sang Texture2D "shot" (preview/UI + làm gốc để xử lý)
        Texture2D shot = new Texture2D(TARGET_SIZE, TARGET_SIZE, TextureFormat.RGBA32, false);
        shot.ReadPixels(new Rect(0, 0, TARGET_SIZE, TARGET_SIZE), 0, 0);
        shot.Apply();
        shot.filterMode = FilterMode.Point;
        shot.name = $"shot_{uid}"; // ★ tên duy nhất cho texture

        // 3) Dọn dẹp
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        if (sourceTexture) Destroy(sourceTexture);

        // --- PHẦN XỬ LÝ ẢNH 
        Debug.Log("Generating dynamic palette from 128x128 image...");
        Color32[] dynamicPalette = KMeansColorQuantizer.GeneratePalette(shot, 32);
        Debug.Log("Dynamic palette generated.");
        QuantizeNoDither(shot, dynamicPalette); // xử lý vào 'shot' trực tiếp

        // 5) Lưu PNG (tùy chọn)
        string dir = Application.persistentDataPath;
        string filename = $"pixel_processed_{uid}.png"; 
        string savePath = System.IO.Path.Combine(dir, filename);
        System.IO.File.WriteAllBytes(savePath, shot.EncodeToPNG());
        Debug.Log($"Saved processed image to: {savePath}");
        OnShotSaved?.Invoke(savePath);

        // 6)  TẠO BẢN SAO HOÀN TOÀN ĐỘC LẬP cho ArtBox (không dùng chung texture với UI)
        Texture2D shotCopy = new Texture2D(shot.width, shot.height, TextureFormat.RGBA32, false);
        shotCopy.SetPixels(shot.GetPixels());
        shotCopy.Apply();
        shotCopy.filterMode = FilterMode.Point;
        shotCopy.name = $"shotCopy_{uid}"; // tên duy nhất cho texture ArtBox

        var artboxSprite = Sprite.Create(
            shotCopy,
            new Rect(0, 0, shotCopy.width, shotCopy.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        artboxSprite.name = $"artbox_{uid}"; // tên duy nhất cho sprite ArtBox

        OnShotSpriteReady?.Invoke(artboxSprite);

        yield break;
    }


    // Tìm màu gần nhất trong palette (khoảng cách Euclid trong RGB)
    static Color32 NearestInPalette(Color c, Color32[] palette)
    {
        // Làm việc ở không gian gamma cho đơn giản
        float cr = c.r, cg = c.g, cb = c.b;
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < palette.Length; i++)
        {
            float dr = cr - (palette[i].r / 255f);
            float dg = cg - (palette[i].g / 255f);
            float db = cb - (palette[i].b / 255f);
            float d = dr * dr + dg * dg + db * db;
            if (d < bestDist) { bestDist = d; best = i; }
        }
        return palette[best];
    }

    // Lượng tử hoá không dithering (nhanh)
    static void QuantizeNoDither(Texture2D tex, Color32[] palette)
    {
        int w = tex.width, h = tex.height;
        var pixels = tex.GetPixels();  // Color[]
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = (Color)NearestInPalette(pixels[i], palette);
        tex.SetPixels(pixels);
        tex.Apply(false, false);
    }
}


/// <summary>
/// Lớp tĩnh để tạo bảng màu động từ một texture bằng thuật toán Median Cut.
/// </summary>
public static class ColorQuantizer
{
    // Lớp nội bộ để quản lý một "xô" chứa các pixel
    private class ColorBucket
    {
        public List<Color32> Pixels = new List<Color32>();
        private byte _minR, _maxR, _minG, _maxG, _minB, _maxB;

        public ColorBucket(List<Color32> pixels)
        {
            Pixels = pixels;
            CalculateBounds();
        }

        // Tính toán khoảng màu (màu tối nhất và sáng nhất) trong xô
        private void CalculateBounds()
        {
            if (Pixels.Count == 0) return;

            _minR = 255; _maxR = 0;
            _minG = 255; _maxG = 0;
            _minB = 255; _maxB = 0;

            foreach (var p in Pixels)
            {
                if (p.r < _minR) _minR = p.r;
                if (p.r > _maxR) _maxR = p.r;
                if (p.g < _minG) _minG = p.g;
                if (p.g > _maxG) _maxG = p.g;
                if (p.b < _minB) _minB = p.b;
                if (p.b > _maxB) _maxB = p.b;
            }
        }

        // Tìm ra kênh màu có dải rộng nhất (R, G, hoặc B)
        public char GetLongestAxis()
        {
            int rRange = _maxR - _minR;
            int gRange = _maxG - _minG;
            int bRange = _maxB - _minB;

            if (rRange >= gRange && rRange >= bRange) return 'r';
            if (gRange >= rRange && gRange >= bRange) return 'g';
            return 'b';
        }

        // Tính màu trung bình của tất cả các pixel trong xô
        public Color32 GetAverageColor()
        {
            if (Pixels.Count == 0) return new Color32(0, 0, 0, 255);

            long r = 0, g = 0, b = 0;
            foreach (var p in Pixels)
            {
                r += p.r;
                g += p.g;
                b += p.b;
            }
            return new Color32(
                (byte)(r / Pixels.Count),
                (byte)(g / Pixels.Count),
                (byte)(b / Pixels.Count),
                255
            );
        }
    }

    /// <summary>
    /// Tạo một bảng màu từ texture.
    /// </summary>
    /// <param name="texture">Texture nguồn</param>
    /// <param name="colorCount">Số lượng màu mong muốn trong bảng màu
    /// <returns>Mảng màu Color32[] là bảng màu đã tạo</returns>
    public static Color32[] GeneratePalette(Texture2D texture, int colorCount)
    {
        var pixels = texture.GetPixels32().ToList();
        var initialBucket = new ColorBucket(pixels);

        var buckets = new List<ColorBucket> { initialBucket };

        // Liên tục chia đôi xô màu có dải màu lớn nhất
        // cho đến khi đạt được số lượng màu mong muốn.
        while (buckets.Count < colorCount)
        {
            // Tìm xô có dải màu lớn nhất để chia
            ColorBucket bucketToSplit = null;
            int maxRange = -1;
            foreach (var bucket in buckets)
            {
                int rRange = bucket.GetLongestAxis() == 'r' ? 1 : 0;
                int gRange = bucket.GetLongestAxis() == 'g' ? 1 : 0;
                int bRange = bucket.GetLongestAxis() == 'b' ? 1 : 0;
                int currentRange = rRange + gRange + bRange;
                if (currentRange > maxRange)
                {
                    maxRange = currentRange;
                    bucketToSplit = bucket;
                }
            }

            if (bucketToSplit == null || bucketToSplit.Pixels.Count < 2) break; // Không thể chia được nữa

            // Sắp xếp các pixel trong xô theo kênh màu dài nhất
            char axis = bucketToSplit.GetLongestAxis();
            switch (axis)
            {
                case 'r': bucketToSplit.Pixels.Sort((a, b) => a.r.CompareTo(b.r)); break;
                case 'g': bucketToSplit.Pixels.Sort((a, b) => a.g.CompareTo(b.g)); break;
                case 'b': bucketToSplit.Pixels.Sort((a, b) => a.b.CompareTo(b.b)); break;
            }

            // Chia đôi xô tại vị trí trung vị (median)
            int medianIndex = bucketToSplit.Pixels.Count / 2;
            var newBucket1 = new ColorBucket(bucketToSplit.Pixels.Take(medianIndex).ToList());
            var newBucket2 = new ColorBucket(bucketToSplit.Pixels.Skip(medianIndex).ToList());

            // Xóa xô cũ và thêm 2 xô mới
            buckets.Remove(bucketToSplit);
            buckets.Add(newBucket1);
            buckets.Add(newBucket2);
        }

        // Lấy màu trung bình từ mỗi xô để tạo bảng màu cuối cùng
        var palette = new Color32[buckets.Count];
        for (int i = 0; i < buckets.Count; i++)
        {
            palette[i] = buckets[i].GetAverageColor();
        }

        return palette;
    }
}

/// <summary>
/// Lớp tĩnh để tạo bảng màu động từ một texture bằng thuật toán K-Means Clustering.
/// </summary>
public static class KMeansColorQuantizer
{
    /// <summary>
    /// Tạo một bảng màu từ texture bằng K-Means.
    /// </summary>
    /// <param name="texture">Texture nguồn</param>
    /// <param name="colorCount">Số lượng màu mong muốn (số cụm K)</param>
    /// <param name="maxIterations">Số lần lặp tối đa để thuật toán hội tụ</param>
    /// <returns>Mảng màu Color32[] là bảng màu đã tạo</returns>
    public static Color32[] GeneratePalette(Texture2D texture, int colorCount, int maxIterations = 10)
    {
        var pixels = texture.GetPixels32();
        var pixelList = new List<Color32>(pixels);

        // 1. Khởi tạo các tâm cụm (centroids) ban đầu
        Color32[] centroids = InitializeCentroids(pixelList, colorCount);

        // Danh sách các cụm, mỗi cụm là một danh sách các pixel
        var clusters = new List<List<Color32>>(colorCount);

        for (int i = 0; i < maxIterations; i++)
        {
            // Xóa và tạo lại các cụm cho mỗi lần lặp
            clusters.Clear();
            for (int c = 0; c < colorCount; c++)
            {
                clusters.Add(new List<Color32>());
            }

            // 2. Gán mỗi pixel vào cụm có tâm gần nhất
            foreach (var pixel in pixelList)
            {
                int nearestCentroidIndex = FindNearestCentroid(pixel, centroids);
                clusters[nearestCentroidIndex].Add(pixel);
            }

            // 3. Cập nhật lại các tâm cụm
            for (int c = 0; c < colorCount; c++)
            {
                if (clusters[c].Count > 0)
                {
                    centroids[c] = CalculateAverageColor(clusters[c]);
                }
            }
        }

        return centroids;
    }

    // Khởi tạo các tâm bằng cách chọn ngẫu nhiên các pixel từ ảnh
    private static Color32[] InitializeCentroids(List<Color32> pixels, int colorCount)
    {
        var random = new System.Random();
        var centroids = new Color32[colorCount];
        // Tạo một bản sao để không làm thay đổi danh sách pixel gốc
        var distinctPixels = pixels.Distinct().ToList();

        for (int i = 0; i < colorCount; i++)
        {
            if (i < distinctPixels.Count)
            {
                int index = random.Next(distinctPixels.Count);
                centroids[i] = distinctPixels[index];
                // Xóa pixel đã chọn để đảm bảo các tâm ban đầu là duy nhất
                distinctPixels.RemoveAt(index);
            }
            else
            {
                // Nếu ảnh có ít hơn colorCount màu, ta tạo màu ngẫu nhiên
                centroids[i] = new Color32((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), 255);
            }
        }
        return centroids;
    }

    // Tìm chỉ số của tâm cụm gần nhất với một pixel
    private static int FindNearestCentroid(Color32 pixel, Color32[] centroids)
    {
        int bestIndex = 0;
        int minDistance = int.MaxValue;

        for (int i = 0; i < centroids.Length; i++)
        {
            int dist = ColorDistanceSq(pixel, centroids[i]);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    // Tính bình phương khoảng cách màu (nhanh hơn tính căn bậc hai)
    private static int ColorDistanceSq(Color32 c1, Color32 c2)
    {
        int dr = c1.r - c2.r;
        int dg = c1.g - c2.g;
        int db = c1.b - c2.b;
        return dr * dr + dg * dg + db * db;
    }

    // Tính màu trung bình của một danh sách các pixel
    private static Color32 CalculateAverageColor(List<Color32> pixels)
    {
        long r = 0, g = 0, b = 0;
        foreach (var p in pixels)
        {
            r += p.r;
            g += p.g;
            b += p.b;
        }
        return new Color32(
            (byte)(r / pixels.Count),
            (byte)(g / pixels.Count),
            (byte)(b / pixels.Count),
            255
        );
    }
}
