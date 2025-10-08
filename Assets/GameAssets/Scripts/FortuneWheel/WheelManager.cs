using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine.UI;

public class WheelManager : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform wheel;
    public List<WheelRewardSO> rewards;
    public TextMeshProUGUI text;
    public Image spinButton;

    [Header("Spin")]
    public int minFullTurns = 5;
    public float spinDuration = 4f;
    public float segment0AngleOffset = 0f;

    [Header("Animation")]
    [SerializeField] private RectTransform rewardSpawnPoint;

    private static bool isSpinning;

    private DateTime lastCheck;

    public static bool firstTimeShowSpin = false;

    void Start()
    {
        if (Helper.Daily.IsNewDay())
        {
            PlayerManager.Instance.spinAmount = 1;
            if (PlayerPrefs.GetInt("HasSeenTutorial", 0) == 0)
            {
                firstTimeShowSpin = true;

            }
            else
            {
                firstTimeShowSpin = false;
                UIManager.Instance.ShowSpinPopup();
            }
        }
        text.SetText($"Daily spin limit: {PlayerManager.Instance.spinAmount}/1");
        UpdateSpinButton();
        lastCheck = DateTime.Now;
    }
    void Update()
    {
        if ((DateTime.Now - lastCheck).TotalSeconds > 60) // check mỗi phút
        {
            lastCheck = DateTime.Now;
            if (Helper.Daily.IsNewDay())
            {
                PlayerManager.Instance.spinAmount = 1;
                text.SetText($"Daily spin limit: {PlayerManager.Instance.spinAmount}/1");
                UpdateSpinButton();
            }
        }
    }
    public void SpinRandom()
    {
        if (!CanSpin()) return;
        if (PlayerManager.Instance.spinAmount > 0)
        {
            PlayerManager.Instance.spinAmount--;
            text.SetText($"Daily spin limit: {PlayerManager.Instance.spinAmount}/1");
            int idx = UnityEngine.Random.Range(0, rewards.Count);
            SpinToIndex(idx);
        }
        else
        {
            Debug.Log("Watch Reward Ads to Spin");
            int idx = UnityEngine.Random.Range(0, rewards.Count);
            SpinToIndex(idx);
        }
        AudioManager.Instance.WheelSpinSfx();
        UpdateSpinButton();
    }

    public void SpinToIndex(int index)
    {
        if (!CanSpin()) return;

        index = Mathf.Clamp(index, 0, rewards.Count - 1);
        float segAngle = 360f / rewards.Count;

        // Góc mục tiêu (so với Up)
        float targetAngleFromUp = (index * segAngle);

        // Góc hiện tại của wheel so với Up + offset
        float currentZ = wheel.eulerAngles.z;
        float startAngleFromUp = NormalizeAngleFromUp(currentZ - segment0AngleOffset);

        // Tính góc cần xoay (theo chiều kim đồng hồ)
        float deltaAngle = targetAngleFromUp - startAngleFromUp;
        if (deltaAngle < 0) deltaAngle += 360f;

        // Tổng góc xoay (âm = chiều kim đồng hồ)
        float totalAngle = -(minFullTurns * 360f + deltaAngle);

        isSpinning = true;

        wheel.DORotate(
            new Vector3(0, 0, currentZ + totalAngle),
            spinDuration,
            RotateMode.FastBeyond360
        )
        .SetEase(Ease.OutQuart)
        .OnComplete(() =>
        {
            float finalAngle = wheel.eulerAngles.z;
            int finalIndex = GetIndexFromWheelAngle(finalAngle);

            WheelRewardSO reward = rewards[finalIndex];
            Debug.Log(reward.name);
            OnSpinResult(reward);
            isSpinning = false;
        });
    }

    private void UpdateSpinButton()
    {
        if (PlayerManager.Instance.spinAmount > 0)
        {
            spinButton.sprite = GameAssets.i.spinButton;
        }
        else
        {
            spinButton.sprite = GameAssets.i.adsSpinButton;
        }
    }

    private bool CanSpin()
    {
        if (isSpinning) return false;
        if (wheel == null || rewards == null || rewards.Count == 0) return false;
        return true;
    }

    private float NormalizeAngleFromUp(float currentZ)
    {
        float a = currentZ + 90f;
        a %= 360f;
        if (a < 0) a += 360f;
        return a;
    }

    private void OnSpinResult(WheelRewardSO rewardSO)
    {
        RectTransform destination = PlayerManager.Instance.coinContainer;

        switch (rewardSO.rewardType)
        {
            case Helper.RewardType.Coin:
                Debug.Log($"Get {rewardSO.amount} coin!");
                PlayerManager.Instance.AddCoinWithAnimation(rewardSO.amount, rewardSpawnPoint);
                break;
            case Helper.RewardType.Fill:
                Debug.Log($"Get {rewardSO.amount} fillBooster!");
                ShopBuyAnim.Instance.AnimateReward(rewardSpawnPoint, destination, rewardSO.rewardType, rewardSO.amount);
                PlayerManager.Instance.AddFillBooster(rewardSO.amount);
                break;
            case Helper.RewardType.Boom:
                Debug.Log($"Get {rewardSO.amount} boomBooster!");
                ShopBuyAnim.Instance.AnimateReward(rewardSpawnPoint, destination, rewardSO.rewardType, rewardSO.amount);
                PlayerManager.Instance.AddBoomBooster(rewardSO.amount);
                break;
            case Helper.RewardType.Find:
                Debug.Log($"Get {rewardSO.amount} findBooster!");
                ShopBuyAnim.Instance.AnimateReward(rewardSpawnPoint, destination, rewardSO.rewardType, rewardSO.amount);
                PlayerManager.Instance.AddFindBooster(rewardSO.amount);
                break;
        }
    }

    private int GetIndexFromWheelAngle(float zAngle)
    {
        float segAngle = 360f / rewards.Count;

        // chuẩn hoá về [0,360)
        float normalized = Mathf.Repeat(zAngle - segment0AngleOffset, 360f);

        for (int i = 0; i < rewards.Count; i++)
        {
            // góc chuẩn (ví dụ ô đầu ở segAngle/2, ô 1 ở 45°)
            float centerAngle = i * segAngle + segAngle / 2f;

            // độ lệch giữa góc hiện tại và góc chuẩn
            float diff = Mathf.Abs(Mathf.DeltaAngle(normalized, centerAngle));

            if (diff <= 20f)
            {
                return i;
            }
        }

        // fallback: nếu không khớp ô nào thì tính gần nhất
        return Mathf.RoundToInt(normalized / segAngle) % rewards.Count;
    }


    public static bool GetIsSpinning()
    {
        return isSpinning;
    }
}
