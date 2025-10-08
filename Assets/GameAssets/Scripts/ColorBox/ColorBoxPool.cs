using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBoxPool : MonoBehaviour
{
    public static ColorBoxPool Instance;
    private void Awake()
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

    [SerializeField] private ColorBox colorBoxPrefab;
    [SerializeField] private Transform colorBoxContainer;
    private Queue<ColorBox> colorBoxPool = new Queue<ColorBox>();
    private List<ColorBox> activeColorBoxs = new List<ColorBox>();

    public ColorBox GetColorBox()
    {
        if (colorBoxPool.Count > 0)
        {
            ColorBox colorBox = colorBoxPool.Dequeue();
            activeColorBoxs.Add(colorBox);
            colorBox.gameObject.SetActive(true);
            colorBox.transform.SetAsLastSibling();
            return colorBox;
        }
        else
        {
            ColorBox newColorBox = Instantiate(colorBoxPrefab, colorBoxContainer);
            activeColorBoxs.Add(newColorBox);
            newColorBox.gameObject.SetActive(true);
            newColorBox.transform.SetAsLastSibling();
            return newColorBox;
        }
    }

    public void ReturnColorBox(ColorBox colorBox)
    {
        if (activeColorBoxs.Contains(colorBox))
        {
            activeColorBoxs.Remove(colorBox);
            colorBoxPool.Enqueue(colorBox);
            colorBox.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ColorBox not found in active lines.");
        }
    }

    public void ReturnAllColorBoxes()
    {
        foreach (var colorBox in activeColorBoxs)
        {
            colorBox.gameObject.SetActive(false);
            colorBoxPool.Enqueue(colorBox);
        }
        activeColorBoxs.Clear();
    }

    public List<ColorBox> GetActiveColorBoxs()
    {
        return activeColorBoxs;
    }

    public ColorBox GetColorBoxByNumber(int number)
    {
        if (number > 0 && number <= activeColorBoxs.Count)
            return activeColorBoxs[number - 1];
        return null;
    }
}
