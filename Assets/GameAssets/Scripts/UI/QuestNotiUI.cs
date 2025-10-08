using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestNotiUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questNameTMP;
    [SerializeField] private TextMeshProUGUI progressBarText;
    public Image progressBarFill;
    private int maxProgress = 0;
    public CanvasGroup canvasGroup;

    public void SetData(string questName, int maxProgressValue)
    {
        questNameTMP.text = questName;
        maxProgress = maxProgressValue;
        progressBarFill.fillAmount = 0;
        progressBarText.SetText(0 + "/" + maxProgress);
    }

    public void SetData(QuestNotiData questNotiData)
    {
        questNameTMP.text = questNotiData.name;
        maxProgress = questNotiData.maxProgress;
        progressBarFill.fillAmount = 0;
        progressBarText.SetText(0 + "/" + maxProgress);
    }

    public void SetProgress(float value)
    {
        progressBarFill.fillAmount = value;
        progressBarText.SetText(Mathf.CeilToInt(value * maxProgress) + "/" + maxProgress);
    }

    public void SetProgressText(float value)
    {
        progressBarText.SetText(Mathf.CeilToInt(value * maxProgress) + "/" + maxProgress);
    }
}
