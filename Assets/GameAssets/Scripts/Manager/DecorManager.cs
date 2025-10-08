using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecorManager : MonoBehaviour
{
    public static DecorManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ResetDecorate();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public Camera captureCamera;
    public Texture2D PreviewTempTex { get; private set; }

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private SpriteRenderer frame;
    [SerializeField] private DecorDatabaseSO[] database;
    private Helper.DecorType decorType = Helper.DecorType.Background;

    void Update()
    {
        if (!UIManager.Instance.GetPreviewUI().GetDecorObject().activeInHierarchy) return;
        if (Input.GetMouseButtonDown(0)) // click chuột trái
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            UnselectAllStickers();
            if (hit == null)
            {
                return;
            }

            Sticker sticker = hit.transform.GetComponentInParent<Sticker>();
            if (sticker != null)
            {
                sticker.OnSelectButton();
            }
        }
    }

    public void UnselectAllStickers()
    {
        foreach (Sticker sticker in StickerPool.Instance.GetActiveStickers())
        {
            sticker.OnFinishButton();
        }
    }
    
    public void LoadDecorate()
    {
        ArtBoxSO currentArtBoxSO = CoreGameManager.Instance.GetCurrentArtBoxSO();
        if (currentArtBoxSO == null) return;
        // Debug.Log(currentArtBoxSO.bgIndex);
        // Debug.Log(currentArtBoxSO.frameIndex);
        //Load Background
        if (currentArtBoxSO.bgIndex > 0)
        {
            background.sprite = GetDecorDatabaseByType(Helper.DecorType.Background).decorSOs[currentArtBoxSO.bgIndex].sprite;
            background.gameObject.SetActive(true);
        }

        //Load Frame
        if (currentArtBoxSO.frameIndex > 0)
        {
            frame.sprite = GetDecorDatabaseByType(Helper.DecorType.Frame).decorSOs[currentArtBoxSO.frameIndex].sprite;
            frame.gameObject.SetActive(true);
        }

        //Load Sticker
        foreach (StickerData stickerData in currentArtBoxSO.stickerDatas)
        {
            Sticker sticker = StickerPool.Instance.GetSticker();
            sticker.SetData(stickerData);
            sticker.SetActiveFrame(false);
        }
    }

    public void ResetDecorate()
    {
        background.sprite = GameAssets.i.emptyBG;
        frame.gameObject.SetActive(false);
        StickerPool.Instance.ReturnAllStickers();
    }

    public DecorDatabaseSO GetDecorDatabaseByType(Helper.DecorType type)
    {
        foreach (DecorDatabaseSO decorDatabaseSO in database)
        {
            if (decorDatabaseSO.type == type) return decorDatabaseSO;
        }
        return null;
    }

    public Sprite GetDecorSprite(Helper.DecorType type, int index)
    {
        foreach (DecorDatabaseSO decorDatabaseSO in database)
        {
            if (decorDatabaseSO.type == type) return decorDatabaseSO.decorSOs[index].sprite;
        }
        return null;
    }

    public void UpdateDecorType(Helper.DecorType type)
    {
        decorType = type;
        DecorBoxPool.Instance.ReturnAllDecorBoxes();
        DecorDatabaseSO decorDatabaseSO = GetDecorDatabaseByType(decorType);
        int i = 0;
        foreach (DecorSO decorSO in decorDatabaseSO.decorSOs)
        {
            DecorBox decorBox = DecorBoxPool.Instance.GetDecorBox();
            decorBox.SetData(i, decorSO);
            i++;
        }
    }

    public void Decorate(int index)
    {
        ArtBoxSO currentArtBoxSO = CoreGameManager.Instance.GetCurrentArtBoxSO();
        if (currentArtBoxSO == null) return;

        if (decorType == Helper.DecorType.Background)
        {
            if (index <= 0)
            {
                background.sprite = GameAssets.i.emptyBG;
                currentArtBoxSO.bgIndex = index;
            }
            else
            {
                DecorSO decorSO = GetDecorDatabaseByType(decorType).decorSOs[index];
                if (decorSO.isAds)
                {
                    Debug.Log("Show Reward Ads");
                    decorSO.isAds = false;
                }
                if (!decorSO.isAds)
                {
                    background.sprite = GetDecorDatabaseByType(decorType).decorSOs[index].sprite;
                    background.gameObject.SetActive(true);
                    currentArtBoxSO.bgIndex = index;
                }
            }
        }
        else if (decorType == Helper.DecorType.Frame)
        {
            if (index <= 0)
            {
                frame.gameObject.SetActive(false);
                currentArtBoxSO.frameIndex = index;
            }
            else
            {
                DecorSO decorSO = GetDecorDatabaseByType(decorType).decorSOs[index];
                if (decorSO.isAds)
                {
                    Debug.Log("Show Reward Ads");
                    decorSO.isAds = false;
                }
                if (!decorSO.isAds)
                {
                    frame.sprite = GetDecorDatabaseByType(decorType).decorSOs[index].sprite;
                    frame.gameObject.SetActive(true);
                    currentArtBoxSO.frameIndex = index;
                }
            }
        }
        else if (decorType == Helper.DecorType.Sticker)
        {
            if (currentArtBoxSO.stickerDatas.Count >= 10) return;
            DecorSO decorSO = GetDecorDatabaseByType(decorType).decorSOs[index];
            if (decorSO.isAds)
            {
                Debug.Log("Show Reward Ads");
                decorSO.isAds = false;
            }
            if (!decorSO.isAds)
            {
                UnselectAllStickers();
                Sticker sticker = StickerPool.Instance.GetSticker();
                StickerData stickerData = new StickerData(sticker.name, index);
                currentArtBoxSO.stickerDatas.Add(stickerData);
                sticker.SetData(stickerData);
                sticker.SetActiveFrame(true);
            }
        }
    }

    public BoxCollider2D GetBackgroundArea()
    {
        return background.GetComponent<BoxCollider2D>();
    }

    public void SaveStickerCapture()
    {
        ArtBoxSO artBoxSO = CoreGameManager.Instance.GetCurrentArtBoxSO();
        if (artBoxSO == null) return;
        if (!artBoxSO.isDone) return;
        if (artBoxSO.stickerDatas.Count == 0)
        {
            SaveLoadImage.DeleteSpriteStickers(artBoxSO.sprite);
            return;
        }
        if (StickerPool.Instance.GetActiveStickers().Count == 0) return;
        Texture2D tex = SpriteCapture.CaptureSpriteRegion(captureCamera, background.transform, true);
        SaveLoadImage.SaveTexture(tex, artBoxSO.sprite.name, "_stickers.png");
    }
    public void CreatePreviewTexture()
    {
        ReleasePreviewTexture();
        var area = GetBackgroundArea();
        PreviewTempTex = SpriteCapture.CaptureSpriteRegion(captureCamera, area.transform, false);
    }

    public void ReleasePreviewTexture()
    {
        if (PreviewTempTex != null)
        {
            Destroy(PreviewTempTex);
            PreviewTempTex = null;
        }
    }

}
