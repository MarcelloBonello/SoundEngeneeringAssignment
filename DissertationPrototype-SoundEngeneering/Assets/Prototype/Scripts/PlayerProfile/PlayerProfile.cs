// using System;
// using System.Collections.Generic;
// using System.Diagnostics.Contracts;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;

// [Serializable]
// public class PlayerProfileData
// {
//     // =========================
//     // EXPLORATION METRICS
//     // ========================= 

//     // Unique areas visited (Forest, Desert, Castle, Dungeon, etc.)
//     public HashSet<string> areasVisited = new HashSet<string>();

//     // Time spent per area (seconds)
//     public Dictionary<string, float> timeInArea = new Dictionary<string, float>();

//     // Total distance travelled by the player
//     public float distanceTravelled;



//     // =========================
//     // COMBAT METRICS
//     // ========================= 

//     // Engagement & outcomes
//     public int enemiesAggroed;
//     public int enemiesKilled;

//     // Damage tracking
//     public float damageDealt;
//     public float damageTaken;

//     // Combat style
//     public int meleeHits;
//     public int rangedHits;

//     // Weapon preferences
//     public int swordHits;
//     public int bluntHits;



//     // =========================
//     // ACHIEVER / QUEST METRICS
//     // =========================

//     // Quest outcomes
//     public int questsCompleted;
//     public int questsFailed;

//     // Total time spent completing quests (seconds)
//     public float totalQuestCompletionTime;

//     // Breakdown by quest template (e.g. CLEAR_DUNGEON, SCOUT_AREA)
//     public Dictionary<string, int> completedByTemplateId =
//         new Dictionary<string, int>();

//     // Breakdown by quest type (combat / exploration / etc.)
//     public Dictionary<QuestType, int> completedByQuestType =
//         new Dictionary<QuestType, int>();

//     // Breakdown by quest score (combat vs exploration alignment)
//     public Dictionary<string, int> completedByQuestScore =
//         new Dictionary<string, int>(); // e.g. "combat", "exploration"



//     // =============================
//     // DERIVED BARTLE SCORES (0�1)
//     // =============================

//     [Range(0f, 1f)]
//     public float explorationScore01;

//     [Range(0f, 1f)]
//     public float combatScore01;

//     [Range(0f, 1f)]
//     public float achieverScore01;

// }

// public class PlayerProfile : MonoBehaviour
// {
//     public static PlayerProfile Instance;

//     [Header("Data")]
//     public PlayerProfileData Data = new PlayerProfileData();

//     [Header("Quest Generation")]
//     [SerializeField] private QuestGenCoordinator questGenCodr;

//     [Header("Runtime")]
//     public string CurrentAreaId { get; private set; } = "";

//     [Header("Recalculate Scores every N Seconds (0 = manual)")]
//     [SerializeField] private float autoRecalcInterval = 1f;
//     private float recalcTimer;

//     [Header("Normalization Caps")]
//     [Tooltip("How many unique areas exist in your prototype (Forest, Desert, Castle, Dungeon = 4).")]
//     public int capAreasVisited = 4;

//     [Tooltip("How much total explore time (seconds) counts as 'max'.")]
//     public float capTotalExploreTime = 600f;

//     [Tooltip("How much distance (meters) counts as 'max'.")]
//     public float capDistance = 2000f;

//     [Tooltip("How much damage dealt counts as 'max'.")]
//     public float capDamageDealt = 500f;

//     [Tooltip("How much damage taken counts as 'max'.")]
//     public float capDamageTaken = 500f;

//     [Tooltip("How many kills counts as 'max'.")]
//     public int capKills = 20;

//     [Tooltip("How many enemy aggro events counts as 'max'.")]
//     public int capAggro = 30;

//     [Tooltip("How many quests completed counts as 'max'.")]
//     public int capQuestsCompleted = 10;

//     [Tooltip("Average completion time (seconds) where faster = better (used for achiever speed).")]
//     public float capAvgQuestTime = 300f;

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         else
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//     }

//     private void Update()
//     {
//         if (questGenCodr != null) questGenCodr.Tick(Data, Time.deltaTime);

//         if (autoRecalcInterval <= 0f) return;

//         recalcTimer += Time.deltaTime;

//         if(recalcTimer >= autoRecalcInterval)
//         {
//             recalcTimer = 0f;
//             RecalculateAllScores();
//         }
//     }

