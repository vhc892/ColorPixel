using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingButtonUI : MonoBehaviour
{
    [SerializeField] private Image image;


    public void SetButton(bool isOn, float duration = 0.5f)
    {
        image.gameObject.SetActive(true);
        int alpha = isOn ? 1 : 0;
        image.DOKill();
        image.DOFade(alpha, duration).From(1-alpha);
    }

    public Image GetSelectedImage()
    {
        return image;
    }

}
