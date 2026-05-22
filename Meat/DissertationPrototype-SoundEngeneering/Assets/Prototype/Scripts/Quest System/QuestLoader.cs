// using System.IO;
// using UnityEngine;

// public static class QuestLoader
// {
//     public static QuestData getQuestFromFile(string fileName)
// {
//     string path = Path.Combine(Application.streamingAssetsPath, fileName);

//     try
//     {
//         if (!File.Exists(path))
//         {
//             Debug.LogError($"[QuestLoader.cs] File not found: {path}");
//             return null;
//         }

//         string json = File.ReadAllText(path);
//         return JsonUtility.FromJson<QuestData>(json);
//     }
//     catch (System.Exception ex)
//     {
//         Debug.LogError($"[QuestLoader.cs] Failed to read/parse quest file at {path}\n{ex}");
//         return null;
//     }
// }
// }

using System.IO;
using UnityEngine;

public static class QuestLoader
{
    public static QuestData GetQuestFromFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            Debug.LogError("[QuestLoader.cs] File name is empty.");
            return null;
        }

        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        try
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"[QuestLoader.cs] File not found: {path}");
                return null;
            }

            string json = File.ReadAllText(path);
            QuestData quest = JsonUtility.FromJson<QuestData>(json);

            if (quest == null)
            {
                Debug.LogError($"[QuestLoader.cs] Parsed quest is null: {path}");
                return null;
            }

            return quest;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QuestLoader.cs] Failed to read/parse quest file at {path}\n{ex}");
            return null;
        }
    }

    // Kept for compatibility with your older method name.
    public static QuestData getQuestFromFile(string fileName)
    {
        return GetQuestFromFile(fileName);
    }
}