using System;
using UnityEngine;

[Serializable]

public class PlayerSaveSystem : BaseSaveSystem<PlayerData>
{
    protected override string FileName => "player_data.json";
}
public class PlayerData
{
    public int coin;
    public int fillBoosterNumber;
    public int boomBoosterNumber;
    public int findBoosterNumber;
    public int spinAmount;
    public float coinProgress;

    public bool sound;
    public bool music;
    public bool vibration;
}
