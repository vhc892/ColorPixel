using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class QuestNotiManager : MonoBehaviour
{
    public static QuestNotiManager Instance;

    [SerializeField] private QuestNotiUI questNotiUI;   // gán object duy nhất trong scene
    [SerializeField] private float fadeDuration = 0.3f; // thời gian fade in/out
    [SerializeField] private float progressDuration = 1f; // thời gian thanh chạy đầy
    [SerializeField] private float stayTime = 1f;      // giữ sau khi thanh đầy (1s)

    private Queue<QuestNotiData> queue = new Queue<QuestNotiData>();
    private bool isShowing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questNotiUI.canvasGroup.alpha = 0f;
        questNotiUI.gameObject.SetActive(false);
    }

    public void ShowQuestNoti(string questName, int maxProgressValue)
    {
        Debug.Log($"Add to queue: {questName}");
        queue.Enqueue(new QuestNotiData(questName, maxProgressValue));
        if (!isShowing)
            StartCoroutine(ProcessQueue());
    }

    public void ShowQuestNoti(QuestSO questSO)
    {
        if (!questSO.canShowNoti) return;
        questSO.canShowNoti = false;
        Debug.Log($"Add to queue: {questSO.questName}");
        queue.Enqueue(new QuestNotiData(questSO.questName, questSO.milestone[questSO.GetCurrentMileStone()]));
        if (!isShowing)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (queue.Count > 0)
        {
            QuestNotiData questNotiData = queue.Dequeue();

            questNotiUI.gameObject.SetActive(true);
            questNotiUI.SetData(questNotiData);
            questNotiUI.SetProgress(0f);

            // Fade in panel
            yield return questNotiUI.canvasGroup
                .DOFade(1f, fadeDuration)
                .WaitForCompletion();

            // Chạy progress từ 0 -> 1 trong progressDuration
            yield return questNotiUI
                .progressBarFill
                .DOFillAmount(1f, progressDuration)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    questNotiUI.SetProgressText(questNotiUI.progressBarFill.fillAmount);
                })
                .WaitForCompletion();

            // Giữ lại 1 giây sau khi đầy
            yield return new WaitForSeconds(stayTime);

            // Fade out panel
            yield return questNotiUI.canvasGroup
                .DOFade(0f, fadeDuration)
                .WaitForCompletion();

            questNotiUI.gameObject.SetActive(false);
        }

        isShowing = false;
    }
}

public class QuestNotiData
{
    public string name;
    public int maxProgress;

    public QuestNotiData(string name, int maxProgress)
    {
        this.name = name;
        this.maxProgress = maxProgress;
    }
}
