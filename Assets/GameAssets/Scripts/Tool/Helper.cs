using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    public enum InputState
    {
        None,
        Zoom,
        Drag,
        Painting,
    }

    public enum PaintBooster
    {
        None,
        Fill,
        Boom,
        Find,
    }

    public enum RewardType
    {
        Coin,
        Fill,
        Boom,
        Find,
        NewPainting,
        Spin,
        RandomForDaily,
    }

    public enum QuestType
    {
        //Achievement
        Achievement_Picture,
        Achievement_Pixel,
        Achievement_Fill,
        Achievement_Boom,
        Achievement_Find,
        Achievement_Import_n_Paint,
        Achievement_Event,
        Achievement_Different_Category,
        Achievement_Frequently,
        Achievement_Pictures_Same_Day,
        Achievement_Cute_Category_Picture,
        Achievement_Picture_Each_Category,

        //Daily Quest
        Daily_100_Blocks,
        Daily_1_Picture,
        Daily_3_Boosters,
        Daily_3_Picture,
        Daily_Watch_Ads,
        Daily_Finish_Color,
        Daily_Import_Picture,
    }

    public enum ConceptType
    {
        Hero,
        Anime,
        Kpop,
        Game,
        Cute,
        Trend,
        Cartoon,
    }

    public enum DecorType
    {
        Background,
        Frame,
        Sticker,
    }

    public class ScriptTool
    {
        public static void ShuffleArray<T>(T[] array)
        {
            System.Random rng = new System.Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (array[k], array[n]) = (array[n], array[k]);
            }
        }

        public static void ShuffleList<T>(List<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static Vector2 WorldToUIPosition(Vector3 worldPos, Canvas canvas, Camera cam)
        {
            Vector2 screenPos;

            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    break;

                case RenderMode.ScreenSpaceCamera:
                    screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);
                    break;

                case RenderMode.WorldSpace:
                    return canvas.transform.InverseTransformPoint(worldPos);

                default:
                    screenPos = Vector2.zero;
                    break;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint
            );

            return localPoint;
        }
    }

    public class Daily : MonoBehaviour
    {
        private const string LastLoginKey = "LastLoginDate";
        public static bool IsNewDay()
        {
            string lastDateStr = PlayerPrefs.GetString(LastLoginKey, "");
            DateTime today = DateTime.Now.Date; // chỉ lấy ngày (0h)

            if (string.IsNullOrEmpty(lastDateStr))
            {
                // Chưa có dữ liệu => lần đầu chơi
                return true;
            }

            DateTime lastDate = DateTime.Parse(lastDateStr);

            if (lastDate < today) // khác ngày
            {
                return true;
            }

            return false; // cùng ngày => không reset
        }

        public static void UpdateNewDay()
        {
            string lastDateStr = PlayerPrefs.GetString(LastLoginKey, "");
            DateTime today = DateTime.Now.Date; // chỉ lấy ngày (0h)

            if (string.IsNullOrEmpty(lastDateStr))
            {
                // Chưa có dữ liệu => lần đầu chơi
                PlayerPrefs.SetString(LastLoginKey, today.ToString("yyyy-MM-dd"));
                return;
            }

            DateTime lastDate = DateTime.Parse(lastDateStr);

            if (lastDate < today) // khác ngày
            {
                PlayerPrefs.SetString(LastLoginKey, today.ToString("yyyy-MM-dd"));
            }
        }
    }
}