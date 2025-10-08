using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorBoxPool : MonoBehaviour
{
    public static DecorBoxPool Instance;
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

    [SerializeField] private DecorBox decorBoxPrefab;
    [SerializeField] private Transform decorBoxContainer;
    private Queue<DecorBox> decorBoxPool = new Queue<DecorBox>();
    private List<DecorBox> activeDecorBoxs = new List<DecorBox>();

    public DecorBox GetDecorBox()
    {
        if (decorBoxPool.Count > 0)
        {
            DecorBox decorBox = decorBoxPool.Dequeue();
            activeDecorBoxs.Add(decorBox);
            decorBox.gameObject.SetActive(true);
            decorBox.transform.SetAsLastSibling();
            return decorBox;
        }
        else
        {
            DecorBox newDecorBox = Instantiate(decorBoxPrefab, decorBoxContainer);
            activeDecorBoxs.Add(newDecorBox);
            newDecorBox.gameObject.SetActive(true);
            newDecorBox.transform.SetAsLastSibling();
            return newDecorBox;
        }
    }

    public void ReturnDecorBox(DecorBox decorBox)
    {
        if (activeDecorBoxs.Contains(decorBox))
        {
            activeDecorBoxs.Remove(decorBox);
            decorBoxPool.Enqueue(decorBox);
            decorBox.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DecorBox not found in active lines.");
        }
    }

    public void ReturnAllDecorBoxes()
    {
        foreach (var decorBox in activeDecorBoxs)
        {
            decorBox.gameObject.SetActive(false);
            decorBoxPool.Enqueue(decorBox);
        }
        activeDecorBoxs.Clear();
    }

    public List<DecorBox> GetActiveDecorBoxs()
    {
        return activeDecorBoxs;
    }

    public DecorBox GetDecorBoxByNumber(int number)
    {
        if (number > 0 && number <= activeDecorBoxs.Count)
            return activeDecorBoxs[number - 1];
        return null;
    }
}
