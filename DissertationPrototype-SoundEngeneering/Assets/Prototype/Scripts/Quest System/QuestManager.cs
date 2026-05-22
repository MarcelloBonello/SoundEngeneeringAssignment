// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public class QuestManager : MonoBehaviour
// {
//     public QuestData ActiveQuest { get; private set; }
//     public bool requestQuestGen = true;

//     public event Action<QuestData> OnQuestStarted;
//     public event Action<QuestStepData> OnCurrentStepShown;
//     public event Action OnQuestEnded;

//     private readonly HashSet<string> _activeAreas = new();
//     private float _currentStepAreaTimer = 0f;

//     private bool _questCompleted = false;
//     private int _currentStepIndex = 0;

//     [Header("UI")]
//     [SerializeField] private QuestGenerationUI questGenerationUI;

//     private const string EnemyRandomId = "ENEMY_RANDOM";
//     private const string EnemyMeleeId = "ENEMY_MELLE";
//     private const string EnemyRangedId = "ENEMY_RANGED";

//     private static readonly string[] RandomEnemyIds =
//     {
//         EnemyMeleeId,
//         EnemyRangedId
//     };

//     private void OnEnable()
//     {
//         GameEvents.OnAreaEntered += HandleAreaEntered;
//         GameEvents.OnAreaExited += HandleAreaExited;

//         GameEvents.OnEnemyKilled += HandleEnemyKilled;
//         GameEvents.OnItemCollected += HandleItemCollected;
//         GameEvents.OnInteracted += HandleInteracted;
//     }

//     private void OnDisable()
//     {
//         GameEvents.OnAreaEntered -= HandleAreaEntered;
//         GameEvents.OnAreaExited -= HandleAreaExited;

//         GameEvents.OnEnemyKilled -= HandleEnemyKilled;
//         GameEvents.OnItemCollected -= HandleItemCollected;
//         GameEvents.OnInteracted -= HandleInteracted;
//     }

//     private void Update()
//     {
//         if (ActiveQuest == null || _questCompleted) return;
//         if (!HasValidCurrentStep()) return;

//         QuestStepData step = ActiveQuest.steps[_currentStepIndex];

//         if (SameId(step.type, "TimeInArea") && ContainsActiveArea(step.target))
//         {
//             int before = step.progress;

//             _currentStepAreaTimer += Time.deltaTime;
//             step.progress = Mathf.Clamp(Mathf.FloorToInt(_currentStepAreaTimer), 0, step.required);

//             if (step.progress != before)
//             {
//                 Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
//             }

//             TryAdvanceStep();
//         }
//     }

//     public QuestStepData GetCurrentStep()
//     {
//         if (!HasValidCurrentStep()) return null;
//         return ActiveQuest.steps[_currentStepIndex];
//     }

//     public void StartQuest(QuestData quest)
//     {
//         if (quest == null)
//         {
//             Debug.LogError("[Quest] StartQuest failed: quest is null.");
//             return;
//         }

//         if (quest.steps == null || quest.steps.Length == 0)
//         {
//             Debug.LogError("[Quest] StartQuest failed: quest has no steps.");
//             return;
//         }

//         PrepareQuestTargets(quest);
//         RandomiseEnemyTargets(quest);

//         if (questGenerationUI != null)
//         {
//             questGenerationUI.turnOffAutoGenerationUI();
//             questGenerationUI.turnOffPreInstructions();
//             questGenerationUI.turnOffInstructions();
//         }
//         else
//         {
//             Debug.LogWarning("[QuestManager] QuestGenerationUI is not assigned.");
//         }

//         if (QuestAssetSpawner.Instance != null)
//         {
//             QuestAssetSpawner.Instance.ClearSpawnedPickups();
//         }

//         requestQuestGen = false;
//         ActiveQuest = quest;
//         _questCompleted = false;
//         _currentStepIndex = 0;
//         _activeAreas.Clear();
//         _currentStepAreaTimer = 0f;

//         Debug.Log($"[Quest] Started: {ActiveQuest.title} ({ActiveQuest.quest_id})");

//         OnQuestStarted?.Invoke(ActiveQuest);
//         ShowCurrentStep();
//     }

