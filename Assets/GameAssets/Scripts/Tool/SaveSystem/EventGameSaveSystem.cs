using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGameSaveSystem : BaseSaveSystem<EventGameList>
{
    protected override string FileName => "event_game.json";
}

[System.Serializable]
public class EventGameList
{
    public EventGameData[] list;
}

[System.Serializable]
public class EventGameData
{
    public EventArtData[] list;
}

[System.Serializable]
public class EventArtData
{
    public bool isPurchased;
    public int adsWatched;
}

