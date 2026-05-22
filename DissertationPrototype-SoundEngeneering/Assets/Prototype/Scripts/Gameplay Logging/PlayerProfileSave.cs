// using System.IO;
// using UnityEngine;
// using Newtonsoft.Json;
// using System;

// public class PlayerProfileSave : MonoBehaviour
// {
//     private static string FolderPath =>
//         Path.Combine(Application.persistentDataPath, "PlayerModelingRecords");

//     public static void Save(PlayerProfileData data)
//     {
//         if (data == null)
//         {
//             Debug.LogWarning("[PlayerProfileSave.cs] Cannot save: PlayerProfileData is null.");
//             return;
//         }

//         Directory.CreateDirectory(FolderPath);

//         string timestamp = DateTime.UtcNow.ToString("__yyyy-MM-dd__HH-mm-ss-fff");
//         string fileName = $"player_profile_{timestamp}.json";
//         string filePath = Path.Combine(FolderPath, fileName);

//         var settings = new JsonSerializerSettings
//         {
//             Formatting = Formatting.Indented,
//             TypeNameHandling = TypeNameHandling.Auto
//         };

//         string json = JsonConvert.SerializeObject(data, settings);

//         File.WriteAllText(filePath, json);

//         Debug.Log($"[PlayerProfileSave.cs] Profile saved to: {filePath}");
//     }
// }

using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class PlayerProfileSave : MonoBehaviour
{
    private static string FolderPath => PlaytestSavePaths.PlayerModelingFolder;

    public static void Save(PlayerProfileData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[PlayerProfileSave.cs] Cannot save: PlayerProfileData is null.");
            return;
        }

        PlaytestSavePaths.EnsureFoldersExist();

        string timestamp = DateTime.UtcNow.ToString("__yyyy-MM-dd__HH-mm-ss-fff");
        string fileName = $"player_profile_{timestamp}.json";
        string filePath = Path.Combine(FolderPath, fileName);

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };

        string json = JsonConvert.SerializeObject(data, settings);

        File.WriteAllText(filePath, json);

        Debug.Log($"[PlayerProfileSave.cs] Profile saved to: {filePath}");
    }
}