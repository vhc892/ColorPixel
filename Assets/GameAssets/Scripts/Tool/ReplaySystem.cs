using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum ReplayActionType
{
    Paint,
    Fill,
    Boom
}

[System.Serializable]
public struct ReplayStep
{
    public ReplayActionType actionType;
    public int pixelX;
    public int pixelY;
    public int num;
}

[System.Serializable]
public class ReplayData
{
    public List<ReplayStep> steps = new List<ReplayStep>();
}

public class ReplaySystem : MonoBehaviour
{
    public static ReplaySystem Instance;

    [SerializeField] private int paintStepsPerFrame = 5;

    private ReplayData currentRecording;
    private bool isRecording = false;
    private string currentArtName;
    public static bool isReplaying = false;

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

    public void StartRecording(string artName)
    {
        currentArtName = artName;
        string fileName = currentArtName + "_replay.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentRecording = JsonUtility.FromJson<ReplayData>(json);
            if (currentRecording == null)
            {
                currentRecording = new ReplayData();
            }
            Debug.Log("Resuming replay recording for: " + artName);
        }
        else
        {
            currentRecording = new ReplayData();
            Debug.Log("Starting new replay recording for: " + artName);
        }

        isRecording = true;
    }

    public void DeleteRecording(string artName)
    {
        string fileName = artName + "_replay.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted old replay file for: " + artName);
        }
    }
    public void RecordAction(ReplayActionType type, int x, int y, int colorNumber = 0)
    {
        if (!isRecording) return;

        ReplayStep step = new ReplayStep
        {
            actionType = type,
            pixelX = x,
            pixelY = y,
            num = colorNumber
        };

        currentRecording.steps.Add(step);
    }

    public void StopAndSaveRecording()
    {
        if (!isRecording) return;
        isRecording = false;

        string json = JsonUtility.ToJson(currentRecording, false);
        string fileName = currentArtName + "_replay.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);

        Debug.Log("Replay saved to: " + path);
    }

    public void PlayReplay(string artName)
    {
        isReplaying = true;
        string fileName = artName + "_replay.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("Replay file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ReplayData replayData = JsonUtility.FromJson<ReplayData>(json);

        if (replayData != null && replayData.steps.Count > 0)
        {
            StartCoroutine(PlaybackCoroutine(replayData));
        }
        else
        {
            Debug.LogError("Failed to load or replay data is empty.");
        }
    }

    private IEnumerator PlaybackCoroutine(ReplayData replayData)
    {
        CoreGameManager.Instance.ResetForReplay();
        SpriteRenderer graySpriteRenderer = CoreGameManager.Instance.GetSpriteRendererGrayScale();
        graySpriteRenderer.sprite.texture.Apply();
        yield return new WaitForSeconds(0.5f);

        int currentStepIndex = 0;
        while (currentStepIndex < replayData.steps.Count)
        {
            var currentStep = replayData.steps[currentStepIndex];
            if (graySpriteRenderer.sprite == null)
            {
                isReplaying = false;
                yield break;
            }
            switch (currentStep.actionType)
            {
                case ReplayActionType.Paint:
                    for (int i = 0; i < paintStepsPerFrame; i++)
                    {
                        if (currentStepIndex < replayData.steps.Count && replayData.steps[currentStepIndex].actionType == ReplayActionType.Paint)
                        {
                            var paintStep = replayData.steps[currentStepIndex];
                            CoreGameManager.Instance.PaintForReplay(paintStep.pixelX, paintStep.pixelY, paintStep.num);
                            currentStepIndex++; 
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (graySpriteRenderer.sprite != null)
                        graySpriteRenderer.sprite.texture.Apply();
                    break;

                case ReplayActionType.Fill:
                    yield return FillManager.Instance.StartFillForReplay(currentStep.pixelX, currentStep.pixelY);
                    currentStepIndex++;
                    break;

                case ReplayActionType.Boom:
                    yield return BoomManager.Instance.StartBoomForReplay(currentStep.pixelX, currentStep.pixelY);
                    currentStepIndex++;
                    break;
            }

            yield return null;
        }
        Debug.Log("Replay finished.");
        isReplaying = false;
    }

}