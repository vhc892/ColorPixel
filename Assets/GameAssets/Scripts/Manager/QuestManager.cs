using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetUpQuestDict();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [SerializeField] private QuestDatabaseSO achievementSO;
    [SerializeField] private QuestDatabaseSO dailyQuestSO;

    [SerializeField] private Image dailyQuestButton;
    [SerializeField] private Image achievementButton;

    private int stateIndex = -1; //0: daily  1: achievement

    //Check ngày reset daily quest
    private DateTime lastCheck;
    private int dailyQuestAmount = 3;

    private QuestSO[] currentDailyQuest = new QuestSO[0];

    private Dictionary<Helper.QuestType, QuestSO> questDict = new Dictionary<Helper.QuestType, QuestSO>();

    void Start()
    {
        SetQuestInContainer(0);
        if (Helper.Daily.IsNewDay())
        {
            UpdateQuestProgress(Helper.QuestType.Achievement_Frequently);
        }
        lastCheck = DateTime.Now;
    }
    void Update()
    {
        if ((DateTime.Now - lastCheck).TotalSeconds > 60) // check mỗi phút
        {
            lastCheck = DateTime.Now;
            if (Helper.Daily.IsNewDay())
            {
                ResetDailyQuests();
                UpdateQuestProgress(Helper.QuestType.Achievement_Frequently);
            }
        }
    }

    private void SetUpQuestDict()
    {
        foreach (QuestSO quest in dailyQuestSO.database)
        {
            questDict[quest.type] = quest;
        }

        foreach (QuestSO quest in achievementSO.database)
        {
            questDict[quest.type] = quest;
        }
    }

    public void OnDailyQuestButton()
    {
        SetQuestInContainer(0);
        AudioManager.Instance.PressButtonSfx();
    }

    public void OnAchievementButton()
    {
        SetQuestInContainer(1);
        AudioManager.Instance.PressButtonSfx();
    }

    public void SetQuestInContainer(int index)
    {
        if (stateIndex == index) return;
        dailyQuestButton.DOKill();
        achievementButton.DOKill();
        stateIndex = index;
        QuestBoxPool.Instance.ReturnAllQuestBoxes();
        QuestSO[] database;
        if (index == 0)
        {
            //if (currentDailyQuest.Length == 0) ResetDailyQuests();
            database = currentDailyQuest;
            dailyQuestButton.DOFade(1, 0.5f);
            achievementButton.DOFade(0, 0.5f);
        }
        else
        {
            database = achievementSO.database;
            dailyQuestButton.DOFade(0, 0.5f);
            achievementButton.DOFade(1, 0.5f);
        }

        foreach (QuestSO questSO in database)
        {
            QuestBox questBox = QuestBoxPool.Instance.GetQuestBox();
            questBox.SetQuest(questSO);
        }
        Canvas.ForceUpdateCanvases();
    }

    public void UpdateQuestUI()
    {
        List<QuestBox> quests = QuestBoxPool.Instance.GetActiveQuestBoxs();
        foreach (QuestBox quest in quests)
        {
            quest.UpdateUI();
        }
    }

    public void UpdateQuestProgress(Helper.QuestType questType)
    {
        QuestSO questSO = questDict[questType];
        if (questSO == null) return;
        if (questSO.IsFinishQuest()) return;
        questSO.UpdateQuestProgress();
        if (currentDailyQuest.Contains(questSO) || !dailyQuestSO.database.Contains(questSO))
        {
            if (questSO.CanClaimReward())
            {
                UIManager.Instance.taskbarController.SetActiveQuestNoti(true);
                QuestNotiManager.Instance.ShowQuestNoti(questSO);
            }

            if (questSO.type == Helper.QuestType.Achievement_Pixel && questSO.currentProgress > 50)
            {
                // Show real-time Tut for first time
                if (PlayerPrefs.GetInt("HasSeenRealTimeTutorial", 0) == 0)
                {
                    TutorialManager.Instance.ShowRealtimeTutorial();
                    PlayerPrefs.SetInt("HasSeenRealTimeTutorial", 1);
                    PlayerPrefs.Save();
                }
            }
        }
        UpdateQuestUI();
    }

    public void SetQuestProgress(Helper.QuestType questType, int progress)
    {
        QuestSO questSO = questDict[questType];
        if (questSO == null) return;
        if (questSO.IsFinishQuest()) return;
        if (questSO != null) questSO.SetQuestProgress(progress);
        if (currentDailyQuest.Contains(questSO) || !dailyQuestSO.database.Contains(questSO))
        {
            if (questSO.CanClaimReward())
                QuestNotiManager.Instance.ShowQuestNoti(questSO);
        }
        UpdateQuestUI();
    }

    public void ResetDailyQuests()
    {
        //Reset Daily Quest
        Helper.ScriptTool.ShuffleArray(dailyQuestSO.database);
        foreach (QuestSO quest in dailyQuestSO.database)
        {
            quest.SetCurrentMileStone(0);
            quest.currentProgress = 0;
            quest.canShowNoti = true;
        }
        currentDailyQuest = dailyQuestSO.database.Take(dailyQuestAmount).ToArray();

        //Reset Achievement
        questDict[Helper.QuestType.Achievement_Pictures_Same_Day].currentProgress = 0;
        Debug.Log("ResetDailyQuests");
    }

    public Dictionary<Helper.QuestType, QuestSO> GetQuestDict()
    {
        return questDict;
    }

    public QuestSO[] CurrentDailyQuest
    {
        get { return currentDailyQuest; }
        set { currentDailyQuest = value; }
    }

    public void CheckImportAndPaintPictureQuest(ArtBoxSO artBoxSO)
    {
        if (artBoxSO.name.Contains("artbox_"))
            UpdateQuestProgress(Helper.QuestType.Achievement_Import_n_Paint);
    }

    public void CheckAllColorPictureAchievements()
    {
        int diffCategoryCount = 0; // số category có ít nhất 1 tranh done
        int cutePictureCount = 0;  // số tranh done trong category Cute
        int minPicturePerCategory = int.MaxValue; // min tranh done trong mọi category

        foreach (ConceptDatabaseSO concept in DatabaseManager.Instance.concepts)
        {
            int finishPicture = 0;
            bool hasDone = false;

            foreach (ArtBoxSO artBoxSO in concept.artBoxSOList)
            {
                if (artBoxSO.isDone)
                {
                    finishPicture++;
                    hasDone = true;

                    if (concept.type == Helper.ConceptType.Cute)
                    {
                        cutePictureCount++;
                    }
                }
            }

            if (hasDone)
                diffCategoryCount++;

            if (concept.artBoxSOList.Count > 0) // tránh category rỗng
                minPicturePerCategory = Mathf.Min(minPicturePerCategory, finishPicture);
        }

        if (minPicturePerCategory == int.MaxValue)
            minPicturePerCategory = 0; // nếu chưa có category nào thì cho = 0

        // set quest progress
        SetQuestProgress(Helper.QuestType.Achievement_Different_Category, diffCategoryCount);
        SetQuestProgress(Helper.QuestType.Achievement_Cute_Category_Picture, cutePictureCount);
        SetQuestProgress(Helper.QuestType.Achievement_Picture_Each_Category, minPicturePerCategory);
    }

    public void RandomReward(Vector3 pos, int rewardAmount)
    {
        StartCoroutine(CoroutineRandomReward(pos, rewardAmount));
    }

    private IEnumerator CoroutineRandomReward(Vector3 pos, int rewardAmount)
    {
        yield return new WaitForSeconds(1);
        int randomIndex = UnityEngine.Random.Range(0, 4);
        switch (randomIndex)
        {
            case 0:
                PlayerManager.Instance.AddCoinWithAnimation(10 * rewardAmount, pos);
                break;
            case 1:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: 1 * rewardAmount, boomAmount: 0, findAmount: 0);
                ShopBuyAnim.Instance.AnimateReward(pos, PlayerManager.Instance.coinContainer, Helper.RewardType.Fill, 1 * rewardAmount);
                break;
            case 2:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: 0, boomAmount: 1 * rewardAmount, findAmount: 0);
                ShopBuyAnim.Instance.AnimateReward(pos, PlayerManager.Instance.coinContainer, Helper.RewardType.Boom, 1 * rewardAmount);
                break;
            case 3:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: 0, boomAmount: 0, findAmount: 3 * rewardAmount);
                ShopBuyAnim.Instance.AnimateReward(pos, PlayerManager.Instance.coinContainer, Helper.RewardType.Find, 3 * rewardAmount);
                break;
        }
    }

    public bool HaveQuestCanClaim()
    {
        foreach (QuestSO quest in currentDailyQuest)
        {
            if (quest.CanClaimReward()) return true;
        }

        foreach (QuestSO quest in achievementSO.database)
        {
            if (quest.CanClaimReward()) return true;
        }
        return false;
    }

}
