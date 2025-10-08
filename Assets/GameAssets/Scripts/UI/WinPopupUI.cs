using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WinPopupUI : MonoBehaviour
{
    public Animation giftAnim;      // Animation component
    public GameObject[] buttons;

    void OnEnable()
    {
        foreach (GameObject button in buttons)
        {
            button.SetActive(false);
        }

        StartCoroutine(PlayGiftSequence());
    }

    IEnumerator PlayGiftSequence()
    {
        giftAnim.Play("GiftBox_Open");
        Debug.Log(giftAnim["GiftBox_Open"].length);
        yield return new WaitForSeconds(giftAnim["GiftBox_Open"].length);

        // Fade từng button lần lượt
        foreach (GameObject button in buttons)
        {
            button.SetActive(true);

            // Lấy hoặc thêm CanvasGroup
            CanvasGroup cg = button.GetComponent<CanvasGroup>();
            if (cg == null) cg = button.AddComponent<CanvasGroup>();

            // Fade vào trong 0.5s
            cg.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).From(0f);

            // Delay nhỏ giữa các button để nhìn đẹp hơn
            yield return new WaitForSeconds(0.15f);
        }
    }

    public void GetReward()
    {
        PlayerManager.Instance.AddCoinWithAnimation(10, GetComponent<RectTransform>());
    }
    
    public void GetAdsReward()
    {
        Debug.Log("Watch reward ads while Claim win coin");
        PlayerManager.Instance.AddCoinWithAnimation(20, GetComponent<RectTransform>());
    }
}
