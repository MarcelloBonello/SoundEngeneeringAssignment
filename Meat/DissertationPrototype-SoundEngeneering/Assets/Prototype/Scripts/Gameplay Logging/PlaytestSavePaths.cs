using System;
using System.IO;
using UnityEngine;

public static class PlaytestSavePaths
{
    private const string RootFolderName = "PlaytestGameplayLogging";
    private const string PlayerModelingFolderName = "PlayerModelingRecords";
    private const string QuestGenerationFolderName = "QuestGenerationRecords";

    public static string RootFolder
    {
        get
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads");

            if (string.IsNullOrWhiteSpace(downloadsPath))
            {
                Debug.LogWarning("[PlaytestSavePaths] Downloads folder not found. Falling back to persistentDataPath.");
                downloadsPath = Application.persistentDataPath;
            }

            return Path.Combine(downloadsPath, RootFolderName);
        }
    }

    public static string PlayerModelingFolder
    {
        get
        {
            return Path.Combine(RootFolder, PlayerModelingFolderName);
        }
    }

    public static string QuestGenerationFolder
    {
        get
        {
            return Path.Combine(RootFolder, QuestGenerationFolderName);
        }
    }

    public static void EnsureFoldersExist()
    {
        Directory.CreateDirectory(RootFolder);
        Directory.CreateDirectory(PlayerModelingFolder);
        Directory.CreateDirectory(QuestGenerationFolder);

        Debug.Log($"[PlaytestSavePaths] Save folders ready at: {RootFolder}");
    }

    public static void OpenRootFolder()
    {
        EnsureFoldersExist();

        string fixedPath = RootFolder.Replace("\\", "/");
        Application.OpenURL("file:///" + fixedPath);
    }
}