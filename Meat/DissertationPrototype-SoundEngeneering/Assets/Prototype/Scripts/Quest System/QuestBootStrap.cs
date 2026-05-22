//using UnityEngine;

//public class QuestBootStrap : MonoBehaviour
//{
//    [SerializeField] private QuestManager questManager;
//    [SerializeField] private string questFileName = "generatedQuest.json";

//    private void Start()
//    {
//        if (questManager == null)
//        {
//            Debug.LogError("[QuestBootstrap.cs] QuestManager not assigned.");
//            return;
//        }

//        QuestData quest = QuestLoader.getQuestFromFile(questFileName);

//        Debug.Log($"[QuestBootstrap.cs] Loading quest from: {Application.streamingAssetsPath}/{questFileName}");

//        if (quest == null)
//        {
//            Debug.LogError("[QuestBootstrap.cs] Failed to load quest JSON.");
//            return;
//        }

//        questManager.StartQuest(quest);
//    }
//}
