using System;

[Serializable]
public class QuestGenRecord
{
    public string schema_version;
    public string generation_id;
    public string timestamp_utc;

    public GenerationCondition condition;
    public TriggerContext trigger_context;
    public GenerationInputSnapshot input_snapshot;
    public QuestData generated_quest;
    public GenerationOutcome outcome;
}

[Serializable]
public class GenerationCondition
{
    public bool profile_conditioned;
    public string generator_mode;
    public string model;
    public string notes;
}

[Serializable]
public class TriggerContext
{
    public string trigger_type;
    public float trigger_time_seconds;
    public bool quest_request_allowed;
    public string active_scene;
}

[Serializable]
public class GenerationInputSnapshot
{
    public PlayerProfileData player_profile;
    public string game_scope_file;
    public string quest_rules_file;
}

[Serializable]
public class GenerationOutcome
{
    public string status;
    public bool started_in_game;
    public bool completed;
    public bool failed;
    public float completion_time_seconds;
    public string completion_timestamp_utc;
}