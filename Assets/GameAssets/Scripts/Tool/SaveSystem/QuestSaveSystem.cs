using System.Collections;
using System.Collections.Generic;
using Helper;

public class QuestSaveSystem : BaseSaveSystem<QuestDataList>
{
    protected override string FileName => "quest_save.json";
}

[System.Serializable]
public class QuestDataList
{
    public List<QuestData> list = new List<QuestData>();
    public List<QuestType> dailyQuest = new List<QuestType>();
}

[System.Serializable]
public class QuestData
{
    public QuestType questType;
    public int currentProgress;
    public int currentMilestone;
    public bool canShowNoti;
}