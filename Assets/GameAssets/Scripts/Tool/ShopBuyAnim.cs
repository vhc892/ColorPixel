using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Helper;

public class ShopBuyAnim : MonoBehaviour
{
    public static ShopBuyAnim Instance;

    [Header("Configuration")]
    [SerializeField] private float flyOutDuration = 0.3f;
    [SerializeField] private float flyOutSpreadRadius = 150f;
    [SerializeField] private float delayBetweenItems = 0.05f;
    [SerializeField] private float flyToTargetDuration = 0.5f;
    [SerializeField] private Ease easeFlyOut = Ease.OutQuad;
    [SerializeField] private Ease easeFlyToTarget = Ease.InQuad;

    [Header("References")]
    [SerializeField] private Canvas parentCanvas;
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject fillPrefab;
    [SerializeField] private GameObject boomPrefab;
    [SerializeField] private GameObject findPrefab;

    [Header("Object Pooling")]
    [SerializeField] private int initialPoolSize = 20;
    private Dictionary<RewardType, Queue<GameObject>> rewardPools;

    //public enum RewardType { Gold, Fill, Boom, Find }

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
    void Start()
    {
        InitializePools();
    }

    public void AnimateReward(RectTransform startTransform, RectTransform endTransform, RewardType type, int quantity)
    {
        if (GetPrefabForType(type) == null)
        {
            Debug.LogError("Prefab for reward type " + type.ToString() + " is not set!");
            return;
        }

        StartCoroutine(RewardSequence(startTransform, endTransform, type, quantity));
    }

    public void AnimateReward(Vector3 pos, RectTransform endTransform, RewardType type, int quantity)
    {
        if (GetPrefabForType(type) == null)
        {
            Debug.LogError("Prefab for reward type " + type.ToString() + " is not set!");
            return;
        }

        StartCoroutine(RewardSequence(pos, endTransform, type, quantity));
    }

    private IEnumerator RewardSequence(Vector3 pos, RectTransform endTransform, RewardType type, int quantity)
    {
        int animationCount = Mathf.Min(quantity, 10);

        for (int i = 0; i < animationCount; i++)
        {
            GameObject rewardInstance = GetFromPool(type);
            rewardInstance.transform.position = pos;

            Vector3 intermediatePos = pos + (Vector3)(Random.insideUnitCircle * flyOutSpreadRadius);

            Sequence seq = DOTween.Sequence();
            seq.Append(rewardInstance.transform.DOMove(intermediatePos, flyOutDuration).SetEase(easeFlyOut));
            seq.Append(rewardInstance.transform.DOMove(endTransform.position, flyToTargetDuration).SetEase(easeFlyToTarget));

            seq.OnComplete(() =>
            {
                ReturnToPool(type, rewardInstance);
                if (type == RewardType.Coin)
                {
                    AudioManager.Instance.CoinReceivedSfx();
                }
                else
                {
                    AudioManager.Instance.CollectItemSfx();
                }
            });

            yield return new WaitForSeconds(delayBetweenItems);
        }
    }

    private IEnumerator RewardSequence(RectTransform startTransform, RectTransform endTransform, RewardType type, int quantity)
    {
        int animationCount = Mathf.Min(quantity, 10);

        for (int i = 0; i < animationCount; i++)
        {
            GameObject rewardInstance = GetFromPool(type);
            rewardInstance.transform.position = startTransform.position;

            Vector3 intermediatePos = startTransform.position + (Vector3)(Random.insideUnitCircle * flyOutSpreadRadius);

            Sequence seq = DOTween.Sequence();
            seq.Append(rewardInstance.transform.DOMove(intermediatePos, flyOutDuration).SetEase(easeFlyOut));
            seq.Append(rewardInstance.transform.DOMove(endTransform.position, flyToTargetDuration).SetEase(easeFlyToTarget));

            seq.OnComplete(() =>
            {
                ReturnToPool(type, rewardInstance);
                if (type == RewardType.Coin)
                {
                    AudioManager.Instance.CoinReceivedSfx();
                }
                else
                {
                    AudioManager.Instance.CollectItemSfx();
                }
            });

            yield return new WaitForSeconds(delayBetweenItems);
        }
    }

    private GameObject GetPrefabForType(RewardType type)
    {
        switch (type)
        {
            case RewardType.Coin: return goldPrefab;
            case RewardType.Fill: return fillPrefab;
            case RewardType.Boom: return boomPrefab;
            case RewardType.Find: return findPrefab;
            default: return null;
        }
    }
    private void InitializePools()
    {
        rewardPools = new Dictionary<RewardType, Queue<GameObject>>();

        foreach (RewardType type in System.Enum.GetValues(typeof(RewardType)))
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();
            GameObject prefab = GetPrefabForType(type);

            if (prefab == null) continue;

            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = Instantiate(prefab, parentCanvas.transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }
            rewardPools.Add(type, objectQueue);
        }
    }
    private GameObject GetFromPool(RewardType type)
    {
        if (!rewardPools.ContainsKey(type) || rewardPools[type].Count == 0)
        {
            GameObject prefab = GetPrefabForType(type);
            GameObject newObj = Instantiate(prefab, parentCanvas.transform);
            return newObj;
        }

        GameObject obj = rewardPools[type].Dequeue();
        obj.SetActive(true);
        return obj;
    }
    private void ReturnToPool(RewardType type, GameObject obj)
    {
        if (!rewardPools.ContainsKey(type)) return;

        obj.SetActive(false);
        rewardPools[type].Enqueue(obj);
    }
}