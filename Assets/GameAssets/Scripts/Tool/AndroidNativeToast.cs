using UnityEngine;

public static class AndroidNativeToast
{
    /// <summary>
    /// Hiển thị một thông báo Toast gốc của Android.
    /// </summary>
    /// <param name="message">Nội dung thông báo cần hiển thị.</param>
    /// <param name="isLong">True nếu muốn thời gian hiển thị dài, False cho thời gian ngắn.</param>
    public static void Show(string message, bool isLong = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        
        // Lấy Activity hiện tại của game
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        // Thời gian hiển thị của Toast (LENGTH_LONG hoặc LENGTH_SHORT)
        int duration = isLong ? 1 : 0; // 1 tương ứng với Toast.LENGTH_LONG

        // Run Toast on UI thread of Android
        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            // create Toast
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, duration);
            
            // show Toast
            toastObject.Call("show");
        }));
#else
        Debug.Log("Toast (Android): " + message);
#endif
    }
}