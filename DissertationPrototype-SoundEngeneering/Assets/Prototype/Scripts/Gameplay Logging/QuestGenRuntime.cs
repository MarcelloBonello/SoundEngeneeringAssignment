using UnityEngine;

public class QuestGenRuntime : MonoBehaviour
{
    public static QuestGenRuntime Instance { get; private set; }

    public QuestGenRecord CurrentRecord { get; private set; }
    public string CurrentRecordPath { get; private set; }

    private float currentQuestStartTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BeginRecord(QuestGenRecord record)
    {
        CurrentRecord = record;
        CurrentRecordPath = QuestGenRecordSave.CreateRecord(record);
    }

    public void MarkQuestStarted()
    {
        if (CurrentRecord == null) return;

        currentQuestStartTime = Time.time;
        CurrentRecord.outcome.status = "started";
        CurrentRecord.outcome.started_in_game = true;

        QuestGenRecordSave.UpdateRecord(CurrentRecordPath, CurrentRecord);
    }

    public void MarkQuestCompleted()
    {
        if (CurrentRecord == null) return;

        CurrentRecord.outcome.status = "completed";
        CurrentRecord.outcome.completed = true;
        CurrentRecord.outcome.failed = false;
        CurrentRecord.outcome.completion_time_seconds = Time.time - currentQuestStartTime;
        CurrentRecord.outcome.completion_timestamp_utc = System.DateTime.UtcNow.ToString("o");

        QuestGenRecordSave.UpdateRecord(CurrentRecordPath, CurrentRecord);
    }

    public void MarkQuestFailed()
    {
        if (CurrentRecord == null) return;

        CurrentRecord.outcome.status = "failed";
        CurrentRecord.outcome.completed = false;
        CurrentRecord.outcome.failed = true;
        CurrentRecord.outcome.completion_time_seconds = Time.time - currentQuestStartTime;
        CurrentRecord.outcome.completion_timestamp_utc = System.DateTime.UtcNow.ToString("o");

        QuestGenRecordSave.UpdateRecord(CurrentRecordPath, CurrentRecord);
    }
}