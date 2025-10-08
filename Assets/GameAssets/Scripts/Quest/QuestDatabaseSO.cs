using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabaseSO", menuName = "ScriptableObject/Database/QuestDatabaseSO")]
public class QuestDatabaseSO : ScriptableObject
{
    public QuestSO[] database;
}