//     // =========================
//     // EXPLORATION 
//     // =========================

//     /// <summary>Call when the player enters a new area trigger.</summary>
//     public void SetCurrentArea(string areaID)//
//     {
//         CurrentAreaId = areaID ?? "";

//         if (!string.IsNullOrEmpty(CurrentAreaId))
//             Data.areasVisited.Add(CurrentAreaId);
//     }

//     /// <summary>Call every frame (or fixed interval) while in an area.</summary>
//     public void AddTimerInCurrentArea(float dt)//
//     {
//         if (string.IsNullOrEmpty(CurrentAreaId)) return;
//         if (dt <= 0f) return;

//         if (!Data.timeInArea.ContainsKey(CurrentAreaId))
//             Data.timeInArea[CurrentAreaId] = 0f;

//         Data.timeInArea[CurrentAreaId] += dt;
//     }

//     /// <summary>Call from a DistanceTracker to accumulate meters travelled.</summary>
//     public void AddDistance(float meters)//
//     {
//         if (meters <= 0f) return;
//         Data.distanceTravelled += meters;
//     }

//     // =========================
//     // COMBAT 
//     // =========================

//     public void RecordAggro()//
//     {
//         Data.enemiesAggroed++;
//     }

//     public void RecordKill()//
//     {
//         Data.enemiesKilled++;
//     }

//     public void RecordDamageTaken(float amouint)//
//     {
//         if(amouint <= 0f) return;
//         Data.damageTaken += amouint;
//     }

//     public void RecordDamageDealt(float amount)//
//     {
//         if (amount <= 0f) return;
//         Data.damageDealt += amount;
//     }

//     public void RecordHit(AttackType attackType, WeaponType weaponType)//
//     {
//         if (attackType == AttackType.Melee) Data.meleeHits++;
//         else Data.rangedHits++;

//         if (weaponType == WeaponType.Sword) Data.meleeHits++;
//         else if (weaponType == WeaponType.Blunt) Data.bluntHits++;
//     }

//     // =========================
//     // QUEST / ACHIEVER 
//     // ========================= 

//     /// <summary>
//     /// Call when a quest completes successfully.
//     /// questScore is a string like "combat" or "exploration" to match your template idea.
//     /// </summary>
//     public void RecordQuestCompleted(string templateId, QuestType qeustType, string questScore, float completionTimeSec)
//     {
//         Data.questsCompleted++;
//         Data.totalQuestCompletionTime += Mathf.Max(0f, completionTimeSec);

//         if(!string.IsNullOrEmpty(templateId))
//         {
//             if (!Data.completedByTemplateId.ContainsKey(templateId))
//             {
//                 Data.completedByTemplateId[templateId] = 0;
//             }

//             Data.completedByTemplateId[templateId]++;
//         }

//         if (!Data.completedByQuestType.ContainsKey(qeustType))
//         {
//             Data.completedByQuestType[qeustType] = 0;
//         }
            
//         Data.completedByQuestType[qeustType]++;

//         if(!string.IsNullOrEmpty(questScore))
//         {
//             questScore = questScore.Trim().ToLowerInvariant();
//             if (!Data.completedByQuestScore.ContainsKey(questScore))
//             {
//                 Data.completedByQuestScore[questScore] = 0;
//             }
               
//             Data.completedByQuestScore[questScore]++;
//         }
//     }

//     /// <summary>Call when a quest fails.</summary>
//     public void RecordQuestFailed()
//     {
//         Data.questsFailed++;
//     }

//     // =========================
//     // SCORE CALCULATION
//     // ========================= 

//     public void RecalculateAllScores()
//     {
//         RecalculateExplorationScore();
//         RecalculateCombatScore();
//         RecalculateAchieverScore();
//     }

//     /// <summary>
//     /// Calculates the Exploration score (0�1) based on:
//     /// - Number of unique areas visited
//     /// - Total time spent exploring areas
//     /// - Total distance travelled
//     /// This represents curiosity, movement, and world engagement.
//     /// </summary>
//     private void RecalculateExplorationScore()
//     {
//         float areas01 = Mathf.Clamp01((float)Data.areasVisited.Count / Mathf.Max(1, capAreasVisited));

//         float totalExploreTime = 0f;

//         foreach (var kv in Data.timeInArea)
//             totalExploreTime += kv.Value;