//     private void PrepareQuestTargets(QuestData quest)
//     {
//         if (quest == null || quest.steps == null) return;

//         foreach (QuestStepData step in quest.steps)
//         {
//             if (step == null) continue;

//             // Start every new quest from clean progress, even if the JSON came with a wrong value.
//             step.progress = Mathf.Max(0, step.progress);

//             if (string.IsNullOrWhiteSpace(step.type)) continue;
//             if (string.IsNullOrWhiteSpace(step.target)) continue;

//             step.type = step.type.Trim();
//             step.target = step.target.Trim();

//             if (SameId(step.type, "KillEnemy"))
//             {
//                 step.target = NormalizeId(step.target);
//             }
//             else if (SameId(step.type, "KillWithWeapon"))
//             {
//                 step.target = NormalizeId(step.target);
//             }
//             else if (SameId(step.type, "KillEnemyWithWeapon"))
//             {
//                 step.target = NormalizeEnemyWithWeaponTarget(step.target);
//             }
//             else
//             {
//                 // Keep normal area/item/interactable IDs readable, but remove accidental spaces.
//                 step.target = step.target.Trim();
//             }
//         }
//     }

//     private void RandomiseEnemyTargets(QuestData quest)
//     {
//         if (quest == null || quest.steps == null) return;

//         foreach (QuestStepData step in quest.steps)
//         {
//             if (step == null) continue;

//             if (SameId(step.type, "KillEnemy") && SameId(step.target, EnemyRandomId))
//             {
//                 string selectedEnemy = GetRandomEnemyId();
//                 step.target = selectedEnemy;

//                 if (SameId(selectedEnemy, EnemyMeleeId))
//                     step.text = $"Defeat {step.required} melee enemy/enemies.";
//                 else if (SameId(selectedEnemy, EnemyRangedId))
//                     step.text = $"Defeat {step.required} ranged enemy/enemies.";

//                 Debug.Log($"[Quest] Random enemy selected: {step.target}");
//             }

//             if (SameId(step.type, "KillEnemyWithWeapon") && !string.IsNullOrEmpty(step.target))
//             {
//                 string[] parts = step.target.Split(':');

//                 if (parts.Length == 2)
//                 {
//                     string enemyPart = NormalizeId(parts[0]);
//                     string weaponPart = NormalizeId(parts[1]);

//                     if (SameId(enemyPart, EnemyRandomId))
//                     {
//                         string selectedEnemy = GetRandomEnemyId();
//                         step.target = $"{selectedEnemy}:{weaponPart}";

//                         Debug.Log($"[Quest] Random enemy selected for weapon quest: {step.target}");
//                     }
//                 }
//             }
//         }
//     }

//     private string GetRandomEnemyId()
//     {
//         int index = UnityEngine.Random.Range(0, RandomEnemyIds.Length);
//         return RandomEnemyIds[index];
//     }

//     private void HandleAreaEntered(string areaId)
//     {
//         if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

//         if (!string.IsNullOrWhiteSpace(areaId))
//         {
//             _activeAreas.Add(areaId.Trim());
//         }

//         QuestStepData step = ActiveQuest.steps[_currentStepIndex];

//         bool isAreaStep =
//             SameId(step.type, "EnterArea") ||
//             SameId(step.type, "ReturnToArea") ||
//             SameId(step.type, "Interact");

//         if (isAreaStep && SameId(step.target, areaId))
//         {
//             step.progress = Mathf.Clamp(step.progress + 1, 0, step.required);
//             Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
//             TryAdvanceStep();
//         }

//         Debug.Log($"[Quest] Area entered: {areaId}");
//     }

//     private void HandleAreaExited(string areaId)
//     {
//         if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

//         RemoveActiveArea(areaId);

//         QuestStepData step = ActiveQuest.steps[_currentStepIndex];

//         if (SameId(step.type, "TimeInArea") && SameId(step.target, areaId) && !ContainsActiveArea(step.target))
//         {
//             _currentStepAreaTimer = 0f;
//         }

//         Debug.Log($"[Quest] Area exited: {areaId}");
//     }

//     private void HandleEnemyKilled(string enemyId, WeaponType weaponTypeUsed)
//     {
//         if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

