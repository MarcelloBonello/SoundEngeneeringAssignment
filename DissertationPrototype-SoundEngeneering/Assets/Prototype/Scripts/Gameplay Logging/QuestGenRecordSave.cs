// using System.IO;
// using UnityEngine;
// using Newtonsoft.Json;
// using System;

// public static class QuestGenRecordSave
// {
//     private static string FolderPath =>
//         Path.Combine(Application.persistentDataPath, "QuestGenerationRecords");

//     public static string CreateRecord(QuestGenRecord record)
//     {
//         if (record == null)
//         {
//             Debug.LogError("[QuestGenerationRecordSave.cs] Cannot create record: record is null.");
//             return null;
//         }

//         Directory.CreateDirectory(FolderPath);

//         string timestamp = DateTime.UtcNow.ToString("__yyyy-MM-dd__HH-mm-ss-fff");
//         string questId = record.generated_quest != null ? record.generated_quest.quest_id : "unknownQuest";

//         string fileName = $"QuestGenLog_{questId}_{timestamp}.json";
//         string filePath = Path.Combine(FolderPath, fileName);

//         string json = JsonConvert.SerializeObject(record, Formatting.Indented);
//         File.WriteAllText(filePath, json);

//         Debug.Log($"[QuestGenerationRecordSave.cs] Record created: {filePath}");
//         return filePath;
//     }

//     public static void UpdateRecord(string filePath, QuestGenRecord record)
//     {
//         if (string.IsNullOrEmpty(filePath))
//         {
//             Debug.LogError("[QuestGenerationRecordSave.cs] Cannot update record: filePath is null or empty.");
//             return;
//         }

//         if (record == null)
//         {
//             Debug.LogError("[QuestGenerationRecordSave.cs] Cannot update record: record is null.");
//             return;
//         }

//         string json = JsonConvert.SerializeObject(record, Formatting.Indented);
//         File.WriteAllText(filePath, json);

//         Debug.Log($"[QuestGenerationRecordSave.cs] Record updated: {filePath}");
//     }
// }

using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

public static class QuestGenRecordSave
{
    private static string FolderPath => PlaytestSavePaths.QuestGenerationFolder;

    public static string CreateRecord(QuestGenRecord record)
    {
        if (record == null)
        {
            Debug.LogError("[QuestGenerationRecordSave.cs] Cannot create record: record is null.");
            return null;
        }

        PlaytestSavePaths.EnsureFoldersExist();

        string timestamp = DateTime.UtcNow.ToString("__yyyy-MM-dd__HH-mm-ss-fff");
        string questId = record.generated_quest != null ? record.generated_quest.quest_id : "unknownQuest";
        questId = MakeSafeFileName(questId);

        string fileName = $"QuestGenLog_{questId}_{timestamp}.json";
        string filePath = Path.Combine(FolderPath, fileName);

        string json = JsonConvert.SerializeObject(record, Formatting.Indented);
        File.WriteAllText(filePath, json);

        Debug.Log($"[QuestGenerationRecordSave.cs] Record created: {filePath}");
        return filePath;
    }

    public static void UpdateRecord(string filePath, QuestGenRecord record)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("[QuestGenerationRecordSave.cs] Cannot update record: filePath is null or empty.");
            return;
        }

        if (record == null)
        {
            Debug.LogError("[QuestGenerationRecordSave.cs] Cannot update record: record is null.");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        string json = JsonConvert.SerializeObject(record, Formatting.Indented);
        File.WriteAllText(filePath, json);

        Debug.Log($"[QuestGenerationRecordSave.cs] Record updated: {filePath}");
    }

    private static string MakeSafeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "unknownQuest";

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        return fileName;
    }
}