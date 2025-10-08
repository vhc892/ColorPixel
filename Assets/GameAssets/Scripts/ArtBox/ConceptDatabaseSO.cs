using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConceptDatabaseSO", menuName = "ScriptableObject/Database/ConceptDatabaseSO")]
public class ConceptDatabaseSO : ScriptableObject
{
    public Helper.ConceptType type;
    //public ArtBoxSO[] artBoxSOList;
    public List<ArtBoxSO> artBoxSOList;

    public string GetName()
    {
        switch (type)
        {
            case Helper.ConceptType.Hero:
                return "Hero";
            case Helper.ConceptType.Anime:
                return "Anime";
            case Helper.ConceptType.Kpop:
                return "Idol";
            case Helper.ConceptType.Game:
                return "Game";
            case Helper.ConceptType.Cute:
                return "Cute";
            case Helper.ConceptType.Trend:
                return "Trend";
            case Helper.ConceptType.Cartoon:
                return "Cartoon";
        }
        return "";
    }

}

