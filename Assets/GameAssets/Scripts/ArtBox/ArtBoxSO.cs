using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtBoxSO", menuName = "ScriptableObject/Data/ArtBoxSO")]
public class ArtBoxSO : ScriptableObject
{
    public Sprite sprite;
    public bool ads;
    public bool isDone;
    public int bgIndex = -1;
    public int frameIndex = -1;
    public List<StickerData> stickerDatas = new List<StickerData>();

    public bool isBlink;
    public bool canBlink;
    public bool isGlitter;

    public StickerData GetStickerByName(string name)
    {
        foreach (StickerData stickerData in stickerDatas)
        {
            if (stickerData.name == name) return stickerData;
        }
        return null;
    }
}

[Serializable]
public class StickerData
{
    public string name;
    public int index;
    public Vector3 pos;
    public float rotationAngle;
    public float scale;

    public StickerData(string name, int index)
    {
        this.name = name;
        this.index = index;
        pos = Vector3.zero;
        rotationAngle = 0;
        scale = Sticker.DEFAULT_SELECTION_FRAME_SIZE;
    }
}
