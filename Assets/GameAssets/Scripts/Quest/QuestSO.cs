using UnityEngine;

[CreateAssetMenu(fileName = "QuestSO", menuName = "ScriptableObject/Data/QuestSO")]
public class QuestSO : ScriptableObject
{
    [Header("Quest UI")]
    public Sprite icon;

    [Header("Quest Info")]
    public Helper.QuestType type;
    public string questName;
    [SerializeField] private string description;
    public int[] milestone;
    [SerializeField]private int currentMilestone;
    public int currentProgress;

    [Header("Reward Info")]
    public Helper.RewardType rewardType;
    public int[] rewardAmount;

    public bool canShowNoti = true;

    public string GetDescription()
    {
        if (milestone != null && milestone.Length > 0)
        {
            return description.Replace("_", milestone[GetCurrentMileStone()].ToString());
        }
        return description;
    }

    public string GetProgressText()
    {
        return Mathf.Clamp(currentProgress, 0, milestone[milestone.Length - 1]) + "/" + milestone[GetCurrentMileStone()];
    }

    public bool IsFinishQuest()
    {
        return currentMilestone >= milestone.Length;
    }

    public void UpdateQuestProgress()
    {
        if (IsFinishQuest()) return;
        currentProgress = Mathf.Clamp(currentProgress + 1, 0, milestone[milestone.Length - 1]);
    }

    public void SetQuestProgress(int progress)
    {
        if (IsFinishQuest()) return;
        currentProgress = Mathf.Clamp(progress, 0, milestone[milestone.Length - 1]);
    }

    public bool CanClaimReward()
    {
        if (currentMilestone >= milestone.Length) return false;
        return currentProgress >= milestone[GetCurrentMileStone()];
    }

    public int GetCurrentMileStone()
    {
        return Mathf.Clamp(currentMilestone, 0, milestone.Length - 1);
    }

    public int GetRealCurrentMileStone()
    {
        return currentMilestone;
    }

    public void SetCurrentMileStone(int milestone)
    {
        currentMilestone = milestone;
    }
}
