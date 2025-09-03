// LevelDataConverter.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

public class LevelDataConverter : EditorWindow
{
    // Chúng ta không cần định nghĩa sẵn palette nữa
    // private List<Color> colorPalette = new List<Color>();

    // Thay vào đó, chúng ta chỉ cần định nghĩa màu nền
    private Color backgroundColor = new Color32(0, 0, 0, 0); // Mặc định là màu hồng

    private Texture2D sourceTexture;

    [MenuItem("Tools/Level Data Converter V2")]
    public static void ShowWindow()
    {
        GetWindow<LevelDataConverter>("Level Data Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("Image to LevelData Converter (Auto-Palette)", EditorStyles.boldLabel);

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Image", sourceTexture, typeof(Texture2D), false);

        // Cho phép developer tùy chỉnh màu nền ngay trên giao diện
        backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);

        if (GUILayout.Button("Generate Level and Palette"))
        {
            if (sourceTexture != null)
            {
                ConvertTexture();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a source image first.", "OK");
            }
        }
    }

    private void ConvertTexture()
    {
        if (!sourceTexture.isReadable)
        {
            EditorUtility.DisplayDialog("Error", $"Texture '{sourceTexture.name}' is not readable. Please enable 'Read/Write Enabled' in its import settings.", "OK");
            return;
        }

        // --- BƯỚC 1: TỰ ĐỘNG XÂY DỰNG PALETTE ---
        List<Color> detectedPalette = new List<Color>();
        detectedPalette.Add(backgroundColor); // Luôn thêm màu nền vào vị trí index 0

        for (int y = 0; y < sourceTexture.height; y++)
        {
            for (int x = 0; x < sourceTexture.width; x++)
            {
                Color pixelColor = sourceTexture.GetPixel(x, y);

                // Nếu màu này không phải màu nền và chưa có trong palette
                if (pixelColor.a > 0 && !IsColorInPalette(pixelColor, detectedPalette))
                {
                    detectedPalette.Add(pixelColor);
                }
            }
        }

        // --- BƯỚC 2: TẠO MẢNG levelData DỰA TRÊN PALETTE VỪA TÌM ĐƯỢC ---
        int width = sourceTexture.width;
        int height = sourceTexture.height;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"// LevelData for {sourceTexture.name} ({width}x{height})");
        sb.AppendLine($"private int[,] levelData = new int[,]\n{{");

        for (int y = 0; y < height; y++)
        {
            sb.Append("    { ");
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = sourceTexture.GetPixel(x, (height - 1 - y));
                int colorID = FindColorIDInPalette(pixelColor, detectedPalette);
                sb.Append(colorID);
                if (x < width - 1) sb.Append(", ");
            }
            sb.Append(" }");
            if (y < height - 1) sb.Append(",");
            sb.AppendLine($" // Row {y}");
        }
        sb.AppendLine("};");

        // In mảng levelData ra Console
        Debug.Log("--- LevelData Generated --- \n" + sb.ToString());

        // --- BƯỚC 3: IN PALETTE RA CONSOLE ĐỂ COPY VÀO GAMEMANAGER ---
        StringBuilder paletteSb = new StringBuilder();
        paletteSb.AppendLine($"// Auto-generated Palette for {sourceTexture.name}");
        paletteSb.AppendLine($"// Please copy this into your GameManager's 'gameColors' list.");
        for (int i = 0; i < detectedPalette.Count; i++)
        {
            Color c = detectedPalette[i];
            paletteSb.AppendLine($"// Index {i}: new Color({c.r}f, {c.g}f, {c.b}f, {c.a}f);");
        }
        Debug.Log("--- Palette Generated --- \n" + paletteSb.ToString());

        EditorUtility.DisplayDialog("Success", "LevelData and Palette have been generated and printed to the Console.", "OK");
    }

    // Hàm phụ trợ để kiểm tra màu đã tồn tại trong palette chưa
    private bool IsColorInPalette(Color color, List<Color> palette)
    {
        foreach (Color c in palette)
        {
            if (Mathf.Approximately(c.r, color.r) &&
                Mathf.Approximately(c.g, color.g) &&
                Mathf.Approximately(c.b, color.b) &&
                Mathf.Approximately(c.a, color.a))
            {
                return true;
            }
        }
        return false;
    }

    // Hàm phụ trợ để tìm chỉ số của màu trong palette
    private int FindColorIDInPalette(Color color, List<Color> palette)
    {
        if (color.a == 0) return 0; // Coi các ô trong suốt hoàn toàn là nền

        for (int i = 0; i < palette.Count; i++)
        {
            if (IsColorInPalette(color, new List<Color> { palette[i] })) // Tái sử dụng hàm kiểm tra
            {
                return i;
            }
        }
        return 0; // Mặc định là nền nếu không tìm thấy
    }
}