//         float time01 = Mathf.Clamp01(totalExploreTime / Mathf.Max(1f, capTotalExploreTime));

//         float dist01 = Mathf.Clamp01(Data.distanceTravelled / Mathf.Max(1f, capDistance));

//         // Explainable: average of key exploration indicators
//         Data.explorationScore01 = (areas01 + time01 + dist01) / 3f;
//     }

//     /// <summary>
//     /// Calculates the Combat score (0�1) based on:
//     /// - Damage dealt
//     /// - Damage taken (risk exposure)
//     /// - Enemies killed
//     /// - Enemy aggro events
//     /// This represents combat engagement and aggressiveness.
//     /// </summary>
//     private void RecalculateCombatScore()
//     {
//         float dealt01 = Mathf.Clamp01(Data.damageDealt / Mathf.Max(1f, capDamageDealt));
//         float taken01 = Mathf.Clamp01(Data.damageTaken / Mathf.Max(1f, capDamageTaken));
//         float kills01 = Mathf.Clamp01((float)Data.enemiesKilled / Mathf.Max(1, capKills));
//         float aggro01 = Mathf.Clamp01((float)Data.enemiesAggroed / Mathf.Max(1, capAggro));

//         // Explainable: average of key combat indicators
//         Data.combatScore01 = (dealt01 + kills01 + aggro01 + taken01) / 4f;
//     }

//     /// <summary>
//     /// Calculates the Achiever score (0�1) based on:
//     /// - Number of quests completed
//     /// - Average quest completion speed
//     /// - Quest reliability (low failure rate)
//     /// This represents goal-oriented and efficiency-driven play.
//     /// </summary>
//     private void RecalculateAchieverScore()
//     {
//         // Achiever: "completes goals and does so efficiently"
//         float completed01 = Mathf.Clamp01((float)Data.questsCompleted / Mathf.Max(1, capQuestsCompleted));

//         float avgTime = (Data.questsCompleted > 0)
//             ? Data.totalQuestCompletionTime / Data.questsCompleted
//             : capAvgQuestTime;

//         // faster completion -> higher score
//         float speed01 = 1f - Mathf.Clamp01(avgTime / Mathf.Max(1f, capAvgQuestTime));

//         // Penalize lots of failures slightly (optional, simple)
//         float failurePenalty01 = 0f;
//         int totalOutcomes = Data.questsCompleted + Data.questsFailed;
//         if (totalOutcomes > 0)
//         {
//             float failRate = (float)Data.questsFailed / totalOutcomes;
//             failurePenalty01 = Mathf.Clamp01(failRate); // 0..1
//         }
//         float reliability01 = 1f - failurePenalty01;

//         Data.achieverScore01 = (completed01 + speed01 + reliability01) / 3f;
//     }

//     // =========================
//     // DEBUG HELPERS
//     // ========================= 

//     public float GetTotalExploreTime()
//     {
//         float total = 0f;
//         foreach (var kv in Data.timeInArea)
//             total += kv.Value;
//         return total;
//     }

//     public string GetTopCompletedTemplate()
//     {
//         string best = "";
//         int bestCount = 0;

//         foreach (var kv in Data.completedByTemplateId)
//         {
//             if (kv.Value > bestCount)
//             {
//                 bestCount = kv.Value;
//                 best = kv.Key;
//             }
//         }

