using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetMoreBoosterUI : MonoBehaviour
{
    [SerializeField] private Helper.PaintBooster boosterType;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private RectTransform buyButton;

    [SerializeField] private RectTransform fillBoosterButton;
    [SerializeField] private RectTransform bombBoosterButton;
    [SerializeField] private RectTransform findBoosterButton;

    public void SetType(Helper.PaintBooster type)
    {
        boosterType = type;
        UpdateUI();
    }

    private void UpdateUI()
    {
        switch (boosterType)
        {
            case Helper.PaintBooster.None:
                return;
            case Helper.PaintBooster.Fill:
                itemImage.sprite = GameAssets.i.getBoosterUI_fill;
                amountText.SetText("x1");
                break;
            case Helper.PaintBooster.Boom:
                itemImage.sprite = GameAssets.i.getBoosterUI_bomb;
                amountText.SetText("x1");
                break;
            case Helper.PaintBooster.Find:
                itemImage.sprite = GameAssets.i.getBoosterUI_find;
                amountText.SetText("x5");
                break;
        }
    }

    public void OnBuyButton()
    {
        if (!PlayerManager.Instance.CanMinusCoin(10))
        {
            UIManager.Instance.ShowNotEnoughCoins(buyButton.position);
            return;
        }
        switch (boosterType)
        {
            case Helper.PaintBooster.None:
                return;
            case Helper.PaintBooster.Fill:
                PlayerManager.Instance.PurchaseItem(cost: 10, fillAmount: 1, boomAmount: 0, findAmount: 0);
                ShopBuyAnim.Instance.AnimateReward(itemImage.GetComponent<RectTransform>(), fillBoosterButton, Helper.RewardType.Fill, 1);
                break;
            case Helper.PaintBooster.Boom:
                PlayerManager.Instance.PurchaseItem(cost: 10, fillAmount: 0, boomAmount: 1, findAmount: 0);
                ShopBuyAnim.Instance.AnimateReward(itemImage.GetComponent<RectTransform>(), bombBoosterButton, Helper.RewardType.Boom, 1);
                break;
            case Helper.PaintBooster.Find:
                PlayerManager.Instance.PurchaseItem(cost: 10, fillAmount: 0, boomAmount: 0, findAmount: 5);
                ShopBuyAnim.Instance.AnimateReward(itemImage.GetComponent<RectTransform>(), findBoosterButton, Helper.RewardType.Find, 5);
                break;
        }
        UIManager.Instance.HideGetMoreBoosterPopup();
    }

    public void OnAdsButton()
    {
        Debug.Log("Watch Reward Ads when Buy Booster");
        switch (boosterType)
        {
            case Helper.PaintBooster.None:
                return;
            case Helper.PaintBooster.Fill:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: 1, boomAmount: 0, findAmount: 0);
                ShopBuyAnim.Instance.AnimateReward(itemImage.GetComponent<RectTransform>(), fillBoosterButton, Helper.RewardType.Fill, 1);
                break;
            case Helper.PaintBooster.Boom:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: 0, boomAmount: 1, findAmount: 0);
                ShopBuyAnim.Instance.AnimateReward(itemImage.GetComponent<RectTransform>(), bombBoosterButton, Helper.RewardType.Boom, 1);
                break;
            case Helper.PaintBooster.Find:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: 0, boomAmount: 0, findAmount: 5);
                ShopBuyAnim.Instance.AnimateReward(itemImage.GetComponent<RectTransform>(), findBoosterButton, Helper.RewardType.Find, 5);
                break;
        }
        UIManager.Instance.HideGetMoreBoosterPopup();
    }
}
