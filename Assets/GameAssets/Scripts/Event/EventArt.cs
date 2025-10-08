using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventArt : MonoBehaviour, IPointerClickHandler
{
    public ArtBoxSO artBoxSO;
    [SerializeField] private SpriteRenderer fullColorArt;
    [SerializeField] private SpriteRenderer grayColorArt;
    public EventArtDataSO eventArtDataSO;

    private bool isTutorialTarget = false;
    private System.Action onTutorialClickCallback;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isTutorialTarget)
        {
            onTutorialClickCallback?.Invoke();
            isTutorialTarget = false;
        }
        AudioManager.Instance.PressButtonSfx();
        if (eventArtDataSO.isPurchased)
        {
            StartGame();
        }
        else
        {
            UIManager.Instance.ShowBuyEventAdsPopup(this);
        }

    }

    void Start()
    {
        fullColorArt.sprite = artBoxSO.sprite;
    }

    public void StartGame()
    {
        UIManager.Instance.ShowGamePlayUI(true);
        CoreGameManager.Instance.StartGame(artBoxSO);
        DecorManager.Instance.ResetDecorate();
        EventGameManager.Instance.currentEventGame.gameObject.SetActive(false);

        if (artBoxSO.isDone)
        {
            Debug.Log("show preview");
            UIManager.Instance.ShowPreviewUI();
            Camera.main.orthographicSize = InputHandler.Instance.GetInitialCameraSize();
            Camera.main.transform.position = InputHandler.Instance.GetWinCameraPosition();
            DecorManager.Instance.LoadDecorate();
        }
    }

    public void Purchased()
    {
        eventArtDataSO.isPurchased = true;
        StartGame();
    }

    public void UpdateEventArt()
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
    }

    public int GetAds()
    {
        float spriteSize = artBoxSO.sprite.rect.height;

        if (spriteSize < 64) return 1;
        return Mathf.RoundToInt(spriteSize / 64f);
    }

    public int GetPrice()
    {
        float spriteSize = artBoxSO.sprite.rect.height;
        if (spriteSize < 40) return 20;
        return 20 + 30 * Mathf.RoundToInt(spriteSize / 64f);
    }

    public Sprite GetGrayColorSprite()
    {
        return grayColorArt.sprite;
    }

    public Sprite GetFullColorSprite()
    {
        return fullColorArt.sprite;
    }
    public void SetupForTutorial(System.Action onClickCallback)
    {
        isTutorialTarget = true;
        onTutorialClickCallback = onClickCallback;
    }
}
