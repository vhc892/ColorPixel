using System.Collections;
using System.Collections.Generic;
using Helper;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;

    void Awake()
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
    private void Start()
    {
        ArtBoxCaptureSpawner.Instance.LoadCameraArtBoxes();
        RemoveNullSO();
        LoadArtBoxData();
        LoadMyWorkData();
        LoadQuests();
        LoadPlayerData();
        LoadEventData();
    }

    void OnApplicationQuit()
    {
        if (!ReplaySystem.isReplaying)
        {
            var sr = CoreGameManager.Instance.GetSpriteRendererGrayScale();
            if (sr != null && sr.sprite != null)
                SaveLoadImage.SaveSpriteProgress(sr.sprite);
        }
        
        ReplaySystem.Instance.StopAndSaveRecording();
        SaveArtBoxData();
        SaveMyWorkData();
        SaveQuests();
        SavePlayerData();
        SaveEventData();

        DecorManager.Instance.SaveStickerCapture();
        Debug.Log("Auto-saved on quit!");
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            if (!ReplaySystem.isReplaying)
            {
                var sr = CoreGameManager.Instance.GetSpriteRendererGrayScale();
                if (sr != null && sr.sprite != null)
                    SaveLoadImage.SaveSpriteProgress(sr.sprite);
            }
            ReplaySystem.Instance.StopAndSaveRecording();
            Debug.Log("Auto-saved on pause!");
            SaveArtBoxData();
            SaveMyWorkData();
            SaveQuests();
            SavePlayerData();
            SaveEventData();

            DecorManager.Instance.SaveStickerCapture();
            Debug.Log("Auto-saved on pause!");
        }
        else
        {
            if (!ReplaySystem.isReplaying && CoreGameManager.Instance != null)
            {
                ArtBoxSO currentArt = CoreGameManager.Instance.GetCurrentArtBoxSO();
                if (currentArt != null)
                {
                    ReplaySystem.Instance.StartRecording(currentArt.sprite.name);
                    Debug.Log("Resumed recording on application focus.");
                }
            }
        }
    }
    private void RemoveNullSO()
    {
        var artBoxList = DatabaseManager.Instance.artBoxSODatabase.artBoxSOList;
        if (artBoxList != null)
        {
            int countBefore = artBoxList.Count;
            artBoxList.RemoveAll(item => item == null);
            int countAfter = artBoxList.Count;

            if (countBefore > countAfter)
            {
                Debug.Log($"Remove {countBefore - countAfter} null so in artBoxSODatabase.");
            }
        }
    }

    public void SaveArtBoxData()
    {
        ArtBoxSaveCollection myArtBoxCollection = new ArtBoxSaveCollection();

        foreach (ArtBoxSO artBoxSO in DatabaseManager.Instance.artBoxSODatabase.artBoxSOList)
        {
            ArtBoxSaveData saveData = new ArtBoxSaveData
            {
                soName = artBoxSO.name,
                isDone = artBoxSO.isDone,
                bgIndex = artBoxSO.bgIndex,
                frameIndex = artBoxSO.frameIndex,
                stickerDatas = new List<StickerData>(artBoxSO.stickerDatas),
                isBlink = artBoxSO.isBlink,
                canBlink = artBoxSO.canBlink,
                isGlitter = artBoxSO.isGlitter,
            };
            myArtBoxCollection.artBoxes.Add(saveData);
        }

        ArtBoxSaveSystem artBoxSaveSystem = new ArtBoxSaveSystem();
        artBoxSaveSystem.Save(myArtBoxCollection);

        Debug.Log("ArtBox data collected and saved.");
    }

    public void LoadArtBoxData()
    {
        ArtBoxSaveSystem artBoxSaveSystem = new ArtBoxSaveSystem();

        if (!artBoxSaveSystem.Exists())
        {
            Debug.LogWarning("No ArtBox save file found.");
            return;
        }

        ArtBoxSaveCollection loadedCollection = artBoxSaveSystem.Load();
        if (loadedCollection == null || loadedCollection.artBoxes == null)
        {
            Debug.LogWarning("Loaded ArtBox data is null or empty.");
            return;
        }

        var existingSOList = DatabaseManager.Instance.artBoxSODatabase.artBoxSOList;
        var existingSODict = new Dictionary<string, ArtBoxSO>();
        foreach (var so in existingSOList)
        {
            if (so != null && !existingSODict.ContainsKey(so.name))
            {
                existingSODict.Add(so.name, so);
            }
        }
        // Cập nhật dữ liệu cho các SO đã tồn tại
        foreach (var savedData in loadedCollection.artBoxes)
        {
            if (existingSODict.TryGetValue(savedData.soName, out ArtBoxSO artBox))
            {
                artBox.isDone = savedData.isDone;
                artBox.bgIndex = savedData.bgIndex;
                artBox.frameIndex = savedData.frameIndex;

                artBox.stickerDatas.Clear();
                artBox.stickerDatas.AddRange(savedData.stickerDatas);
                artBox.isBlink = savedData.isBlink;
                artBox.canBlink = savedData.canBlink;
                artBox.isGlitter = savedData.isGlitter;
            }
        }
    }

    public void SaveMyWorkData()
    {
        if (DatabaseManager.Instance.myWork.Count <= 0) return;
        MyWorkList myWorkList = new MyWorkList();

        myWorkList.list = DatabaseManager.Instance.myWork;

        MyWorksSaveSystem myWorksSaveSystem = new MyWorksSaveSystem();
        myWorksSaveSystem.Save(myWorkList);
    }

    public void LoadMyWorkData()
    {
        MyWorksSaveSystem myWorksSaveSystem = new MyWorksSaveSystem();

        if (!myWorksSaveSystem.Exists())
        {
            Debug.LogWarning("No MyWorkList save file found.");
            return;
        }

        MyWorkList myWorkList = myWorksSaveSystem.Load();
        if (myWorkList == null || myWorkList.list.Count <= 0)
        {
            Debug.LogWarning("Loaded MyWorkList Data is null or empty.");
            return;
        }
        DatabaseManager.Instance.myWork.Clear();
        DatabaseManager.Instance.myWork.AddRange(myWorkList.list);
    }

    public void SaveQuests()
    {
        QuestSaveSystem questSaveSystem = new QuestSaveSystem();

        QuestDataList dataList = new QuestDataList();

        foreach (var kvp in QuestManager.Instance.GetQuestDict())
        {
            QuestData data = new QuestData
            {
                questType = kvp.Key,
                currentProgress = kvp.Value.currentProgress,
                currentMilestone = kvp.Value.GetRealCurrentMileStone(),
                canShowNoti = kvp.Value.canShowNoti,
            };
            dataList.list.Add(data);
        }

        foreach (QuestSO questSO in QuestManager.Instance.CurrentDailyQuest)
        {
            dataList.dailyQuest.Add(questSO.type);
        }

        questSaveSystem.Save(dataList);
    }

    // Load
    public void LoadQuests()
    {
        QuestSaveSystem questSaveSystem = new QuestSaveSystem();
        QuestDataList dataList = questSaveSystem.Load();

        if (dataList == null)
        {
            QuestManager.Instance.ResetDailyQuests();
            return;
        }

        Dictionary<QuestType, QuestSO> questDict = QuestManager.Instance.GetQuestDict();
        foreach (var data in dataList.list)
        {
            if (questDict.ContainsKey(data.questType))
            {
                questDict[data.questType].currentProgress = data.currentProgress;
                questDict[data.questType].SetCurrentMileStone(data.currentMilestone);
                questDict[data.questType].canShowNoti = data.canShowNoti;
            }
        }

        // Load daily quest
        if (Daily.IsNewDay())
        {
            QuestManager.Instance.ResetDailyQuests();
        }
        else
        {
            if (dataList.dailyQuest.Count == 0)
            {
                QuestManager.Instance.ResetDailyQuests();
            }
            else
            {
                List<QuestSO> dailyQuestList = new List<QuestSO>();
                foreach (QuestType questType in dataList.dailyQuest)
                {
                    dailyQuestList.Add(questDict[questType]);
                }
                QuestManager.Instance.CurrentDailyQuest = dailyQuestList.ToArray();
            }
        }
        UIManager.Instance.taskbarController.SetActiveQuestNoti(QuestManager.Instance.HaveQuestCanClaim());
    }

    public void SavePlayerData()
    {
        PlayerData playerData = new PlayerData
        {
            coin = PlayerManager.Instance.coin,
            fillBoosterNumber = PlayerManager.Instance.fillBoosterNumber,
            boomBoosterNumber = PlayerManager.Instance.boomBoosterNumber,
            findBoosterNumber = PlayerManager.Instance.findBoosterNumber,
            spinAmount = PlayerManager.Instance.spinAmount,
            coinProgress = PlayerManager.Instance.coinProgressBar.CurrentProgress,

            sound = SettingManager.Instance.sound,
            music = SettingManager.Instance.music,
            vibration = SettingManager.Instance.vibration,
        };

        PlayerSaveSystem playerSaveSystem = new PlayerSaveSystem();
        playerSaveSystem.Save(playerData);
        Debug.Log("Player data saved.");
    }

    public void LoadPlayerData()
    {
        PlayerSaveSystem playerSaveSystem = new PlayerSaveSystem();

        if (!playerSaveSystem.Exists())
        {
            Debug.LogWarning("No PlayerData save file found.");
            return;
        }

        PlayerData loadedData = playerSaveSystem.Load();
        if (loadedData == null)
        {
            Debug.LogWarning("Loaded PlayerData is null.");
            return;
        }

        PlayerManager.Instance.coin = loadedData.coin;
        PlayerManager.Instance.fillBoosterNumber = loadedData.fillBoosterNumber;
        PlayerManager.Instance.boomBoosterNumber = loadedData.boomBoosterNumber;
        PlayerManager.Instance.findBoosterNumber = loadedData.findBoosterNumber;
        PlayerManager.Instance.spinAmount = loadedData.spinAmount;
        PlayerManager.Instance.coinProgressBar.SetProgress(loadedData.coinProgress);

        SettingManager.Instance.sound = loadedData.sound;
        SettingManager.Instance.music = loadedData.music;
        SettingManager.Instance.vibration = loadedData.vibration;

        PlayerManager.Instance.UpdateCoinUI();
        PlayerManager.Instance.UpdateBoosterUI();

        AudioManager.Instance.SwitchMusicVolume(SettingManager.Instance.music);
        AudioManager.Instance.SwitchSfxVolume(SettingManager.Instance.sound);

        Debug.Log("Player data loaded.");
    }

    public void SaveEventData()
    {
        EventGameList eventGameList = new EventGameList();
        EventGame[] events = DatabaseManager.Instance.eventDatabase;

        eventGameList.list = new EventGameData[events.Length];

        for (int i = 0; i < events.Length; i++)
        {
            EventGameData gameData = new EventGameData();

            // Giả sử mỗi EventGame có List<EventArt> artList
            EventArt[] arts = events[i].eventArts;
            gameData.list = new EventArtData[arts.Length];

            for (int j = 0; j < arts.Length; j++)
            {
                EventArtData artData = new EventArtData();
                artData.isPurchased = arts[j].eventArtDataSO.isPurchased;
                artData.adsWatched = arts[j].eventArtDataSO.adsWatched;

                gameData.list[j] = artData;
            }

            eventGameList.list[i] = gameData;
        }

        EventGameSaveSystem eventGameSaveSystem = new EventGameSaveSystem();
        eventGameSaveSystem.Save(eventGameList);
        Debug.Log("Event data saved.");
    }

    public void LoadEventData()
    {
        EventGameSaveSystem eventGameSaveSystem = new EventGameSaveSystem();

        if (!eventGameSaveSystem.Exists())
        {
            Debug.LogWarning("No Event data save file found.");
            return;
        }

        EventGameList loadedData = eventGameSaveSystem.Load();
        if (loadedData == null)
        {
            Debug.LogWarning("Loaded Event data is null.");
            return;
        }

        EventGame[] events = DatabaseManager.Instance.eventDatabase;

        // Map ngược dữ liệu từ file -> database
        for (int i = 0; i < loadedData.list.Length && i < events.Length; i++)
        {
            EventGameData gameData = loadedData.list[i];
            EventArt[] arts = events[i].eventArts;

            for (int j = 0; j < gameData.list.Length && j < arts.Length; j++)
            {
                EventArtData artData = gameData.list[j];
                arts[j].eventArtDataSO.isPurchased = artData.isPurchased;
                arts[j].eventArtDataSO.adsWatched = artData.adsWatched;
            }
        }
        Debug.Log("Event data loaded.");
    }

}