//         return best;
//     }
// }

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProfileData
{
    // =========================
    // EXPLORATION METRICS
    // ========================= 

    // Unique areas visited.
    public HashSet<string> areasVisited = new HashSet<string>();

    // Time spent per area in seconds.
    public Dictionary<string, float> timeInArea = new Dictionary<string, float>();

    // Total distance travelled by the player.
    public float distanceTravelled;



    // =========================
    // COMBAT METRICS
    // ========================= 

    // Engagement and outcomes.
    public int enemiesAggroed;
    public int enemiesKilled;
    public int enemiesAvoided;

    // Damage tracking.
    public float damageDealt;
    public float damageTaken;

    // Combat style.
    public int meleeHits;
    public int rangedHits;

    // Weapon preferences.
    public int swordHits;
    public int bluntHits;



    // =========================
    // ACHIEVER / QUEST METRICS
    // =========================

    // Quest outcomes.
    public int questsCompleted;
    public int questsFailed;

    // Total time spent completing quests in seconds.
    public float totalQuestCompletionTime;

    // Breakdown by quest ID/template.
    public Dictionary<string, int> completedByTemplateId =
        new Dictionary<string, int>();

    // Breakdown by quest type.
    public Dictionary<QuestType, int> completedByQuestType =
        new Dictionary<QuestType, int>();

    // Breakdown by quest score string.
    public Dictionary<string, int> completedByQuestScore =
        new Dictionary<string, int>();



    // =============================
    // DERIVED BARTLE SCORES (0-1)
    // =============================

    [Range(0f, 1f)]
    public float explorationScore01;

    [Range(0f, 1f)]
    public float combatScore01;

    [Range(0f, 1f)]
    public float achieverScore01;
}

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance;

    [Header("Data")]
    public PlayerProfileData Data = new PlayerProfileData();

    [Header("Quest Generation")]
    [SerializeField] private QuestGenCoordinator questGenCodr;

    [Header("Runtime")]
    public string CurrentAreaId { get; private set; } = "";

    [Header("Recalculate Scores every N Seconds (0 = manual)")]
    [SerializeField] private float autoRecalcInterval = 1f;
    private float recalcTimer;

    [Header("Normalization Caps")]
    [Tooltip("How many unique areas exist in your prototype.")]
    public int capAreasVisited = 4;

    [Tooltip("How much total explore time in seconds counts as max.")]
    public float capTotalExploreTime = 600f;

    [Tooltip("How much distance counts as max.")]
    public float capDistance = 2000f;

    [Tooltip("How much damage dealt counts as max.")]
    public float capDamageDealt = 500f;

    [Tooltip("How much damage taken counts as max.")]
    public float capDamageTaken = 500f;

    [Tooltip("How many kills count as max.")]
    public int capKills = 20;

    [Tooltip("How many enemy aggro events count as max.")]
    public int capAggro = 30;

    [Tooltip("How many quests completed count as max.")]
    public int capQuestsCompleted = 10;

    [Tooltip("Average completion time in seconds where faster = better.")]
    public float capAvgQuestTime = 300f;

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

    private void Update()
    {
        if (questGenCodr != null)
        {
            questGenCodr.Tick(Data, Time.deltaTime);
        }

        if (autoRecalcInterval <= 0f) return;

        recalcTimer += Time.deltaTime;

        if (recalcTimer >= autoRecalcInterval)
        {
            recalcTimer = 0f;
            RecalculateAllScores();
        }
    }

    // =========================
    // EXPLORATION 
    // =========================

    public void SetCurrentArea(string areaID)
    {
        CurrentAreaId = areaID ?? "";

        if (!string.IsNullOrWhiteSpace(CurrentAreaId))
        {
            Data.areasVisited.Add(CurrentAreaId.Trim());
        }
    }

    public void AddTimerInCurrentArea(float dt)
    {
        if (string.IsNullOrWhiteSpace(CurrentAreaId)) return;
        if (dt <= 0f) return;

        if (!Data.timeInArea.ContainsKey(CurrentAreaId))
        {
            Data.timeInArea[CurrentAreaId] = 0f;
        }

        Data.timeInArea[CurrentAreaId] += dt;
    }

    public void AddDistance(float meters)
    {
        if (meters <= 0f) return;

        Data.distanceTravelled += meters;
    }

    // =========================
    // COMBAT 
    // =========================

    public void RecordAggro()
    {
        Data.enemiesAggroed++;
    }

    public void RecordKill()
    {
        Data.enemiesKilled++;
    }

    public void RecordEnemyAvoided()
    {
        Data.enemiesAvoided++;
    }

    public void RecordDamageTaken(float amount)
    {
        if (amount <= 0f) return;

        Data.damageTaken += amount;
    }

    public void RecordDamageDealt(float amount)
    {
        if (amount <= 0f) return;

        Data.damageDealt += amount;
    }

    public void RecordHit(AttackType attackType, WeaponType weaponType)
    {
        if (attackType == AttackType.Melee)
        {
            Data.meleeHits++;
        }
        else
        {
            Data.rangedHits++;
        }

        if (weaponType == WeaponType.Sword)
        {
            Data.swordHits++;
        }
        else if (weaponType == WeaponType.Blunt)
        {
            Data.bluntHits++;
        }
    }

    // =========================
    // QUEST / ACHIEVER 
    // ========================= 

    public void RecordQuestCompleted(
        string templateId,
        QuestType questType,
        string questScore,
        float completionTimeSec
    )
    {
        Data.questsCompleted++;
        Data.totalQuestCompletionTime += Mathf.Max(0f, completionTimeSec);

        if (!string.IsNullOrWhiteSpace(templateId))
        {
            templateId = templateId.Trim();

            if (!Data.completedByTemplateId.ContainsKey(templateId))
            {
                Data.completedByTemplateId[templateId] = 0;
            }

            Data.completedByTemplateId[templateId]++;
        }

        if (!Data.completedByQuestType.ContainsKey(questType))
        {
            Data.completedByQuestType[questType] = 0;
        }

        Data.completedByQuestType[questType]++;

        if (!string.IsNullOrWhiteSpace(questScore))
        {
            questScore = questScore.Trim().ToLowerInvariant();

            if (!Data.completedByQuestScore.ContainsKey(questScore))
            {
                Data.completedByQuestScore[questScore] = 0;
            }

            Data.completedByQuestScore[questScore]++;
        }

        Debug.Log(
            $"[PlayerProfile] Quest completed recorded. " +
            $"TemplateId='{templateId}', Type='{questType}', Score='{questScore}', Time={completionTimeSec:0.00}s"
        );
    }

    public void RecordQuestFailed()
    {
        Data.questsFailed++;
    }

    // =========================
    // SCORE CALCULATION
    // ========================= 

    public void RecalculateAllScores()
    {
        RecalculateExplorationScore();
        RecalculateCombatScore();
        RecalculateAchieverScore();
    }

    private void RecalculateExplorationScore()
    {
        float areas01 = Mathf.Clamp01((float)Data.areasVisited.Count / Mathf.Max(1, capAreasVisited));

        float totalExploreTime = 0f;

        foreach (var kv in Data.timeInArea)
        {
            totalExploreTime += kv.Value;
        }

        float time01 = Mathf.Clamp01(totalExploreTime / Mathf.Max(1f, capTotalExploreTime));
        float dist01 = Mathf.Clamp01(Data.distanceTravelled / Mathf.Max(1f, capDistance));

        Data.explorationScore01 = (areas01 + time01 + dist01) / 3f;
    }

    private void RecalculateCombatScore()
    {
        float dealt01 = Mathf.Clamp01(Data.damageDealt / Mathf.Max(1f, capDamageDealt));
        float taken01 = Mathf.Clamp01(Data.damageTaken / Mathf.Max(1f, capDamageTaken));
        float kills01 = Mathf.Clamp01((float)Data.enemiesKilled / Mathf.Max(1, capKills));
        float aggro01 = Mathf.Clamp01((float)Data.enemiesAggroed / Mathf.Max(1, capAggro));

        Data.combatScore01 = (dealt01 + kills01 + aggro01 + taken01) / 4f;
    }

    private void RecalculateAchieverScore()
    {
        float completed01 = Mathf.Clamp01((float)Data.questsCompleted / Mathf.Max(1, capQuestsCompleted));

        float avgTime = (Data.questsCompleted > 0)
            ? Data.totalQuestCompletionTime / Data.questsCompleted
            : capAvgQuestTime;

        float speed01 = 1f - Mathf.Clamp01(avgTime / Mathf.Max(1f, capAvgQuestTime));

        float failurePenalty01 = 0f;
        int totalOutcomes = Data.questsCompleted + Data.questsFailed;

        if (totalOutcomes > 0)
        {
            float failRate = (float)Data.questsFailed / totalOutcomes;
            failurePenalty01 = Mathf.Clamp01(failRate);
        }

        float reliability01 = 1f - failurePenalty01;

        Data.achieverScore01 = (completed01 + speed01 + reliability01) / 3f;
    }

    // =========================
    // DEBUG HELPERS
    // ========================= 

    public float GetTotalExploreTime()
    {
        float total = 0f;

        foreach (var kv in Data.timeInArea)
        {
            total += kv.Value;
        }

        return total;
    }

    public string GetTopCompletedTemplate()
    {
        string best = "";
        int bestCount = 0;

        foreach (var kv in Data.completedByTemplateId)
        {
            if (kv.Value > bestCount)
            {
                bestCount = kv.Value;
                best = kv.Key;
            }
        }

        return best;
    }
}