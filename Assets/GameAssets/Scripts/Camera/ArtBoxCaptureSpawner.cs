using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ArtBoxCaptureSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform parentForBoxes;

    public static ArtBoxCaptureSpawner Instance;
    public List<ArtBoxSO> cameraArtBoxList = new List<ArtBoxSO>();

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

    private void OnEnable()
    {
        MobileCameraPixelate.OnShotSpriteReady += HandleShotSpriteReady;
    }

    private void OnDisable()
    {
        MobileCameraPixelate.OnShotSpriteReady -= HandleShotSpriteReady;
    }

    public void LoadCameraArtBoxes()
    {
        ArtBoxSaveSystem artBoxSaveSystem = new ArtBoxSaveSystem();
        if (!artBoxSaveSystem.Exists())
        {
            Debug.Log("ArtBoxCaptureSpawner: No save file found to load camera pictures.");
            return;
        }

        ArtBoxSaveCollection loadedCollection = artBoxSaveSystem.Load();
        if (loadedCollection == null || loadedCollection.artBoxes == null) return;

        Debug.Log($"Found {loadedCollection.artBoxes.Count} entries in save data. Checking for camera captures.");

        string path = Application.persistentDataPath;

        foreach (var savedData in loadedCollection.artBoxes)
        {
            if (savedData.soName.StartsWith("artbox_"))
            {
                // artbox_20240101_123456 -> 20240101_123456
                string uid = savedData.soName.Substring("artbox_".Length);
                string originalFileName = $"pixel_processed_{uid}.png";
                string originalFilePath = Path.Combine(path, originalFileName);

                if (File.Exists(originalFilePath))
                {
                    byte[] originalBytes = File.ReadAllBytes(originalFilePath);
                    Texture2D originalTex = new Texture2D(2, 2);
                    originalTex.LoadImage(originalBytes);
                    originalTex.filterMode = FilterMode.Point;

                    Sprite originalSprite = Sprite.Create(
                        originalTex,
                        new Rect(0, 0, originalTex.width, originalTex.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                    originalSprite.name = savedData.soName;

                    var newSO = ScriptableObject.CreateInstance<ArtBoxSO>();
                    newSO.name = savedData.soName;
                    newSO.sprite = originalSprite;
                    newSO.ads = false;
                    newSO.isDone = savedData.isDone;
                    newSO.bgIndex = savedData.bgIndex;
                    newSO.frameIndex = savedData.frameIndex;
                    newSO.stickerDatas = new List<StickerData>(savedData.stickerDatas);

                    DatabaseManager.Instance.artBoxSODatabase.artBoxSOList.Add(newSO);
                    cameraArtBoxList.Add(newSO);
                }
                else
                {
                    Debug.LogWarning($"Found saved data for '{savedData.soName}' but couldn't find the corresponding image file: {originalFilePath}");
                }
            }
        }
    }

    private void HandleShotSpriteReady(Sprite shotSprite)
    {
        var newArtBoxSO = ScriptableObject.CreateInstance<ArtBoxSO>();
        newArtBoxSO.name = shotSprite.name;
        newArtBoxSO.sprite = shotSprite;
        newArtBoxSO.ads = false;
        newArtBoxSO.isDone = false;

        DatabaseManager.Instance.artBoxSODatabase.artBoxSOList.Add(newArtBoxSO);
        cameraArtBoxList.Add(newArtBoxSO);

        SpawnArtBoxFromPool(newArtBoxSO);
        Debug.Log($"New ArtBox from camera created and added to database: {newArtBoxSO.name}");
    }


    private void SpawnArtBoxFromPool(ArtBoxSO artBoxSO)
    {
        ArtBox artBox = ArtBoxPool.Instance.GetArtBoxFromContainer(parentForBoxes);

        if (artBox != null)
        {
            artBox.SetArtBox(artBoxSO);
        }
        else
        {
            Debug.LogError("ArtBoxCaptureSpawner: Could not get ArtBox from pool.");
        }
    }

    public void UpdateCameraArtBoxes()
    {
        ArtBoxPool.Instance.ReturnAllArtBoxesFromContainer(parentForBoxes);

        foreach (ArtBoxSO artBoxSO in cameraArtBoxList)
        {
            SpawnArtBoxFromPool(artBoxSO);
        }
    }
}