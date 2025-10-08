using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCapture
{
    private static Rect GetSpriteWorldRect(Transform tf, float spriteSize = 512f, float ppu = 100f)
    {
        Vector3 scale = tf.localScale;

        float worldW = (spriteSize / ppu) * scale.x;
        float worldH = (spriteSize / ppu) * scale.y;

        Vector3 center = tf.position;
        Vector3 min = new Vector3(center.x - worldW / 2f, center.y - worldH / 2f, center.z);
        Vector3 max = new Vector3(center.x + worldW / 2f, center.y + worldH / 2f, center.z);

        return new Rect(min.x, min.y, worldW, worldH);
    }

    private static Rect GetScreenRect(Camera cam, Transform tf, float spriteSize = 512f, float ppu = 100f)
    {
        Rect worldRect = GetSpriteWorldRect(tf, spriteSize, ppu);

        Vector3 minScreen = cam.WorldToScreenPoint(new Vector3(worldRect.xMin, worldRect.yMin, tf.position.z));
        Vector3 maxScreen = cam.WorldToScreenPoint(new Vector3(worldRect.xMax, worldRect.yMax, tf.position.z));

        return new Rect(minScreen.x, minScreen.y, maxScreen.x - minScreen.x, maxScreen.y - minScreen.y);
    }

    public static Texture2D CaptureSpriteRegion(Camera cam, Transform tf, bool onlyStickerLayer)
    {
        int width = 1024, height = 1024;
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        if (onlyStickerLayer)
        {
            // Chỉ render Sticker
            cam.cullingMask = 1 << LayerMask.NameToLayer("Sticker");
        }
        else
        {
            // Render tất cả trừ Ignore
            int ignoreLayer = LayerMask.NameToLayer("CameraIgnore");
            cam.cullingMask = ~ (1 << ignoreLayer);
        }

        cam.Render();

        RenderTexture.active = rt;

        Rect screenRect = GetScreenRect(cam, tf);
        Texture2D tex = new Texture2D((int)screenRect.width, (int)screenRect.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(screenRect, 0, 0);
        tex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.Destroy(rt);

        return tex;
    }
}
