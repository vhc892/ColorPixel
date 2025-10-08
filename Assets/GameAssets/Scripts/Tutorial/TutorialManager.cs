using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private List<TutorialStep> tutorialSteps;
    [SerializeField] private TextMeshProUGUI tutorialTextUI;
    [SerializeField] private RealTimeTutUI realtimeTutorialUI;
    [SerializeField] private Button[] boosterButtons;

    public Button backButton;
    public Button nextButton;

    private int currentStepIndex = 0;
    private List<string> tutorialMessages;

    //User Interact Step
    private int UserInteractStep = 0;
    private Vector2Int pixelPos = -Vector2Int.one;

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
        InitTutorialMessages();
    }
    void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextButton);
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackButton);
        }
    }
    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(NextButton);
        backButton.onClick.RemoveListener(BackButton);

    }
    public void BeginTutorial()
    {
        currentStepIndex = 0;
        foreach (var step in tutorialSteps)
        {
            step.EndStep();
        }
        ShowStep(currentStepIndex);
    }
    private void InitTutorialMessages()
    {
        string itemBoomColor = "#9D3CFF";
        string itemPaintBrushColor = "#FFA500";
        string itemPaintCursorColor = "#EF62A7";

        tutorialMessages = new List<string>
        {
            $"<color={itemPaintBrushColor}>Press and hold</color> your finger <color={itemPaintCursorColor}>in Paint Area</color> to highlight the selected color",
            $"<color={itemPaintBrushColor}>Use two finger</color> to <color={itemPaintCursorColor}>zoom in or out</color> the pictute until a grid with number appear",
            $"<color={itemBoomColor}>Item Boom</color> colors the surrounding area of the selected cell",
            $"<color={itemPaintBrushColor}>Item Paint Brush</color> fills all the same color cells"
        };
    }
    private void ShowStep(int index)
    {
        if (index < 0 || index >= tutorialSteps.Count)
        {
            return;
        }
        if (tutorialTextUI != null && index < tutorialMessages.Count)
        {
            tutorialTextUI.text = tutorialMessages[index];
        }
        else if (tutorialTextUI != null)
        {
            tutorialTextUI.text = "";
        }
        tutorialSteps[index].StartStep();
        UpdateButton();
    }

    public void NextButton()
    {
        AudioManager.Instance.PressButtonSfx();
        tutorialSteps[currentStepIndex].EndStep();
        currentStepIndex++;

        if (currentStepIndex >= tutorialSteps.Count)
        {
            Debug.Log("Tutorial Finished!");
            UIManager.Instance.HideTutorialPopup();
        }
        else
        {
            ShowStep(currentStepIndex);
        }
    }
    public void BackButton()
    {
        if (currentStepIndex <= 0) return;

        tutorialSteps[currentStepIndex].EndStep();
        currentStepIndex--;
        ShowStep(currentStepIndex);
    }
    private void UpdateButton()
    {
        if (backButton != null)
        {
            backButton.gameObject.SetActive(currentStepIndex > 0);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(currentStepIndex < tutorialSteps.Count - 1);
        }
    }

    public void ShowRealtimeTutorial()
    {
        if (realtimeTutorialUI.gameObject.activeInHierarchy) return;
        SetInteractableBoosterButtons(true);
        realtimeTutorialUI.gameObject.SetActive(true);
        realtimeTutorialUI.fillButtonTut.SetActive(true);
        realtimeTutorialUI.bombButtonTut.SetActive(false);
        realtimeTutorialUI.findButtonTut.SetActive(false);
        realtimeTutorialUI.interactArea.SetActive(false);
        UserInteractStep = 0;
        realtimeTutorialUI.GetComponentInChildren<TextMeshProUGUI>().DOFade(1, 0.5f).From(0);
        realtimeTutorialUI.GetComponentInChildren<TextMeshProUGUI>()
        .SetText($"<color={"#FFA500"}>Item Paint Brush</color> fills all the same color cells");
        SetRealTimeTutHandTouchPos(realtimeTutorialUI.fillButtonTut.transform.position);
        RealTimeTutHandTouchAnim();
        InputHandler.Instance.SetUpCamera(true);
    }

    public void NextRealtimeTutorial()
    {
        if (CoreGameManager.Instance.IsWin()) HideRealtimeTutorial();
        Vector3 firstTouch = realtimeTutorialUI.cam.ScreenToWorldPoint(
        new Vector3(Input.mousePosition.x, Input.mousePosition.y, realtimeTutorialUI.cam.nearClipPlane));

        if (CoreGameManager.Instance.CanUseBoosterAtPosition(firstTouch))
        {
            TextMeshProUGUI text = realtimeTutorialUI.textTut;
            UserInteractStep++;
            realtimeTutorialUI.fillButtonTut.SetActive(UserInteractStep == 0);
            realtimeTutorialUI.bombButtonTut.SetActive(UserInteractStep == 1);
            realtimeTutorialUI.findButtonTut.SetActive(UserInteractStep == 2);
            realtimeTutorialUI.interactArea.SetActive(false);

            switch (UserInteractStep)
            {
                case 1:
                    FillManager.Instance.StartFill(firstTouch);
                    text.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        text.SetText($"<color={"#9D3CFF"}>Item Boom</color> colors the surrounding area of the selected cell");
                        SetRealTimeTutHandTouchPos(realtimeTutorialUI.bombButtonTut.transform.position);
                        RealTimeTutHandTouchAnim();
                        text.DOFade(1, 0.3f);
                    });

                    break;
                case 2:
                    BoomManager.Instance.StartBoom(firstTouch);
                    text.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        text.SetText($"<color={"#EF62A7"}>Item Hint</color> helps you find a valid cell");
                        SetRealTimeTutHandTouchPos(realtimeTutorialUI.findButtonTut.transform.position);
                        RealTimeTutHandTouchAnim();
                        text.DOFade(1, 0.3f);
                    });
                    break;
                default:
                    break;
            }
        }
    }

    public void OnFillButtonTut()
    {
        if (PaintingFX.Instance.fireworkParticle.IsAlive(true)) return;
        realtimeTutorialUI.fillButtonTut.SetActive(false);
        ActiveRealTimeTutHandAnim();
        PlayerManager.Instance.OnFillBooster();

        realtimeTutorialUI.interactArea.SetActive(true);
    }

    public void OnBombButtonTut()
    {
        if (PaintingFX.Instance.fireworkParticle.IsAlive(true)) return;
        realtimeTutorialUI.bombButtonTut.SetActive(false);
        ActiveRealTimeTutHandAnim();
        PlayerManager.Instance.OnBoomBooster();
        realtimeTutorialUI.interactArea.SetActive(true);
    }

    public void OnFindButtonTut()
    {
        if (PaintingFX.Instance.fireworkParticle.IsAlive(true)) return;
        PlayerManager.Instance.OnFindBooster();
        realtimeTutorialUI.gameObject.SetActive(false);
        RemoveCanvasGroupFromBoosterButtons();
    }

    private void ActiveRealTimeTutHandAnim()
    {
        pixelPos = CoreGameManager.Instance.FindRandomPixelPosition();
        Vector3 worldPos = CoreGameManager.Instance.PixelToWorld(pixelPos);
        if (pixelPos != -Vector2Int.one)
        {
            Vector2 uiPos = Helper.ScriptTool.WorldToUIPosition(worldPos, realtimeTutorialUI.canvas, realtimeTutorialUI.cam);
            realtimeTutorialUI.interactArea.SetActive(true);

            // DÙNG anchoredPosition cho UI
            // RectTransform interactRect = realtimeTutorialUI.interactArea.GetComponent<RectTransform>();
            // interactRect.anchoredPosition = uiPos;

            realtimeTutorialUI.handTut.SetActive(true);

            realtimeTutorialUI.handTut.GetComponent<RectTransform>().anchoredPosition = uiPos;
            Vector3 offset = new Vector3(45, -45, 0);
            realtimeTutorialUI.handTut.transform.position += offset;
            RealTimeTutHandTouchAnim();
        }
    }

    private void RealTimeTutHandTouchAnim()
    {
        realtimeTutorialUI.handTut.transform.DOKill();
        realtimeTutorialUI.handTut.transform.DOScale(0.8f, 0.5f).From(1).SetLoops(-1, LoopType.Yoyo);
    }

    private void SetRealTimeTutHandTouchPos(Vector3 pos)
    {
        Vector3 offset = new Vector3(45, -45, 0);
        pos += offset;
        realtimeTutorialUI.handTut.transform.position = pos;
    }


    public void SetInteractableBoosterButtons(bool interactable)
    {
        foreach (var button in boosterButtons)
        {
            if (!button.TryGetComponent(out CanvasGroup cg))
                cg = button.gameObject.AddComponent<CanvasGroup>();
            cg.interactable = interactable;
            cg.alpha = interactable ? 1 : 0.5f;
        }
    }
    
    public void RemoveCanvasGroupFromBoosterButtons()
    {
        foreach (var button in boosterButtons)
        {
            var cg = button.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = true;
                cg.alpha = 1;
                Destroy(cg);
            }
        }
    }

    
}