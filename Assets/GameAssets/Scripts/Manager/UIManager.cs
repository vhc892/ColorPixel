using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private List<GameObject> screens = new();

    public GameObject menuUI;
    public SlidingTaskbarController taskbarController;

    [Header("Win popup")]
    public GameObject winUI;
    public GameObject winPopup;

    [Header("Shop popup")]
    public GameObject shopUI;
    public GameObject shopPopup;

    [Header("Spin popup")]
    public GameObject spinUI;
    public GameObject spinPopup;

    [Header("Setting popup")]
    public GameObject settingUI;
    public GameObject settingPopup;

    [Header("Tutorial popup")]
    public GameObject tutorialUI;
    public GameObject tutorialPopup;

    [Header("Buy Event Art popup")]
    public BuyEventArtUI buyEventArtUI;
    public GameObject buyEventArtPopup;

    [Header("Get More Booster popup")]
    public GetMoreBoosterUI getMoreBoosterUI;
    public GameObject getMoreBoosterPopup;

    [Header("Popup Animation Settings")]
    [SerializeField] private float backgroundFadeDuration = 0.5f;
    [SerializeField] private float popupScaleDuration = 0.5f;
    [SerializeField] private Ease popupEaseType = Ease.OutBack;
    [SerializeField] private Ease popupHideEaseType = Ease.InBack;

    [Header("Indices in 'screens'")]
    private int eventUI = 0;
    private int myWorkUI = 1;
    private int galleryUI = 2;
    private int questUI = 3;
    private int cameraUI = 4;
    private int gameplayUI = 5;
    private int previewUI = 6;
    private int eventGameplayUI = 7;

    [Header("Sparkle Toggle UI")]
    [SerializeField] private GameObject sparkleButton;
    [SerializeField] private RectTransform sparkleIconRect;
    [SerializeField] private RectTransform sparkleOnPosition;
    [SerializeField] private RectTransform sparkleOffPosition;
    [SerializeField] private float toggleAnimDuration = 0.25f;

    [Header("Button")]
    public Button gameplayBackButton;
    public Button previewBackButton;

    [Header("Not Enough Coins Text")]
    [SerializeField] private GameObject notEnoughCoinsNoti;
    [SerializeField] private Sequence textSeq = null;


    public static int currentGameConcept = 1;
    public bool isEvent = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        ShowOnly(screens[galleryUI]);
    }
    public void ShowOnly(GameObject target)
    {
        foreach (var s in screens) if (s) s.SetActive(false);
        if (target) target.SetActive(true);
    }

    public void HideAllScreen()
    {
        foreach (var s in screens) if (s) s.SetActive(false);
        menuUI.SetActive(false);
    }

    public void ShowGallary()
    {
        ShowOnly(screens[galleryUI]);
        AudioManager.Instance.PressButtonSfx();
    }
    public void ShowCameraUI()
    {
        ArtBoxCaptureSpawner.Instance.UpdateCameraArtBoxes();
        ShowOnly(screens[cameraUI]);
        AudioManager.Instance.PressButtonSfx();
    }
    public void ShowEventUI()
    {
        ShowOnly(screens[eventUI]);
        AudioManager.Instance.PressButtonSfx();
    }

    public void ShowQuestUI()
    {
        ShowOnly(screens[questUI]);
        QuestManager.Instance.UpdateQuestUI();
        AudioManager.Instance.PressButtonSfx();
    }

    public void ShowEventGameplayUI()
    {
        ShowOnly(screens[eventGameplayUI]);
        menuUI.SetActive(false);
        sparkleButton.SetActive(false);
    }

    public void ShowPreviewUI()
    {
        ShowOnly(screens[previewUI]);
        menuUI.SetActive(false);
    }

    public void ShowMyWorkUI()
    {
        ShowOnly(screens[myWorkUI]);
        DatabaseManager.Instance.UpdateMyWorkArtBoxs();
        AudioManager.Instance.PressButtonSfx();
    }
    public void ShowGamePlayUI(bool isEvent = false)
    {
        ShowOnly(screens[gameplayUI]);
        menuUI.SetActive(false);
        this.isEvent = isEvent;
        sparkleButton.SetActive(true);
    }

    private void ReturnToMainMenu(bool shouldSave)
    {
        var sr = CoreGameManager.Instance.GetSpriteRendererGrayScale();
        if (sr != null)
        {
            if (shouldSave && sr.sprite != null)
            {
                SaveLoadImage.SaveSpriteProgress(sr.sprite);
                ReplaySystem.Instance.StopAndSaveRecording();
            }
            sr.sprite = null;
        }
        DecorManager.Instance.SaveStickerCapture();
        foreach (ArtBox artBox in ArtBoxPool.Instance.GetArtBoxByArtBoxSO(CoreGameManager.Instance.GetCurrentArtBoxSO()))
        {
            if (artBox != null) artBox.UpdateArtBox();
        }

        menuUI.SetActive(true);
        DatabaseManager.Instance.UpdateHomeScreenArtBox();
        ShowGallary();
        StickerPool.Instance.ReturnAllStickers();
        taskbarController.ResetToDefaultState(false);
        InputHandler.Instance.TurnOffGameplayInput();
    }

    public void BackMainMenuFromGame()
    {
        if (!isEvent)
        {
            DecorManager.Instance.ReleasePreviewTexture();
            ReturnToMainMenu(true);
        }
        else
        {
            BackEventFromGame();
        }
        CoreGameManager.Instance.ClearCurrentArtBoxSO();
        AudioManager.Instance.PressButtonSfx();
        if (WheelManager.firstTimeShowSpin && PlayerManager.Instance.spinAmount > 0)
        {
            WheelManager.firstTimeShowSpin = false;
            ShowSpinPopup();
        }
    }
    public void BackMenuFromPreview()
    {
        if (!isEvent)
        {
            ReturnToMainMenu(false);
        }
        else
        {
            BackEventFromPreview();
        }
        CoreGameManager.Instance.ClearCurrentArtBoxSO();
        AudioManager.Instance.PressButtonSfx();
        if (WheelManager.firstTimeShowSpin && PlayerManager.Instance.spinAmount > 0)
        {
            WheelManager.firstTimeShowSpin = false;
            ShowSpinPopup();
        }
    }


    public void OnSparkleToggleButton()
    {
        bool isNowActive = CoreGameManager.Instance.ToggleSparkleEffect();
        AnimateSparkleToggle(isNowActive);
        AudioManager.Instance.PressButtonSfx();
        sparkleButton.transform.Find("Ads").gameObject.SetActive(!CoreGameManager.Instance.GetCurrentArtBoxSO().canBlink);
    }

    public void UpdateArtBoxSparkle()
    {
        sparkleButton.transform.Find("Ads").gameObject.SetActive(!CoreGameManager.Instance.GetCurrentArtBoxSO().canBlink);
        AnimateSparkleToggle(CoreGameManager.Instance.GetCurrentArtBoxSO().isBlink);
    }

    private void AnimateSparkleToggle(bool isOn)
    {
        if (isOn)
        {
            sparkleIconRect.DOAnchorPos(sparkleOnPosition.anchoredPosition, toggleAnimDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            sparkleIconRect.DOAnchorPos(sparkleOffPosition.anchoredPosition, toggleAnimDuration).SetEase(Ease.OutQuad);
        }
    }
    public void ShowWinpopup()
    {
        ShowPopup(winUI, winPopup);
        AudioManager.Instance.WinSfx();
    }
    public void ShowShopPopup()
    {
        ShowPopup(shopUI, shopPopup);
    }

    public void ShowSpinPopup()
    {
        ShowPopup(spinUI, spinPopup);
    }

    public void ShowBuyEventAdsPopup(EventArt eventArt)
    {
        ShowPopup(buyEventArtUI.gameObject, buyEventArtPopup);
        buyEventArtUI.SetEventArt(eventArt);
    }

    public void ShowSettingPopup()
    {
        ShowPopup(settingUI, settingPopup);
        SettingManager.Instance.UpdateButtonUI();
    }
    public void ShowGetMoreBoosterPopup(Helper.PaintBooster type)
    {
        ShowPopup(getMoreBoosterUI.gameObject, getMoreBoosterPopup);
        getMoreBoosterUI.SetType(type);
    }
    public void ShowTutorialPopup()
    {
        ShowPopup(tutorialUI, tutorialPopup, TutorialManager.Instance.BeginTutorial);
        TutorialManager.Instance.SetInteractableBoosterButtons(false);
    }
    public void HideTutorialPopup()
    {
        HidePopup(tutorialUI, tutorialPopup);
    }

    public void HideWinPopup()
    {
        HidePopup(winUI, winPopup);
    }
    public void HideShopPopup()
    {
        HidePopup(shopUI, shopPopup);
        AudioManager.Instance.PressButtonSfx();
    }

    public void HideSpinPopup()
    {
        if (WheelManager.GetIsSpinning()) return;
        HidePopup(spinUI, spinPopup);
        AudioManager.Instance.PressButtonSfx();
    }

    public void HideSettingPopup()
    {
        HidePopup(settingUI, settingPopup);
        AudioManager.Instance.PressButtonSfx();
    }

    public void HideGetMoreBoosterPopup()
    {
        HidePopup(getMoreBoosterUI.gameObject, getMoreBoosterPopup);
        AudioManager.Instance.PressButtonSfx();
    }

    public void HideBuyEventAdsPopup()
    {
        HidePopup(buyEventArtUI.gameObject, buyEventArtPopup);
        buyEventArtUI.SetEventArt(null);
    }
    private void ShowPopup(GameObject popupContainer, GameObject popupContent, System.Action onCompleteAction = null)
    {
        CanvasGroup backgroundCanvasGroup = popupContainer.GetComponent<CanvasGroup>();
        if (backgroundCanvasGroup == null)
        {
            Debug.LogError("Popup: " + popupContainer.name + " need CanvasGroup!");
            return;
        }
        AudioManager.Instance.PopupOpenSfx();
        popupContainer.SetActive(true);
        backgroundCanvasGroup.alpha = 0;
        backgroundCanvasGroup.interactable = false;

        backgroundCanvasGroup.DOFade(1, backgroundFadeDuration).SetEase(Ease.Linear);

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < popupContent.transform.childCount; i++)
        {
            Transform child = popupContent.transform.GetChild(i);
            child.localScale = Vector3.zero; // bắt đầu từ 0

            seq.Insert(i * 0.05f,
                child.DOScale(1, popupScaleDuration)
                    .SetEase(popupEaseType)
            );
        }

        seq.OnComplete(() =>
        {
            // enable background khi xong hết
            backgroundCanvasGroup.interactable = true;
            onCompleteAction?.Invoke();
        });
    }
    private void HidePopup(GameObject popupContainer, GameObject popupContent)
    {
        CanvasGroup backgroundCanvasGroup = popupContainer.GetComponent<CanvasGroup>();
        if (backgroundCanvasGroup == null)
        {
            Debug.LogError("Popup: " + popupContainer.name + " need CanvasGroup!");
            popupContainer.SetActive(false);
            return;
        }

        backgroundCanvasGroup.interactable = false;

        Sequence seq = DOTween.Sequence();
        for (int i = popupContent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = popupContent.transform.GetChild(i);

            seq.Insert((popupContent.transform.childCount - 1 - i) * 0.05f,
                child.DOScale(0, popupScaleDuration / 2f)
                    .SetEase(popupHideEaseType)
            );
        }

        seq.OnComplete(() =>
        {
            backgroundCanvasGroup.DOFade(0, backgroundFadeDuration / 2f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                popupContainer.SetActive(false);
            });
        });
    }

    public void BackEventFromGame()
    {
        var sr = CoreGameManager.Instance.GetSpriteRendererGrayScale();
        if (sr != null)
        {
            if (sr.sprite != null)
            {
                SaveLoadImage.SaveSpriteProgress(sr.sprite);
                ReplaySystem.Instance.StopAndSaveRecording();
            }
            sr.sprite = null;
        }
        DecorManager.Instance.SaveStickerCapture();
        foreach (ArtBox artBox in ArtBoxPool.Instance.GetArtBoxByArtBoxSO(CoreGameManager.Instance.GetCurrentArtBoxSO()))
        {
            if (artBox != null) artBox.UpdateArtBox();
        }
        DecorManager.Instance.ReleasePreviewTexture();
        menuUI.SetActive(false);
        ShowEventGameplayUI();
        StickerPool.Instance.ReturnAllStickers();

        EventGameManager.Instance.PlayEvent();
        sparkleButton.SetActive(false);
    }

    public void BackEventFromPreview()
    {
        DecorManager.Instance.SaveStickerCapture();
        foreach (ArtBox artBox in ArtBoxPool.Instance.GetArtBoxByArtBoxSO(CoreGameManager.Instance.GetCurrentArtBoxSO()))
        {
            if (artBox != null) artBox.UpdateArtBox();
        }

        menuUI.SetActive(false);
        ShowEventGameplayUI();
        StickerPool.Instance.ReturnAllStickers();
        InputHandler.Instance.SwitchToEventInput();
        EventGameManager.Instance.PlayEvent();
        sparkleButton.SetActive(false);
    }

    public void BackMenuFromEvent()
    {
        menuUI.SetActive(true);
        DatabaseManager.Instance.UpdateHomeScreenArtBox();
        ShowGallary();
        StickerPool.Instance.ReturnAllStickers();
        taskbarController.ResetToDefaultState(false);
        InputHandler.Instance.TurnOffGameplayInput();
        EventGameManager.Instance.HideEvent();
    }

    public PreviewUI GetPreviewUI()
    {
        return screens[previewUI].GetComponent<PreviewUI>();
    }

    public void SetInteractBackButton(bool interactable)
    {
        gameplayBackButton.interactable = interactable;
        previewBackButton.interactable = interactable;
    }
    
    public void ShowNotEnoughCoins(Vector3 position)
    {
        if (textSeq != null && textSeq.IsActive() && textSeq.IsPlaying()) textSeq.Kill();
        float moveUpDistance = 50f;
        float duration = 1f;
        notEnoughCoinsNoti.SetActive(true);
        notEnoughCoinsNoti.transform.position = position;

        var rect = notEnoughCoinsNoti.GetComponent<RectTransform>();
        // Nếu có Text
        var text = notEnoughCoinsNoti.GetComponent<TextMeshProUGUI>();

        textSeq = DOTween.Sequence();

        // Bay lên
        textSeq.Join(rect.DOAnchorPosY(rect.anchoredPosition.y + moveUpDistance, duration).SetEase(Ease.OutCubic));

        // Fade
        if (text != null)
        {
            textSeq.Join(text.DOFade(0f, duration * 1.5f).From(1f));
        }

        textSeq.OnComplete(() =>
        {
            notEnoughCoinsNoti.SetActive(false);
        });
    }
}
