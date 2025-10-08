using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DecorBox : MonoBehaviour, IPointerClickHandler
{
    private int index;
    private Image image;
    [SerializeField] private GameObject adsIcon;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PressButtonSfx();
        if (adsIcon.gameObject.activeSelf)
        {
            Debug.Log("Watch Reward Ads while choosing Decor");
        }
        DecorManager.Instance.Decorate(index);
    }

    public void SetData(int index, DecorSO decorSO)
    {
        this.index = index;
        image.sprite = decorSO.sprite;
        adsIcon.SetActive(decorSO.isAds);
    }
}
