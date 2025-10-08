using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtBoxPool : MonoBehaviour
{
    public static ArtBoxPool Instance;
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

    [SerializeField] private ArtBox artBoxPrefab;
    public Transform[] artBoxContainer;
    public Transform myWorkContainer;
    public Transform suggestContainer;
    private Queue<ArtBox> artBoxPool = new Queue<ArtBox>();
    private List<ArtBox> activeArtBoxs = new List<ArtBox>();

    public ArtBox GetArtBoxFromContainer(int index)
    {
        if (artBoxPool.Count > 0)
        {
            ArtBox artBox = artBoxPool.Dequeue();
            artBox.transform.SetParent(artBoxContainer[index]);
            activeArtBoxs.Add(artBox);
            artBox.gameObject.SetActive(true);
            artBox.transform.SetAsLastSibling();
            return artBox;
        }
        else
        {
            ArtBox newArtBox = Instantiate(artBoxPrefab, artBoxContainer[index]);
            activeArtBoxs.Add(newArtBox);
            newArtBox.gameObject.SetActive(true);
            newArtBox.transform.SetAsLastSibling();
            return newArtBox;
        }
    }
    
    public ArtBox GetArtBoxFromContainer(Transform transform)
    {
        if (artBoxPool.Count > 0)
        {
            ArtBox artBox = artBoxPool.Dequeue();
            artBox.transform.SetParent(transform);
            activeArtBoxs.Add(artBox);
            artBox.gameObject.SetActive(true);
            artBox.transform.SetAsLastSibling();
            return artBox;
        }
        else
        {
            ArtBox newArtBox = Instantiate(artBoxPrefab, transform);
            activeArtBoxs.Add(newArtBox);
            newArtBox.gameObject.SetActive(true);
            newArtBox.transform.SetAsLastSibling();
            return newArtBox;
        }
    }

    public void ReturnArtBox(ArtBox artBox)
    {
        if (activeArtBoxs.Contains(artBox))
        {
            activeArtBoxs.Remove(artBox);
            artBoxPool.Enqueue(artBox);
            artBox.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ArtBox not found in active lines.");
        }
    }

    public void ReturnAllArtBoxes()
    {
        foreach (var artBox in activeArtBoxs)
        {
            artBox.gameObject.SetActive(false);
            artBoxPool.Enqueue(artBox);
        }
        activeArtBoxs.Clear();
    }

    public void ReturnAllArtBoxesFromContainer(int index)
    {
        List<ArtBox> activeArtBoxsFronContainer = GetActiveArtBoxsFronContainer(index);
        foreach (var artBox in activeArtBoxsFronContainer)
        {
            artBox.gameObject.SetActive(false);
            artBoxPool.Enqueue(artBox);
            activeArtBoxs.Remove(artBox);
        }  
    }

    public void ReturnAllArtBoxesFromContainer(Transform transform)
    {
        List<ArtBox> activeArtBoxsFronContainer = GetActiveArtBoxsFronContainer(transform);
        foreach (var artBox in activeArtBoxsFronContainer)
        {
            artBox.gameObject.SetActive(false);
            artBoxPool.Enqueue(artBox);
            activeArtBoxs.Remove(artBox);
        }  
    }

    public List<ArtBox> GetActiveArtBoxs()
    {
        return activeArtBoxs;
    }

    public List<ArtBox> GetArtBoxByArtBoxSO(ArtBoxSO artBoxSO)
    {
        if (artBoxSO == null) return null;
        List<ArtBox> artBoxes = new List<ArtBox>();
        foreach (ArtBox artBox in activeArtBoxs)
        {
            if (artBox.artBoxSO == artBoxSO)
            {
                artBoxes.Add(artBox);
            }
        }
        return artBoxes;
    }

    public List<ArtBox> GetActiveArtBoxsFronContainer(int index)
    {
        return GetActiveArtBoxsFronContainer(artBoxContainer[index]);
    }

    public List<ArtBox> GetActiveArtBoxsFronContainer(Transform transform)
    {
        List<ArtBox> res = new List<ArtBox>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                res.Add(child.GetComponent<ArtBox>());
            }
        }
        return res;
    }

    public ArtBox GetArtBoxByNumber(int number)
    {
        if (number > 0 && number <= activeArtBoxs.Count)
            return activeArtBoxs[number - 1];
        return null;
    }
}
