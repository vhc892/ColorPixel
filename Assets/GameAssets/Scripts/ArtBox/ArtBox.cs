using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArtBox : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private static Vector3 defaultScale = Vector3.one * 4f;
    public ArtBoxSO artBoxSO;
    [SerializeField] private Image fullColorArt;
    [SerializeField] private Image grayColorArt;
    [SerializeField] private Image frame;
    [SerializeField] private Image sticker;
    [SerializeField] private GameObject tick;
    [SerializeField] private GameObject ads;
    [SerializeField] private SparkleRandom sparkleRandom;

    private Image bg;

    void Awake()
    {
        bg = GetComponent<Image>();
        if (bg == null) Debug.Log("Art Box BG NULL!");
    }



    public void SetArtBox(ArtBoxSO artSO)
    {
        this.artBoxSO = artSO;
        fullColorArt.sprite = artBoxSO.sprite;
        if (bg == null) bg = GetComponent<Image>();
        UpdateArtBox();
    }

    public void UpdateArtBox()
    {
        Sprite loadSprite = SaveLoadImage.LoadSpriteProgress(artBoxSO.sprite);
        if (loadSprite == null)
        {
            grayColorArt.sprite = CoreGameManager.Instance.CreateGrayScaleSprite(artBoxSO.sprite);
        }
        else
        {
            grayColorArt.sprite = loadSprite;
        }

        if (artBoxSO.bgIndex > 0) Debug.Log(bg);
        else Debug.Log(GameAssets.i.artBoxContainer);

        // Set Background
        bg.sprite = artBoxSO.bgIndex <= 0 ?
        GameAssets.i.artBoxContainer : DecorManager.Instance.GetDecorSprite(Helper.DecorType.Background, artBoxSO.bgIndex);


        // Set Frame
        if (artBoxSO.frameIndex <= 0)
        {
            frame.gameObject.SetActive(false);
        }
        else
        {
            frame.gameObject.SetActive(true);
            frame.sprite = DecorManager.Instance.GetDecorSprite(Helper.DecorType.Frame, artBoxSO.frameIndex);
        }

        // Set Stickers
        Sprite stickersSprite = SaveLoadImage.LoadSpriteStickers(artBoxSO.sprite);
        if (stickersSprite == null)
        {
            sticker.gameObject.SetActive(false);
        }
        else
        {
            sticker.gameObject.SetActive(true);
            sticker.sprite = stickersSprite;
        }
        // set blink
        if (sparkleRandom != null)
        {
            if (artBoxSO.isBlink)
            {
                sparkleRandom.gameObject.SetActive(true);
                sparkleRandom.ActivateSparkle();
            }
            else
            {
                sparkleRandom.gameObject.SetActive(false);
            }
        }

        tick.SetActive(artBoxSO.isDone);
        ads.SetActive(artBoxSO.ads);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ScrollRectLocker.Instance.draggingHorizontal || ScrollRectLocker.Instance.draggingVertical) return;
        AudioManager.Instance.PressButtonSfx();
        if (artBoxSO.ads)
        {
            artBoxSO.ads = false;
            Debug.Log("Watch Reward Ads while play");
            //return;
        }
        UIManager.Instance.ShowGamePlayUI();
        CoreGameManager.Instance.StartGame(artBoxSO);
        DecorManager.Instance.ResetDecorate();

        if (artBoxSO.isDone)
        {
            Debug.Log("show preview");
            UIManager.Instance.ShowPreviewUI();
            Camera.main.orthographicSize = InputHandler.Instance.GetInitialCameraSize();
            Camera.main.transform.position = InputHandler.Instance.GetWinCameraPosition();
            DecorManager.Instance.LoadDecorate();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ScrollRect parentScroll = transform.parent.parent.parent.GetComponent<ScrollRect>();
        if (parentScroll == null) return;
        if (!parentScroll.name.Contains("Scroll ArtBox"))
            parentScroll.OnBeginDrag(eventData); // gọi scrollRect gốc
        ScrollRectLocker.Instance.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ScrollRect parentScroll = transform.parent.parent.parent.GetComponent<ScrollRect>();
        if (parentScroll == null) return;
        if (!parentScroll.name.Contains("Scroll ArtBox"))
            parentScroll.OnDrag(eventData);
        ScrollRectLocker.Instance.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ScrollRect parentScroll = transform.parent.parent.parent.GetComponent<ScrollRect>();
        if (parentScroll == null) return;
        if (!parentScroll.name.Contains("Scroll ArtBox"))
            parentScroll.OnEndDrag(eventData);
        ScrollRectLocker.Instance.OnEndDrag(eventData);
    }
}