//         QuestStepData step = ActiveQuest.steps[_currentStepIndex];
//         bool matched = false;

//         string weaponTypeText = weaponTypeUsed.ToString();

//         if (SameId(step.type, "KillEnemy") && SameId(step.target, enemyId))
//         {
//             matched = true;
//         }

//         if (SameId(step.type, "KillWithWeapon") && SameId(step.target, weaponTypeText))
//         {
//             matched = true;
//         }

//         if (SameId(step.type, "KillEnemyWithWeapon") && MatchesEnemyWithWeapon(step.target, enemyId, weaponTypeText))
//         {
//             matched = true;
//         }

//         if (matched)
//         {
//             step.progress = Mathf.Clamp(step.progress + 1, 0, step.required);
//             Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
//             TryAdvanceStep();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 $"[Quest] Enemy kill did not match current step. " +
//                 $"StepType='{step.type}', StepTarget='{step.target}', " +
//                 $"KilledEnemy='{enemyId}', WeaponUsed='{weaponTypeText}'"
//             );
//         }

//         Debug.Log($"[Quest] Enemy killed: {enemyId} using {weaponTypeUsed}");
//     }

//     private void HandleItemCollected(string itemId, int amount)
//     {
//         if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

//         QuestStepData step = ActiveQuest.steps[_currentStepIndex];

//         if (SameId(step.type, "CollectItem") && SameId(step.target, itemId))
//         {
//             step.progress = Mathf.Clamp(step.progress + amount, 0, step.required);
//             Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
//             TryAdvanceStep();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 $"[Quest] Item collection did not match current step. " +
//                 $"StepType='{step.type}', StepTarget='{step.target}', CollectedItem='{itemId}'"
//             );
//         }

//         Debug.Log($"[Quest] Item collected: {itemId} x{amount}");
//     }

//     private void HandleInteracted(string interactableId)
//     {
//         if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

//         QuestStepData step = ActiveQuest.steps[_currentStepIndex];

//         if (SameId(step.type, "Interact") && SameId(step.target, interactableId))
//         {
//             step.progress = Mathf.Clamp(step.progress + 1, 0, step.required);
//             Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
//             TryAdvanceStep();
//         }
//         else
//         {
//             Debug.LogWarning(
//                 $"[Quest] Interaction did not match current step. " +
//                 $"StepType='{step.type}', StepTarget='{step.target}', Interactable='{interactableId}'"
//             );
//         }

//         Debug.Log($"[Quest] Interacted with: {interactableId}");
//     }

//     private void TryAdvanceStep()
//     {
//         if (!HasValidCurrentStep()) return;

//         QuestStepData step = ActiveQuest.steps[_currentStepIndex];

//         if (step.progress < step.required) return;

//         Debug.Log($"[Quest] Step complete: {step.text}");

//         _currentStepIndex++;
//         _currentStepAreaTimer = 0f;

//         if (_currentStepIndex >= ActiveQuest.steps.Length)
//         {
//             CompleteQuest();
//             return;
//         }

//         ShowCurrentStep();
//     }

//     private void ShowCurrentStep()
//     {
//         if (!HasValidCurrentStep()) return;

//         QuestStepData step = ActiveQuest.steps[_currentStepIndex];
//         Debug.Log($"[Quest] Current objective: {step.text} ({step.progress}/{step.required})");

//         if (QuestAssetSpawner.Instance != null)
//         {
//             QuestAssetSpawner.Instance.ShowForStep(step);
//         }
//         else
//         {
//             Debug.LogWarning("[QuestManager] QuestAssetSpawner.Instance is null.");
//         }

//         OnCurrentStepShown?.Invoke(step);
//     }

//     private bool HasValidCurrentStep()
//     {
//         return ActiveQuest != null &&
//                ActiveQuest.steps != null &&
//                _currentStepIndex >= 0 &&
//                _currentStepIndex < ActiveQuest.steps.Length;
//     }

//     private bool SameId(string a, string b)
//     {
//         return string.Equals(NormalizeId(a), NormalizeId(b), StringComparison.OrdinalIgnoreCase);
//     }

