using System.IO;
using UnityEngine;

/// <summary>
/// Generic save/load system cho bất kỳ kiểu dữ liệu T.
/// Kế thừa để đặt tên file và thư mục lưu riêng.
/// </summary>
public abstract class BaseSaveSystem<T> where T : class
{
    /// <summary>Tên file (vd: "player.json").</summary>
    protected abstract string FileName { get; }

    /// <summary>Thư mục lưu (mặc định: Application.persistentDataPath).</summary>
    protected virtual string FolderPath => Application.persistentDataPath;

    protected string SavePath => Path.Combine(FolderPath, FileName);

    public virtual void Save(T data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"{typeof(T).Name} saved: {SavePath}");
    }

    public virtual T Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"No save file found for {typeof(T).Name}");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<T>(json);
    }

    public virtual bool Exists() => File.Exists(SavePath);

    public virtual void Delete()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log($"{typeof(T).Name} save file deleted.");
        }
    }
}
