// PaletteButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PaletteButton : MonoBehaviour
{
    public int colorID;

    [SerializeField] private Image buttonImage; // Kéo component Image của nút vào đây
    [SerializeField] private TextMeshProUGUI numberText; // Kéo component Text của nút vào đây

    // Phương thức này sẽ được GameManager gọi để thiết lập nút
    public void Initialize(int id, Color displayColor)
    {
        colorID = id;

        Color buttonColor = displayColor;
        buttonColor.a = 1f;
        buttonImage.color = buttonColor;

        numberText.text = id.ToString();

        // Thêm sự kiện onClick cho nút
        GetComponent<Button>().onClick.AddListener(() => {
            GameManager.instance.SelectColor(colorID);
        });
    }
}