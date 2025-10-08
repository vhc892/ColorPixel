using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WheelRewardSO", menuName = "ScriptableObject/Data/WheelRewardSO")]
public class WheelRewardSO : ScriptableObject
{
    public Helper.RewardType rewardType;
    public int amount;
}
