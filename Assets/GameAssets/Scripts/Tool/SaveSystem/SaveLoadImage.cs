using UnityEngine;
using System.IO;

public class SaveLoadImage
{
    public static void SaveSpriteProgress(Sprite sprite)
    {
        SaveSprite(sprite, "_progress.png");
    }

    public static Sprite LoadSpriteProgress(Sprite sprite)
    {
        return LoadSprite(sprite, "_progress.png");
    }

    public static void DeleteSpriteProgress(Sprite sprite)
    {
        DeleteImage(sprite, "_progress.png");
    }

    public static Sprite LoadSpriteStickers(Sprite sprite)
    {
        return LoadSprite(sprite, "_stickers.png");
    }

    public static void DeleteSpriteStickers(Sprite sprite)
    {
        DeleteImage(sprite, "_stickers.png");
    }

    private static void SaveSprite(Sprite sprite, string tailPath)
    {
        string fileName = sprite.name + tailPath;
        string path = Path.Combine(Application.persistentDataPath, fileName);

        Texture2D tex = sprite.texture;
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        Debug.Log("Saved to: " + path);
    }

    private static Sprite LoadSprite(Sprite sprite, string tailPath)
    {
        string fileName = sprite.name + tailPath;
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning("File not found: " + path);
            return null;
        }

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        Sprite res = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        res.name = sprite.name;
        return res;
    }

    public static void SaveTexture(Texture2D tex, string name, string tailPath)
    {
        string fileName = name + tailPath;
        string path = Path.Combine(Application.persistentDataPath, fileName);

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        Debug.Log("Saved to: " + path);
    }
    
    private static void DeleteImage(Sprite sprite, string tailPath)
    {
        string fileName = sprite.name + tailPath;
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"{fileName} save file deleted.");
        }
    }
}