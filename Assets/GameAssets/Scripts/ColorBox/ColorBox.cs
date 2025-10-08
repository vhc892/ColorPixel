using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorBox : MonoBehaviour, IPointerClickHandler
{
    public int number;
    public int currentPaintedPixel;
    public int targetPixel;
    private TextMeshProUGUI numberUI;
    public Image colorImage;
    public Image radialProgress;
    public Image checkmarkImage;
    private bool isCompleted = false;
    private ScrollToSelected scroller;


    void Awake()
    {
        numberUI = GetComponentInChildren<TextMeshProUGUI>();
        if (numberUI == null) Debug.LogError("numberUI not found in ColorBox's children");
        colorImage = GetComponent<Image>();
        if (colorImage == null) Debug.LogError("image not found in ColorBox");
        scroller = GetComponentInParent<ScrollToSelected>();
        if (checkmarkImage != null)
        {
            checkmarkImage.gameObject.SetActive(false);
        }
    }

    public void SetData(int num, Color32 color32)
    {
        isCompleted = false;
        transform.localScale = Vector3.one; // Đảm bảo kích thước đúng

        SetColor(color32);
        SetNumber(num);
        numberUI.color = GetContrastColor(color32);
        //radialProgress.color = GetContrastColor(color32);
        radialProgress.fillAmount = 0;
        currentPaintedPixel = 0;

        numberUI.gameObject.SetActive(true);
        if (checkmarkImage != null)
        {
            checkmarkImage.gameObject.SetActive(false);
        }
    }

    private void SetNumber(int num)
    {
        number = num;
        UpdateNumberUI();
    }

    private void SetColor(Color32 color32)
    {
        colorImage.color = color32;
    }

    public void SetTargetPixel(int value)
    {
        targetPixel = value;
        isCompleted = (targetPixel > 0 && currentPaintedPixel >= targetPixel);
        UpdatePaintedProgress();
    }

    public void SetCurrentPaintedPixel(int value)
    {
        currentPaintedPixel = value;
        isCompleted = (targetPixel > 0 && currentPaintedPixel >= targetPixel);
        UpdatePaintedProgress();
    }


    public void UpdateCurrentPixel(int px, int py)
    {
        currentPaintedPixel++;
        UpdatePaintedProgress(px, py);
    }

    public bool CanInteract()
    {
        return currentPaintedPixel < targetPixel;
    }

    private void UpdateNumberUI()
    {
        numberUI.SetText(number + "");
    }
    

    private void UpdatePaintedProgress() {
        if (targetPixel > 0)
        {
            radialProgress.fillAmount = (float)currentPaintedPixel / targetPixel;
        }

        if (gameObject.activeInHierarchy && !isCompleted && targetPixel > 0 && currentPaintedPixel >= targetPixel)
        {
            isCompleted = true;
            Debug.Log("firework");
            AudioManager.Instance.FireWorkLaunchSfx();
            StartCoroutine(CompletionSequence());
        }
    }

    private void UpdatePaintedProgress(int px, int py) {
        if (targetPixel > 0)
        {
            radialProgress.fillAmount = (float)currentPaintedPixel / targetPixel;
        }

        if (gameObject.activeInHierarchy && !isCompleted && targetPixel > 0 && currentPaintedPixel >= targetPixel)
        {
            isCompleted = true;
            PaintingFX.Instance.PlayColorFinishEffect(new Vector2Int(px, py));
            Debug.Log("firework");
            AudioManager.Instance.FireWorkLaunchSfx();
            StartCoroutine(CompletionSequence());
        }
    }

    public bool IsCompleted()
    {
        return !isCompleted && targetPixel > 0 && currentPaintedPixel >= targetPixel;
    }
    private void OnColorBoxCompleted()
    {
        numberUI.gameObject.SetActive(false);
        checkmarkImage.gameObject.SetActive(true);

        if (checkmarkImage != null)
        {
            checkmarkImage.transform.localScale = Vector3.zero;
            checkmarkImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
    }
    private IEnumerator CompletionSequence()
    {
        Color originalColor = colorImage.color;

        // bright color
        Color.RGBToHSV(originalColor, out float H, out float S, out float V);
        if (V < 0.2f) { V = 0.2f; }
        else { V = Mathf.Clamp01(V * 1.2f); }
        S = Mathf.Clamp01(S * 0.7f);
        Color brighterColor = Color.HSVToRGB(H, S, V);

        float particleDuration = PaintingFX.Instance.GetFireworkDuration();
        PaintingFX.Instance.PlayFirework(brighterColor);
        OnColorBoxCompleted();
        CoreGameManager.Instance.CheckWinCondition();
        yield return new WaitForSeconds(particleDuration);
        CoreGameManager.Instance.SelectNextColorBox(this);
        QuestManager.Instance.UpdateQuestProgress(Helper.QuestType.Daily_Finish_Color);
    }
    public void SetCompletedState()
    {
        isCompleted = true;
        numberUI.gameObject.SetActive(false);
        checkmarkImage.gameObject.SetActive(true);
        radialProgress.fillAmount = 1;
    }
    private Color GetContrastColor(Color color)
    {
        // Tính độ sáng theo công thức ITU-R BT.709
        float luminance = (0.2126f * color.r +
                           0.7152f * color.g +
                           0.0722f * color.b);

        // Nếu sáng nhiều → trả về đen, ngược lại → trắng
        return luminance > 0.5f ? Color.black : Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanInteract())
        {
            AudioManager.Instance.PressButtonSfx();
            CoreGameManager.Instance.SelectColorBox(this);
            if (scroller != null)
            {
                scroller.CenterOnItem(transform as RectTransform);
            }
        }
    }
    public void Select()
    {
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Deselect()
    {
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
    }
}
