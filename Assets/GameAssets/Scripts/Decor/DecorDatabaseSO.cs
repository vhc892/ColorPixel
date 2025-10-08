using UnityEngine;

[CreateAssetMenu(fileName = "DecorDatabaseSO", menuName = "ScriptableObject/Database/DecorDatabaseSO")]
public class DecorDatabaseSO : ScriptableObject
{
    public Helper.DecorType type;
    public DecorSO[] decorSOs;
}
    
