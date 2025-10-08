using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerPool : MonoBehaviour
{
    public static StickerPool Instance;
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

    [SerializeField] private Sticker stickerPrefab;
    [SerializeField] private Transform stickerContainer;
    private Queue<Sticker> stickerPool = new Queue<Sticker>();
    private List<Sticker> activeSticker = new List<Sticker>();

    public Sticker GetSticker()
    {
        Sticker sticker;
        if (stickerPool.Count > 0)
        {
            sticker = stickerPool.Dequeue();
        }
        else
        {
            sticker = Instantiate(stickerPrefab, stickerContainer);
        }
        activeSticker.Add(sticker);
        sticker.gameObject.SetActive(true);
        sticker.transform.SetAsLastSibling();
        sticker.name = $"Sticker_{GetActiveStickers().Count}";
        return sticker;
    }

    public void ReturnSticker(Sticker sticker)
    {
        if (activeSticker.Contains(sticker))
        {
            activeSticker.Remove(sticker);
            stickerPool.Enqueue(sticker);
            sticker.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Sticker not found in active lines.");
        }
    }

    public void ReturnAllStickers()
    {
        foreach (var sticker in activeSticker)
        {
            sticker.gameObject.SetActive(false);
            stickerPool.Enqueue(sticker);
        }
        activeSticker.Clear();
    }

    public List<Sticker> GetActiveStickers()
    {
        return activeSticker;
    }

    public Sticker GetStickerByNumber(int number)
    {
        if (number > 0 && number <= activeSticker.Count)
            return activeSticker[number - 1];
        return null;
    }
}