//     private string NormalizeId(string id)
//     {
//         if (string.IsNullOrWhiteSpace(id))
//             return string.Empty;

//         id = id.Trim().ToUpperInvariant();

//         // Accept both spellings as the same enemy ID.
//         if (id == "ENEMY_MELEE")
//             return EnemyMeleeId;

//         // Your Unity enum uses Bow, but your quest rules use RANGED.
//         if (id == "BOW")
//             return "RANGED";

//         return id;
//     }

//     private string NormalizeEnemyWithWeaponTarget(string target)
//     {
//         if (string.IsNullOrWhiteSpace(target)) return string.Empty;

//         string[] parts = target.Split(':');

//         if (parts.Length != 2)
//             return NormalizeId(target);

//         string enemyPart = NormalizeId(parts[0]);
//         string weaponPart = NormalizeId(parts[1]);

//         return $"{enemyPart}:{weaponPart}";
//     }

//     private bool MatchesEnemyWithWeapon(string target, string enemyId, string weaponType)
//     {
//         if (string.IsNullOrEmpty(target)) return false;

//         string[] parts = target.Split(':');

//         if (parts.Length != 2)
//             return false;

//         string targetEnemy = parts[0];
//         string targetWeapon = parts[1];

//         return SameId(targetEnemy, enemyId) && SameId(targetWeapon, weaponType);
//     }

//     private bool ContainsActiveArea(string areaId)
//     {
//         foreach (string activeArea in _activeAreas)
//         {
//             if (SameId(activeArea, areaId))
//                 return true;
//         }

//         return false;
//     }

//     private void RemoveActiveArea(string areaId)
//     {
//         if (string.IsNullOrWhiteSpace(areaId)) return;

//         string areaToRemove = null;

//         foreach (string activeArea in _activeAreas)
//         {
//             if (SameId(activeArea, areaId))
//             {
//                 areaToRemove = activeArea;
//                 break;
//             }
//         }

//         if (areaToRemove != null)
//         {
//             _activeAreas.Remove(areaToRemove);
//         }
//     }

//     private void CompleteQuest()
//     {
//         if (_questCompleted) return;

//         _questCompleted = true;
//         requestQuestGen = false;

//         PlayerProfile.Instance?.RecordQuestCompleted();

//         if (questGenerationUI != null)
//         {
//             questGenerationUI.turnOnInstructions();
//         }

//         if (QuestAssetSpawner.Instance != null)
//         {
//             QuestAssetSpawner.Instance.ClearSpawnedPickups();
//         }

//         if (QuestGenRuntime.Instance != null)
//         {
//             QuestGenRuntime.Instance.MarkQuestCompleted();
//         }
//         else
//         {
//             Debug.LogWarning("[QuestManager.cs] QuestGenRuntime.Instance is null.");
//         }

//         Debug.Log($"[Quest] Completed: {ActiveQuest.title}");
//         OnQuestEnded?.Invoke();
//     }
// }

