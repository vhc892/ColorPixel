using System;
using System.Collections.Generic;
using UnityEngine;
public class ArtBoxSaveSystem : BaseSaveSystem<ArtBoxSaveCollection>
{
    protected override string FileName => "artbox_save.json";
}

[Serializable]
public class ArtBoxSaveData
{
    public string soName;
    public bool isDone;
    public int bgIndex;
    public int frameIndex;
    public List<StickerData> stickerDatas;
    public bool isBlink;
    public bool canBlink;
    public bool isGlitter;
}

[Serializable]
public class ArtBoxSaveCollection
{
    public List<ArtBoxSaveData> artBoxes = new List<ArtBoxSaveData>();
}
