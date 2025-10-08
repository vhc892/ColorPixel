using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestBox : MonoBehaviour
{
    [SerializeField] private Image iconIMG;
    [SerializeField] private Image rewardTypeIMG;
    [SerializeField] private TextMeshProUGUI rewardAmountTMP;
    [SerializeField] private TextMeshProUGUI questNameTMP;
    [SerializeField] private TextMeshProUGUI questDescriptionTMP;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI progressBarTextTMP;
    [SerializeField] private Image claimButtonIMG;

    [SerializeField] private GameObject achievementContainer;
    [SerializeField] private GameObject dailyQuestContainer;
    [SerializeField] private Animator dailyGiftAnimator;

    public QuestSO questSO;


    public void SetQuest(QuestSO questSO)
    {
        this.questSO = questSO;

        // Nếu là Daily
        if (questSO.rewardType == Helper.RewardType.RandomForDaily)
        {
            achievementContainer.SetActive(false);
            dailyQuestContainer.SetActive(true);
            questNameTMP.alignment = TextAlignmentOptions.BottomLeft;
            SetDailyQuestGiftAnimator();
        }
        // Nếu là Achievement
        else
        {
            achievementContainer.SetActive(true);
            dailyQuestContainer.SetActive(false);

            if (questSO.icon != null) iconIMG.sprite = questSO.icon;
            SetRewardIcon();
            rewardAmountTMP.SetText(questSO.rewardAmount[questSO.GetCurrentMileStone()] + "");
            questNameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        }

        questNameTMP.SetText(questSO.questName);
        questDescriptionTMP.SetText(questSO.GetDescription());

        progressBarTextTMP.SetText(questSO.GetProgressText());
        SetClaimButton();
        progressBarFill.fillAmount
            = (float)Mathf.Clamp(questSO.currentProgress, 0, questSO.milestone[questSO.GetCurrentMileStone()])
            / questSO.milestone[questSO.GetCurrentMileStone()];
    }
    
    // private void OnEnable()
    // {
    //     if (questSO != null && questSO.rewardType == Helper.RewardType.RandomForDaily)
    //     {
    //         SetDailyQuestGiftAnimator();
    //     }
    // }

    private void SetDailyQuestGiftAnimator()
    {
        // Set Animator
        if (questSO.IsFinishQuest())
        {
            dailyGiftAnimator.SetTrigger("alreadyOpen");
            Debug.Log("alreadyOpen: " + questSO.IsFinishQuest());
        }
        else
        {
            dailyGiftAnimator.SetTrigger("startAnim");
        }

        dailyGiftAnimator.SetBool("canOpen", questSO.CanClaimReward());
    }

    public void UpdateUI(bool updateAnimator = true)
    {
        if (questSO == null) return;

        // Nếu là Daily
        if (questSO.rewardType == Helper.RewardType.RandomForDaily && updateAnimator)
        {
            SetDailyQuestGiftAnimator();
        }

        rewardAmountTMP.SetText(questSO.rewardAmount[questSO.GetCurrentMileStone()] + "");
        questNameTMP.SetText(questSO.questName);
        progressBarTextTMP.SetText(questSO.GetProgressText());
        SetClaimButton();
        progressBarFill.fillAmount
            = (float)Mathf.Clamp(questSO.currentProgress, 0, questSO.milestone[questSO.GetCurrentMileStone()])
            / questSO.milestone[questSO.GetCurrentMileStone()];
    }

    public void ClaimReward()
    {
        if (questSO == null) return;
        if (questSO.IsFinishQuest()) return;
        if (!questSO.CanClaimReward()) return;
        Vector3 pos = iconIMG.transform.position;
        dailyGiftAnimator.SetTrigger("open");
        switch (questSO.rewardType)
        {
            case Helper.RewardType.Coin:
                PlayerManager.Instance.AddCoinWithAnimation(questSO.rewardAmount[questSO.GetCurrentMileStone()], iconIMG.GetComponent<RectTransform>());
                break;
            case Helper.RewardType.Fill:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: questSO.rewardAmount[questSO.GetCurrentMileStone()], boomAmount: 0, findAmount: 0);
                ShopBuyAnim.Instance.AnimateReward(pos, PlayerManager.Instance.coinContainer, Helper.RewardType.Fill, questSO.rewardAmount[questSO.GetCurrentMileStone()]);
                break;
            case Helper.RewardType.Boom:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: 0, boomAmount: questSO.rewardAmount[questSO.GetCurrentMileStone()], findAmount: 0);
                ShopBuyAnim.Instance.AnimateReward(pos, PlayerManager.Instance.coinContainer, Helper.RewardType.Boom, questSO.rewardAmount[questSO.GetCurrentMileStone()]);
                break;
            case Helper.RewardType.Find:
                PlayerManager.Instance.PurchaseItem(cost: 0, fillAmount: 0, boomAmount: 0, findAmount: questSO.rewardAmount[questSO.GetCurrentMileStone()]);
                ShopBuyAnim.Instance.AnimateReward(pos, PlayerManager.Instance.coinContainer, Helper.RewardType.Find, questSO.rewardAmount[questSO.GetCurrentMileStone()]);
                break;
            case Helper.RewardType.NewPainting:
                break;
            case Helper.RewardType.Spin:
                break;
            case Helper.RewardType.RandomForDaily:
                QuestManager.Instance.RandomReward(pos, 1);
                break;
        }
        questSO.SetCurrentMileStone(Mathf.Clamp(questSO.GetCurrentMileStone() + 1, 0, questSO.milestone.Length));
        questSO.canShowNoti = true;
        UpdateUI(false);
        UIManager.Instance.taskbarController.SetActiveQuestNoti(QuestManager.Instance.HaveQuestCanClaim()); 
        AudioManager.Instance.PressButtonSfx();
    }

    

    public void SetClaimButton()
    {
        if (questSO.IsFinishQuest())
        {
            claimButtonIMG.sprite = GameAssets.i.nonClaimButton;
            return;
        }
        if (questSO.currentProgress < questSO.milestone[questSO.GetCurrentMileStone()])
        {
            claimButtonIMG.sprite = GameAssets.i.nonClaimButton;
        }
        else
        {
            claimButtonIMG.sprite = GameAssets.i.claimButton;
        }
    }

    public void SetRewardIcon()
    {
        switch (questSO.rewardType)
        {
            case Helper.RewardType.Coin:
                rewardTypeIMG.sprite = GameAssets.i.coin;
                break;
            case Helper.RewardType.Fill:
                rewardTypeIMG.sprite = GameAssets.i.fillBooster;
                break;
            case Helper.RewardType.Boom:
                rewardTypeIMG.sprite = GameAssets.i.boomBooster;
                break;
            case Helper.RewardType.Find:
                rewardTypeIMG.sprite = GameAssets.i.findBooster;
                break;
            case Helper.RewardType.NewPainting:
                //rewardTypeIMG.sprite = GameAssets.i.newPainting;
                break;
            case Helper.RewardType.Spin:
                rewardTypeIMG.sprite = GameAssets.i.spin;
                break;
        }
    }
}
