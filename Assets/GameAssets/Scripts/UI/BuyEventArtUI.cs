using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyEventArtUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI adsText;
    [SerializeField] private Image art;
    [SerializeField] private Image grayArt;
    private EventArt currentEventArt;

    public void SetEventArt(EventArt eventArt)
    {
        currentEventArt = eventArt;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (currentEventArt == null) return;
        priceText.SetText(currentEventArt.GetPrice() + "");
        adsText.SetText(currentEventArt.eventArtDataSO.adsWatched + "/" + currentEventArt.GetAds());
        art.sprite = currentEventArt.GetFullColorSprite();
        grayArt.sprite = currentEventArt.GetGrayColorSprite();
    }

    public void OnPurchased()
    {
        if (currentEventArt == null) return;
        if (PlayerManager.Instance.CanMinusCoin(currentEventArt.GetPrice()))
        {
            PlayerManager.Instance.MinusCoin(currentEventArt.GetPrice());
            currentEventArt.Purchased();
            UIManager.Instance.HideBuyEventAdsPopup();
        }
        else
        {
            UIManager.Instance.ShowNotEnoughCoins(Input.mousePosition);
        }
    }

    public void OnAds()
    {
        if (currentEventArt == null) return;
        if (currentEventArt.eventArtDataSO.adsWatched < currentEventArt.GetAds())
        {
            currentEventArt.eventArtDataSO.adsWatched++;
            UpdateUI();
            Debug.Log("Watch Reward Ads while Buy Event Art");
        }

        if (currentEventArt.eventArtDataSO.adsWatched >= currentEventArt.GetAds())
        {
            currentEventArt.Purchased();
            UIManager.Instance.HideBuyEventAdsPopup();
        }
    }
}