using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public QuestData ActiveQuest { get; private set; }
    public bool requestQuestGen = true;

    public event Action<QuestData> OnQuestStarted;
    public event Action<QuestStepData> OnCurrentStepShown;
    public event Action OnQuestEnded;

    private readonly HashSet<string> _activeAreas = new();
    private float _currentStepAreaTimer = 0f;

    private bool _questCompleted = false;
    private int _currentStepIndex = 0;
    private float _questStartTime = 0f;

    [Header("UI")]
    [SerializeField] private QuestGenerationUI questGenerationUI;

    private const string EnemyRandomId = "ENEMY_RANDOM";
    private const string EnemyMeleeId = "ENEMY_MELLE";
    private const string EnemyRangedId = "ENEMY_RANGED";

    private static readonly string[] RandomEnemyIds =
    {
        EnemyMeleeId,
        EnemyRangedId
    };

    private void OnEnable()
    {
        GameEvents.OnAreaEntered += HandleAreaEntered;
        GameEvents.OnAreaExited += HandleAreaExited;

        GameEvents.OnEnemyKilled += HandleEnemyKilled;
        GameEvents.OnItemCollected += HandleItemCollected;
        GameEvents.OnInteracted += HandleInteracted;
    }

    private void OnDisable()
    {
        GameEvents.OnAreaEntered -= HandleAreaEntered;
        GameEvents.OnAreaExited -= HandleAreaExited;

        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        GameEvents.OnItemCollected -= HandleItemCollected;
        GameEvents.OnInteracted -= HandleInteracted;
    }

    private void Update()
    {
        if (ActiveQuest == null || _questCompleted) return;
        if (!HasValidCurrentStep()) return;

        QuestStepData step = ActiveQuest.steps[_currentStepIndex];

        if (SameId(step.type, "TimeInArea") && ContainsActiveArea(step.target))
        {
            int before = step.progress;

            _currentStepAreaTimer += Time.deltaTime;
            step.progress = Mathf.Clamp(Mathf.FloorToInt(_currentStepAreaTimer), 0, step.required);

            if (step.progress != before)
            {
                Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
            }

            TryAdvanceStep();
        }
    }

    public QuestStepData GetCurrentStep()
    {
        if (!HasValidCurrentStep()) return null;
        return ActiveQuest.steps[_currentStepIndex];
    }

    public void StartQuest(QuestData quest)
    {
        if (quest == null)
        {
            Debug.LogError("[Quest] StartQuest failed: quest is null.");
            return;
        }

        if (quest.steps == null || quest.steps.Length == 0)
        {
            Debug.LogError("[Quest] StartQuest failed: quest has no steps.");
            return;
        }

        PrepareQuestTargets(quest);
        RandomiseEnemyTargets(quest);

        quest.runtimeQuestType = GetQuestTypeForProfile(quest);

        if (questGenerationUI != null)
        {
            questGenerationUI.turnOffAutoGenerationUI();
            questGenerationUI.turnOffPreInstructions();
            questGenerationUI.turnOffInstructions();
        }
        else
        {
            Debug.LogWarning("[QuestManager] QuestGenerationUI is not assigned.");
        }

        if (QuestAssetSpawner.Instance != null)
        {
            QuestAssetSpawner.Instance.ClearSpawnedPickups();
        }

        requestQuestGen = false;
        ActiveQuest = quest;
        _questCompleted = false;
        _currentStepIndex = 0;
        _activeAreas.Clear();
        _currentStepAreaTimer = 0f;
        _questStartTime = Time.time;

        Debug.Log($"[Quest] Started: {ActiveQuest.title} ({ActiveQuest.quest_id})");
        Debug.Log($"[Quest] Type detected: {ActiveQuest.runtimeQuestType}");

        OnQuestStarted?.Invoke(ActiveQuest);
        ShowCurrentStep();
    }

    private void PrepareQuestTargets(QuestData quest)
    {
        if (quest == null || quest.steps == null) return;

        foreach (QuestStepData step in quest.steps)
        {
            if (step == null) continue;

            // For generated runtime quests, always start from zero.
            // This protects you if the LLM accidentally returns progress > 0.
            step.progress = 0;

            if (string.IsNullOrWhiteSpace(step.type)) continue;
            if (string.IsNullOrWhiteSpace(step.target)) continue;

            step.type = step.type.Trim();
            step.target = step.target.Trim();

            if (SameId(step.type, "KillEnemy"))
            {
                step.target = NormalizeId(step.target);
            }
            else if (SameId(step.type, "KillWithWeapon"))
            {
                step.target = NormalizeId(step.target);
            }
            else if (SameId(step.type, "KillEnemyWithWeapon"))
            {
                step.target = NormalizeEnemyWithWeaponTarget(step.target);
            }
            else
            {
                step.target = step.target.Trim();
            }
        }
    }

    private void RandomiseEnemyTargets(QuestData quest)
    {
        if (quest == null || quest.steps == null) return;

        foreach (QuestStepData step in quest.steps)
        {
            if (step == null) continue;

            if (SameId(step.type, "KillEnemy") && SameId(step.target, EnemyRandomId))
            {
                string selectedEnemy = GetRandomEnemyId();
                step.target = selectedEnemy;

                if (SameId(selectedEnemy, EnemyMeleeId))
                {
                    step.text = $"Defeat {step.required} melee enemy/enemies.";
                }
                else if (SameId(selectedEnemy, EnemyRangedId))
                {
                    step.text = $"Defeat {step.required} ranged enemy/enemies.";
                }

                Debug.Log($"[Quest] Random enemy selected: {step.target}");
            }

            if (SameId(step.type, "KillEnemyWithWeapon") && !string.IsNullOrEmpty(step.target))
            {
                string[] parts = step.target.Split(':');

                if (parts.Length == 2)
                {
                    string enemyPart = NormalizeId(parts[0]);
                    string weaponPart = NormalizeId(parts[1]);

                    if (SameId(enemyPart, EnemyRandomId))
                    {
                        string selectedEnemy = GetRandomEnemyId();
                        step.target = $"{selectedEnemy}:{weaponPart}";

                        Debug.Log($"[Quest] Random enemy selected for weapon quest: {step.target}");
                    }
                }
            }
        }
    }

    private string GetRandomEnemyId()
    {
        int index = UnityEngine.Random.Range(0, RandomEnemyIds.Length);
        return RandomEnemyIds[index];
    }

    private void HandleAreaEntered(string areaId)
    {
        if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

        if (!string.IsNullOrWhiteSpace(areaId))
        {
            _activeAreas.Add(areaId.Trim());
        }

        QuestStepData step = ActiveQuest.steps[_currentStepIndex];

        bool isAreaStep =
            SameId(step.type, "EnterArea") ||
            SameId(step.type, "ReturnToArea") ||
            SameId(step.type, "Interact");

        if (isAreaStep && SameId(step.target, areaId))
        {
            step.progress = Mathf.Clamp(step.progress + 1, 0, step.required);
            Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
            TryAdvanceStep();
        }

        Debug.Log($"[Quest] Area entered: {areaId}");
    }

    private void HandleAreaExited(string areaId)
    {
        if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

        RemoveActiveArea(areaId);

        QuestStepData step = ActiveQuest.steps[_currentStepIndex];

        if (SameId(step.type, "TimeInArea") && SameId(step.target, areaId) && !ContainsActiveArea(step.target))
        {
            _currentStepAreaTimer = 0f;
        }

        Debug.Log($"[Quest] Area exited: {areaId}");
    }

    private void HandleEnemyKilled(string enemyId, WeaponType weaponTypeUsed)
    {
        if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

        QuestStepData step = ActiveQuest.steps[_currentStepIndex];
        bool matched = false;

        string weaponTypeText = weaponTypeUsed.ToString();

        if (SameId(step.type, "KillEnemy") && SameId(step.target, enemyId))
        {
            matched = true;
        }

        if (SameId(step.type, "KillWithWeapon") && SameId(step.target, weaponTypeText))
        {
            matched = true;
        }

        if (SameId(step.type, "KillEnemyWithWeapon") && MatchesEnemyWithWeapon(step.target, enemyId, weaponTypeText))
        {
            matched = true;
        }

        if (matched)
        {
            step.progress = Mathf.Clamp(step.progress + 1, 0, step.required);
            Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
            TryAdvanceStep();
        }
        else
        {
            Debug.LogWarning(
                $"[Quest] Enemy kill did not match current step. " +
                $"StepType='{step.type}', StepTarget='{step.target}', " +
                $"KilledEnemy='{enemyId}', WeaponUsed='{weaponTypeText}'"
            );
        }

        Debug.Log($"[Quest] Enemy killed: {enemyId} using {weaponTypeUsed}");
    }

    private void HandleItemCollected(string itemId, int amount)
    {
        if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

        QuestStepData step = ActiveQuest.steps[_currentStepIndex];

        if (SameId(step.type, "CollectItem") && SameId(step.target, itemId))
        {
            step.progress = Mathf.Clamp(step.progress + amount, 0, step.required);
            Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
            TryAdvanceStep();
        }
        else
        {
            Debug.LogWarning(
                $"[Quest] Item collection did not match current step. " +
                $"StepType='{step.type}', StepTarget='{step.target}', CollectedItem='{itemId}'"
            );
        }

        Debug.Log($"[Quest] Item collected: {itemId} x{amount}");
    }

    private void HandleInteracted(string interactableId)
    {
        if (ActiveQuest == null || _questCompleted || !HasValidCurrentStep()) return;

        QuestStepData step = ActiveQuest.steps[_currentStepIndex];

        if (SameId(step.type, "Interact") && SameId(step.target, interactableId))
        {
            step.progress = Mathf.Clamp(step.progress + 1, 0, step.required);
            Debug.Log($"[Quest] Current step progress: {step.progress}/{step.required} ({step.text})");
            TryAdvanceStep();
        }
        else
        {
            Debug.LogWarning(
                $"[Quest] Interaction did not match current step. " +
                $"StepType='{step.type}', StepTarget='{step.target}', Interactable='{interactableId}'"
            );
        }

        Debug.Log($"[Quest] Interacted with: {interactableId}");
    }

    private void TryAdvanceStep()
    {
        if (!HasValidCurrentStep()) return;

        QuestStepData step = ActiveQuest.steps[_currentStepIndex];

        if (step.progress < step.required) return;

        Debug.Log($"[Quest] Step complete: {step.text}");

        _currentStepIndex++;
        _currentStepAreaTimer = 0f;

        if (_currentStepIndex >= ActiveQuest.steps.Length)
        {
            CompleteQuest();
            return;
        }

        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (!HasValidCurrentStep()) return;

        QuestStepData step = ActiveQuest.steps[_currentStepIndex];
        Debug.Log($"[Quest] Current objective: {step.text} ({step.progress}/{step.required})");

        if (QuestAssetSpawner.Instance != null)
        {
            QuestAssetSpawner.Instance.ShowForStep(step);
        }
        else
        {
            Debug.LogWarning("[QuestManager] QuestAssetSpawner.Instance is null.");
        }

        OnCurrentStepShown?.Invoke(step);
    }

    private bool HasValidCurrentStep()
    {
        return ActiveQuest != null &&
               ActiveQuest.steps != null &&
               _currentStepIndex >= 0 &&
               _currentStepIndex < ActiveQuest.steps.Length;
    }

    private bool SameId(string a, string b)
    {
        return string.Equals(NormalizeId(a), NormalizeId(b), StringComparison.OrdinalIgnoreCase);
    }

    private string NormalizeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        id = id.Trim().ToUpperInvariant();

        // Accept both spellings as the same enemy ID.
        if (id == "ENEMY_MELEE")
        {
            return EnemyMeleeId;
        }

        // Your quest rules use RANGED, but your Unity weapon may say Bow.
        if (id == "BOW")
        {
            return "RANGED";
        }

        return id;
    }

    private string NormalizeEnemyWithWeaponTarget(string target)
    {
        if (string.IsNullOrWhiteSpace(target)) return string.Empty;

        string[] parts = target.Split(':');

        if (parts.Length != 2)
        {
            return NormalizeId(target);
        }

        string enemyPart = NormalizeId(parts[0]);
        string weaponPart = NormalizeId(parts[1]);

        return $"{enemyPart}:{weaponPart}";
    }

    private bool MatchesEnemyWithWeapon(string target, string enemyId, string weaponType)
    {
        if (string.IsNullOrEmpty(target)) return false;

        string[] parts = target.Split(':');

        if (parts.Length != 2)
        {
            return false;
        }

        string targetEnemy = parts[0];
        string targetWeapon = parts[1];

        return SameId(targetEnemy, enemyId) && SameId(targetWeapon, weaponType);
    }

    private bool ContainsActiveArea(string areaId)
    {
        foreach (string activeArea in _activeAreas)
        {
            if (SameId(activeArea, areaId))
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveActiveArea(string areaId)
    {
        if (string.IsNullOrWhiteSpace(areaId)) return;

        string areaToRemove = null;

        foreach (string activeArea in _activeAreas)
        {
            if (SameId(activeArea, areaId))
            {
                areaToRemove = activeArea;
                break;
            }
        }

        if (areaToRemove != null)
        {
            _activeAreas.Remove(areaToRemove);
        }
    }

    private QuestType GetQuestTypeForProfile(QuestData quest)
    {
        if (quest == null)
        {
            return QuestType.Unknown;
        }

        QuestType explicitType = ParseQuestType(quest.quest_type);

        if (explicitType != QuestType.Unknown)
        {
            return explicitType;
        }

        bool hasCombat = false;
        bool hasExploration = false;

        if (quest.tags != null)
        {
            foreach (string tag in quest.tags)
            {
                if (SameId(tag, "combat"))
                {
                    hasCombat = true;
                }

                if (SameId(tag, "exploration") || SameId(tag, "explore"))
                {
                    hasExploration = true;
                }
            }
        }

        if (quest.steps != null)
        {
            foreach (QuestStepData step in quest.steps)
            {
                if (step == null) continue;

                if (IsCombatStep(step.type))
                {
                    hasCombat = true;
                }

                if (IsExplorationStep(step.type))
                {
                    hasExploration = true;
                }
            }
        }

        if (hasCombat && hasExploration)
        {
            return QuestType.Mixed;
        }

        if (hasCombat)
        {
            return QuestType.Combat;
        }

        if (hasExploration)
        {
            return QuestType.Exploration;
        }

        return QuestType.Unknown;
    }

    private QuestType ParseQuestType(string questTypeText)
    {
        if (string.IsNullOrWhiteSpace(questTypeText))
        {
            return QuestType.Unknown;
        }

        questTypeText = questTypeText.Trim();

        if (string.Equals(questTypeText, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return QuestType.Combat;
        }

        if (string.Equals(questTypeText, "exploration", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(questTypeText, "explore", StringComparison.OrdinalIgnoreCase))
        {
            return QuestType.Exploration;
        }

        if (string.Equals(questTypeText, "mixed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(questTypeText, "hybrid", StringComparison.OrdinalIgnoreCase))
        {
            return QuestType.Mixed;
        }

        return QuestType.Unknown;
    }

    private bool IsCombatStep(string stepType)
    {
        return SameId(stepType, "KillEnemy") ||
               SameId(stepType, "KillWithWeapon") ||
               SameId(stepType, "KillEnemyWithWeapon");
    }

    private bool IsExplorationStep(string stepType)
    {
        return SameId(stepType, "EnterArea") ||
               SameId(stepType, "ReturnToArea") ||
               SameId(stepType, "TimeInArea") ||
               SameId(stepType, "CollectItem") ||
               SameId(stepType, "Interact");
    }

    private string GetQuestScoreForProfile(QuestType questType)
    {
        switch (questType)
        {
            case QuestType.Combat:
                return "combat";

            case QuestType.Exploration:
                return "exploration";

            case QuestType.Mixed:
                return "mixed";

            default:
                return "unknown";
        }
    }

    private void CompleteQuest()
    {
        if (_questCompleted) return;

        _questCompleted = true;
        requestQuestGen = false;

        string templateId = ActiveQuest != null ? ActiveQuest.quest_id : "";
        QuestType questType = ActiveQuest != null ? ActiveQuest.runtimeQuestType : QuestType.Unknown;
        string questScore = GetQuestScoreForProfile(questType);
        float completionTimeSec = Mathf.Max(0f, Time.time - _questStartTime);

        PlayerProfile.Instance?.RecordQuestCompleted(
            templateId,
            questType,
            questScore,
            completionTimeSec
        );

        Debug.Log(
            $"[Quest] Profile recorded. " +
            $"TemplateId='{templateId}', Type='{questType}', Score='{questScore}', Time={completionTimeSec:0.00}s"
        );

        if (questGenerationUI != null)
        {
            questGenerationUI.turnOnInstructions();
        }

        if (QuestAssetSpawner.Instance != null)
        {
            QuestAssetSpawner.Instance.ClearSpawnedPickups();
        }

        if (QuestGenRuntime.Instance != null)
        {
            QuestGenRuntime.Instance.MarkQuestCompleted();
        }
        else
        {
            Debug.LogWarning("[QuestManager.cs] QuestGenRuntime.Instance is null.");
        }

        if (ActiveQuest != null)
        {
            Debug.Log($"[Quest] Completed: {ActiveQuest.title}");
        }
        else
        {
            Debug.Log("[Quest] Completed.");
        }

        OnQuestEnded?.Invoke();
    }
}