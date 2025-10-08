using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventArtDataSO", menuName = "ScriptableObject/Data/EventArtDataSO")]
public class EventArtDataSO : ScriptableObject
{
    public bool isPurchased;
    public int adsWatched;
}
