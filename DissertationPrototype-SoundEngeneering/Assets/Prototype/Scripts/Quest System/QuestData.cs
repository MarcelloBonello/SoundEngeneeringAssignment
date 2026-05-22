using System;
using UnityEngine;

public enum QuestType
{
    Unknown,
    Exploration,
    Combat,
    Mixed
}

[Serializable]
public class QuestData
{
    public string quest_id;
    public string title;
    public string description;

    public string quest_type;

    public string[] tags;
    public RewardData reward;
    public QuestStepData[] steps;

    // Runtime-only value used by QuestManager after inference.
    [NonSerialized] public QuestType runtimeQuestType = QuestType.Unknown;
}

[Serializable]
public class RewardData
{
    public int xp;
    public int gold;
}

[Serializable]
public class QuestStepData
{
    public string step_id;
    public string type;
    public string target;
    public int required;
    public int progress;
    public string text;
}