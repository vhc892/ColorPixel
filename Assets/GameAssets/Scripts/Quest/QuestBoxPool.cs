using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBoxPool : MonoBehaviour
{
    public static QuestBoxPool Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private QuestBox questBoxPrefab;
    [SerializeField] private Transform questBoxContainer;
    private Queue<QuestBox> questBoxPool = new Queue<QuestBox>();
    private List<QuestBox> activeQuestBoxs = new List<QuestBox>();

    public QuestBox GetQuestBox()
    {
        if (questBoxPool.Count > 0)
        {
            QuestBox questBox = questBoxPool.Dequeue();
            activeQuestBoxs.Add(questBox);
            questBox.gameObject.SetActive(true);
            questBox.transform.SetAsLastSibling();
            return questBox;
        }
        else
        {
            QuestBox newQuestBox = Instantiate(questBoxPrefab, questBoxContainer);
            activeQuestBoxs.Add(newQuestBox);
            newQuestBox.gameObject.SetActive(true);
            newQuestBox.transform.SetAsLastSibling();
            return newQuestBox;
        }
    }

    public void ReturnQuestBox(QuestBox questBox)
    {
        if (activeQuestBoxs.Contains(questBox))
        {
            activeQuestBoxs.Remove(questBox);
            questBoxPool.Enqueue(questBox);
            questBox.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("QuestBox not found in active lines.");
        }
    }

    public void ReturnAllQuestBoxes()
    {
        foreach (var questBox in activeQuestBoxs)
        {
            questBox.gameObject.SetActive(false);
            questBoxPool.Enqueue(questBox);
        }
        activeQuestBoxs.Clear();
    }

    public List<QuestBox> GetActiveQuestBoxs()
    {
        return activeQuestBoxs;
    }

    public QuestBox GetQuestBoxByNumber(int number)
    {
        if (number > 0 && number <= activeQuestBoxs.Count)
            return activeQuestBoxs[number - 1];
        return null;
    }
}
