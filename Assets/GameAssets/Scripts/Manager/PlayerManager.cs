using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Helper;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

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

    public int coin = 0;
    public int fillBoosterNumber = 0;
    public int boomBoosterNumber = 0;
    public int findBoosterNumber = 0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinUI;
    public CoinProgressBar coinProgressBar;

    [Header("Boom")]
    public TextMeshProUGUI boomNumberText;
    public GameObject boomSelectedUI;
    public GameObject boomAddUI;

    [Header("Fill")]
    public TextMeshProUGUI fillNumberText;
    public GameObject fillSelectedUI;
    public GameObject fillAddUI;

    [Header("Find")]
    public TextMeshProUGUI findNumberText;
    public GameObject findAddUI;

    [Header("Animation References")]
    public RectTransform coinContainer;
    [SerializeField] private RectTransform freeGoldButton;
    [SerializeField] private RectTransform buyFillButton;
    [SerializeField] private RectTransform buyBoomButton;
    [SerializeField] private RectTransform buyFindButton;
    [SerializeField] private RectTransform buyPack1Button;
    [SerializeField] private RectTransform buyPack2Button;

    [HideInInspector] public int spinAmount;
    public bool IsPlayingAnim { get; set; } = false;


    private InputState inputState = InputState.None;
    private PaintBooster paintBooster = PaintBooster.None;


    public InputState InputState { get => inputState; set => inputState = value; }
    public PaintBooster PaintBooster { get => paintBooster; set => paintBooster = value; }

    void Start()
    {
        UpdateCoinUI();
        UpdateBoosterUI();
        DeselectAllBoosters();
        StartCoroutine(CallUpdateNewDayNextFrame());
    }

    private IEnumerator CallUpdateNewDayNextFrame()
    {
        yield return null;
        Daily.UpdateNewDay();
    }

    public void AddCoin(int coinAdd)
    {
        coin += coinAdd;
        UpdateCoinUI();
    }

    public bool CanMinusCoin(int coinMinus)
    {
        return coin >= coinMinus;
    }

    public void MinusCoin(int coinMinus)
    {
        if (!CanMinusCoin(coinMinus)) return;
        coin -= coinMinus;
        UpdateCoinUI();
    }

    public void UpdateCoinUI()
    {
        coinUI.SetText(coin + "");
    }
    public void UpdateBoosterUI()
    {
        boomNumberText.SetText(boomBoosterNumber.ToString());
        fillNumberText.SetText(fillBoosterNumber.ToString());
        findNumberText.SetText(findBoosterNumber.ToString());

        boomAddUI.SetActive(boomBoosterNumber <= 0);
        fillAddUI.SetActive(fillBoosterNumber <= 0);
        findAddUI.SetActive(findBoosterNumber <= 0);
    }

    public void AddFillBooster(int fillBooster)
    {
        fillBoosterNumber += fillBooster;
        UpdateBoosterUI();
    }

    public void AddBoomBooster(int boomBooster)
    {
        boomBoosterNumber += boomBooster;
        UpdateBoosterUI();
    }

    public void AddFindBooster(int findBooster)
    {
        findBoosterNumber += findBooster;
        UpdateBoosterUI();
    }

    public void OnFillBooster()
    {
        if (IsPlayingAnim) return;
        AudioManager.Instance.PressButtonSfx();

        if (fillBoosterNumber <= 0)
        {
            // Xem Quang cao
            UIManager.Instance.ShowGetMoreBoosterPopup(PaintBooster.Fill);
            return;
        }

        if (paintBooster == PaintBooster.Fill)
        {
            DeselectAllBoosters();
        }
        else
        {
            paintBooster = PaintBooster.Fill;
            fillSelectedUI.SetActive(true);
            boomSelectedUI.SetActive(false);
        }
    }
    public void OnBoomBooster()
    {
        if (IsPlayingAnim) return;
        AudioManager.Instance.PressButtonSfx();
        if (boomBoosterNumber <= 0)
        {
            UIManager.Instance.ShowGetMoreBoosterPopup(PaintBooster.Boom);
            return;
        }
        if (paintBooster == PaintBooster.Boom)
        {
            DeselectAllBoosters();
        }
        else
        {
            paintBooster = PaintBooster.Boom;
            boomSelectedUI.SetActive(true);
            fillSelectedUI.SetActive(false);
        }
    }
    public void OnFindBooster()
    {
        if (IsPlayingAnim) return;
        AudioManager.Instance.PressButtonSfx();
        if (findBoosterNumber > 0)
        {
            findBoosterNumber--;
            CoreGameManager.Instance.ZoomAtValidPixel();
            UpdateBoosterUI();
            QuestManager.Instance.UpdateQuestProgress(QuestType.Daily_3_Boosters);
            QuestManager.Instance.UpdateQuestProgress(QuestType.Achievement_Find);
        }
        else
        {
            UIManager.Instance.ShowGetMoreBoosterPopup(PaintBooster.Find);
            // Xem Quang cao
        }
    }
    public void DeselectAllBoosters()
    {
        paintBooster = PaintBooster.None;
        if (boomSelectedUI != null) boomSelectedUI.SetActive(false);
        if (fillSelectedUI != null) fillSelectedUI.SetActive(false);
    }

    // ----- SHOP -----
    public void PurchaseItem(int cost, int fillAmount, int boomAmount, int findAmount)
    {
        if (coin >= cost)
        {
            AddCoin(-cost);
            if (fillAmount > 0) AddFillBooster(fillAmount);
            if (boomAmount > 0) AddBoomBooster(boomAmount);
            if (findAmount > 0) AddFindBooster(findAmount);
        }
        else
        {
            Debug.Log("Not enough coin");
        }
    }

    public void GetFreeGold()
    {
        bool isProgressBarFull = coinProgressBar.IncreaseProgress();

        if (isProgressBarFull)
        {
            StartCoroutine(RewardAndResetAfterDelay());
        }
        else
        {
            AddCoinWithAnimation(10, freeGoldButton);
        }
    }
    private IEnumerator RewardAndResetAfterDelay()
    {
        yield return new WaitForSeconds(coinProgressBar.animationDuration);
        AddCoinWithAnimation(110, freeGoldButton);
        coinProgressBar.ResetProgress();
    }
    public void AddCoinWithAnimation(int amount, RectTransform startPosition)
    {
        int startingDisplayCoin = coin;
        coin += amount;

        ShopBuyAnim.Instance.AnimateReward(startPosition, coinContainer, RewardType.Coin, amount);

        DOTween.To(() => startingDisplayCoin, x => startingDisplayCoin = x, coin, 0.8f)
            .SetDelay(0.8f)
            .OnUpdate(() =>
            {
                coinUI.SetText(startingDisplayCoin.ToString());
            })
            .SetEase(Ease.Linear);
    }

    public void AddCoinWithAnimation(int amount, Vector3 pos)
    {
        int startingDisplayCoin = coin;
        coin += amount;

        ShopBuyAnim.Instance.AnimateReward(pos, coinContainer, RewardType.Coin, amount);

        DOTween.To(() => startingDisplayCoin, x => startingDisplayCoin = x, coin, 0.8f)
            .SetDelay(0.8f)
            .OnUpdate(() =>
            {
                coinUI.SetText(startingDisplayCoin.ToString());
            })
            .SetEase(Ease.Linear);
    }

    public void BuyFillBooster()
    {
        if (!CanMinusCoin(10))
        {
            UIManager.Instance.ShowNotEnoughCoins(Input.mousePosition);
            return;
        }
        PurchaseItem(cost: 10, fillAmount: 1, boomAmount: 0, findAmount: 0);
        ShopBuyAnim.Instance.AnimateReward(buyFillButton, coinContainer, RewardType.Fill, 1);
    }

    public void BuyBoomBooster()
    {
        if (!CanMinusCoin(10))
        {
            UIManager.Instance.ShowNotEnoughCoins(Input.mousePosition);
            return;
        }
        PurchaseItem(cost: 10, fillAmount: 0, boomAmount: 1, findAmount: 0);
        ShopBuyAnim.Instance.AnimateReward(buyBoomButton, coinContainer, RewardType.Boom, 1);
    }

    public void BuyFindBoosters()
    {
        if (!CanMinusCoin(10))
        {
            UIManager.Instance.ShowNotEnoughCoins(Input.mousePosition);
            return;
        }
        PurchaseItem(cost: 10, fillAmount: 0, boomAmount: 0, findAmount: 5);
        ShopBuyAnim.Instance.AnimateReward(buyFindButton, coinContainer, RewardType.Find, 5);
    }

    public void BuyPack1()
    {
        if (!CanMinusCoin(100))
        {
            UIManager.Instance.ShowNotEnoughCoins(Input.mousePosition);
            return;
        }
        PurchaseItem(cost: 100, fillAmount: 5, boomAmount: 8, findAmount: 15);
        ShopBuyAnim.Instance.AnimateReward(buyPack1Button, coinContainer, RewardType.Fill, 5);
        ShopBuyAnim.Instance.AnimateReward(buyPack1Button, coinContainer, RewardType.Boom, 8);
        ShopBuyAnim.Instance.AnimateReward(buyPack1Button, coinContainer, RewardType.Find, 15);
    }

    public void BuyPack2()
    {
        if (!CanMinusCoin(190))
        {
            UIManager.Instance.ShowNotEnoughCoins(Input.mousePosition);
            return;
        }
        PurchaseItem(cost: 190, fillAmount: 10, boomAmount: 16, findAmount: 30);
        ShopBuyAnim.Instance.AnimateReward(buyPack2Button, coinContainer, RewardType.Fill, 10);
        ShopBuyAnim.Instance.AnimateReward(buyPack2Button, coinContainer, RewardType.Boom, 16);
        ShopBuyAnim.Instance.AnimateReward(buyPack2Button, coinContainer, RewardType.Find, 30);
    }
}
