// using TMPro;
// using UnityEngine;

// public class QuestObjectiveUI : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] private QuestManager questManager;
//     [SerializeField] private Transform playerTransform;

//     [Header("UI")]
//     [SerializeField] private GameObject uiRoot;
//     [SerializeField] private TextMeshProUGUI objectiveText;
//     [SerializeField] private RectTransform arrowRect;

//     private QuestStepData _currentStep;
//     private Transform _currentTarget;

//     private void OnEnable()
//     {
//         if (questManager != null)
//         {
//             questManager.OnQuestStarted += HandleQuestStarted;
//             questManager.OnCurrentStepShown += HandleCurrentStepShown;
//             questManager.OnQuestEnded += HandleQuestEnded;
//         }
//     }

//     private void OnDisable()
//     {
//         if (questManager != null)
//         {
//             questManager.OnQuestStarted -= HandleQuestStarted;
//             questManager.OnCurrentStepShown -= HandleCurrentStepShown;
//             questManager.OnQuestEnded -= HandleQuestEnded;
//         }
//     }

//     private void Start()
//     {
//         if (uiRoot != null)
//             uiRoot.SetActive(false);

//         if (questManager != null && questManager.ActiveQuest != null)
//         {
//             HandleQuestStarted(questManager.ActiveQuest);
//             HandleCurrentStepShown(questManager.GetCurrentStep());
//         }
//     }

//     private void LateUpdate()
//     {
//         RefreshDynamicTargetIfNeeded();
//         UpdateArrow();
//     }

//     private void HandleQuestStarted(QuestData quest)
//     {
//         if (uiRoot != null)
//             uiRoot.SetActive(true);
//     }

//     private void HandleCurrentStepShown(QuestStepData step)
//     {
//         _currentStep = step;

//         if (objectiveText != null)
//             objectiveText.text = step != null ? step.text : "[QuestObjectiveUI.cs] No Step To Show :(";

//         _currentTarget = ResolveTarget(step);
//     }

//     private void HandleQuestEnded()
//     {
//         _currentStep = null;
//         _currentTarget = null;

//         if (objectiveText != null)
//             objectiveText.text = "";

//         if (uiRoot != null)
//             uiRoot.SetActive(false);
//     }

//     private void RefreshDynamicTargetIfNeeded()
//     {
//         if (_currentStep == null) return;

//         // Collect items can disappear as they are picked up,
//         // so refresh their target continuously.
//         if (_currentStep.type == "CollectItem")
//         {
//             _currentTarget = ResolveTarget(_currentStep);
//         }
//     }

//     private Transform ResolveTarget(QuestStepData step)
//     {
//         if (step == null || string.IsNullOrWhiteSpace(step.target))
//             return null;

//         if (step.type == "CollectItem")
//         {
//             if (QuestAssetSpawner.Instance != null && playerTransform != null)
//             {
//                 return QuestAssetSpawner.Instance.GetNearestSpawnedPickupTransform(
//                     step.target,
//                     playerTransform.position
//                 );
//             }

//             return null;
//         }

//         if (step.type == "EnterArea" ||
//             step.type == "ReturnToArea" ||
//             step.type == "TimeInArea" ||
//             step.type == "Interact")
//         {
//             if (QuestTargetRegistry.Instance != null)
//                 return QuestTargetRegistry.Instance.GetTargetTransform(step.target);
//         }

//         // For first version, hide arrow on kill steps.
//         return null;
//     }

//     private void UpdateArrow()
//     {
//         if (arrowRect == null)
//         {
//             //Debug.Log("[QuestObjectiveUI.cs] ArrowRect = null, returned");
//             return;
//         }   
        
//         // if(uiRoot == null) Debug.Log("1");
//         // if( uiRoot.activeSelf) Debug.Log("2");
//         // if(playerTransform == null) Debug.Log("3");
//         // if(_currentTarget == null) Debug.Log("4");

//         bool shouldShowArrow =
//             uiRoot != null &&
//             uiRoot.activeSelf &&
//             playerTransform != null &&
//             _currentTarget != null;

//         arrowRect.gameObject.SetActive(shouldShowArrow);

//         if (!shouldShowArrow)
//         {
//             //Debug.Log("[QuestObjectiveUI.cs] shouldShowArrow is false, returned");
//             return;
//         }

//         Vector2 dir = (Vector2)(_currentTarget.position - playerTransform.position);

//         if (dir.sqrMagnitude < 0.0001f)
//             return;

//         float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
//         arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
//     }
// }

using TMPro;
using UnityEngine;

public class QuestObjectiveUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Transform playerTransform;

    [Header("Objective UI")]
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private RectTransform arrowRect;

    [Header("Timer UI")]
    [SerializeField] private GameObject timerRoot;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Display Options")]
    [SerializeField] private bool showProgressForOtherSteps = false;

    private QuestStepData _currentStep;
    private Transform _currentTarget;

    private void OnEnable()
    {
        if (questManager != null)
        {
            questManager.OnQuestStarted += HandleQuestStarted;
            questManager.OnCurrentStepShown += HandleCurrentStepShown;
            questManager.OnQuestEnded += HandleQuestEnded;
        }
    }

    private void OnDisable()
    {
        if (questManager != null)
        {
            questManager.OnQuestStarted -= HandleQuestStarted;
            questManager.OnCurrentStepShown -= HandleCurrentStepShown;
            questManager.OnQuestEnded -= HandleQuestEnded;
        }
    }

    private void Start()
    {
        if (uiRoot != null)
            uiRoot.SetActive(false);

        HideTimerUI();

        if (questManager != null && questManager.ActiveQuest != null)
        {
            HandleQuestStarted(questManager.ActiveQuest);
            HandleCurrentStepShown(questManager.GetCurrentStep());
        }
    }

    private void LateUpdate()
    {
        RefreshCurrentStepReference();

        RefreshObjectiveText();
        RefreshTimerText();

        RefreshDynamicTargetIfNeeded();
        UpdateArrow();
    }

    private void HandleQuestStarted(QuestData quest)
    {
        if (uiRoot != null)
            uiRoot.SetActive(true);
    }

    private void HandleCurrentStepShown(QuestStepData step)
    {
        _currentStep = step;

        RefreshObjectiveText();
        RefreshTimerText();

        _currentTarget = ResolveTarget(step);
    }

    private void HandleQuestEnded()
    {
        _currentStep = null;
        _currentTarget = null;

        if (objectiveText != null)
            objectiveText.text = "";

        if (uiRoot != null)
            uiRoot.SetActive(false);

        if (arrowRect != null)
            arrowRect.gameObject.SetActive(false);

        HideTimerUI();
    }

    private void RefreshCurrentStepReference()
    {
        if (questManager == null) return;

        QuestStepData latestStep = questManager.GetCurrentStep();

        if (latestStep != _currentStep)
        {
            _currentStep = latestStep;
            _currentTarget = ResolveTarget(_currentStep);

            RefreshObjectiveText();
            RefreshTimerText();
        }
    }

    private void RefreshObjectiveText()
    {
        if (objectiveText == null)
            return;

        if (_currentStep == null)
        {
            objectiveText.text = "";
            return;
        }

        // Important:
        // TimeInArea no longer puts the timer inside the objective text.
        // The objective text only shows the normal step instruction.
        if (showProgressForOtherSteps && !IsStepType(_currentStep, "TimeInArea"))
        {
            objectiveText.text = $"{_currentStep.text}\n{_currentStep.progress}/{_currentStep.required}";
            return;
        }

        objectiveText.text = _currentStep.text;
    }

    private void RefreshTimerText()
    {
        if (_currentStep == null)
        {
            HideTimerUI();
            return;
        }

        if (!IsStepType(_currentStep, "TimeInArea"))
        {
            HideTimerUI();
            return;
        }

        ShowTimerUI();

        if (timerText != null)
        {
            timerText.text = BuildTimeInAreaText(_currentStep);
        }
    }

    private string BuildTimeInAreaText(QuestStepData step)
    {
        int requiredSeconds = Mathf.Max(0, step.required);
        int elapsedSeconds = Mathf.Clamp(step.progress, 0, requiredSeconds);
        int remainingSeconds = Mathf.Max(0, requiredSeconds - elapsedSeconds);

        return $"{FormatSeconds(remainingSeconds)}";
    }

    private string FormatSeconds(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (minutes > 0)
            return $"{minutes}:{seconds:00}";

        return $"{seconds:00}";
    }

    private void ShowTimerUI()
    {
        if (timerRoot != null && !timerRoot.activeSelf)
            timerRoot.SetActive(true);
    }

    private void HideTimerUI()
    {
        if (timerRoot != null && timerRoot.activeSelf)
            timerRoot.SetActive(false);

        if (timerText != null)
            timerText.text = "00";
    }

    private void RefreshDynamicTargetIfNeeded()
    {
        if (_currentStep == null) return;

        if (IsStepType(_currentStep, "CollectItem") ||
            IsStepType(_currentStep, "KillEnemy") ||
            IsStepType(_currentStep, "KillEnemyWithWeapon") ||
            IsStepType(_currentStep, "KillWithWeapon"))
        {
            _currentTarget = ResolveTarget(_currentStep);
        }
    }

    private Transform ResolveTarget(QuestStepData step)
    {
        if (step == null || string.IsNullOrWhiteSpace(step.target))
            return null;

        if (IsStepType(step, "CollectItem"))
        {
            if (QuestAssetSpawner.Instance != null && playerTransform != null)
            {
                return QuestAssetSpawner.Instance.GetNearestSpawnedPickupTransform(
                    step.target,
                    playerTransform.position
                );
            }

            return null;
        }

        if (IsStepType(step, "KillEnemy") ||
            IsStepType(step, "KillEnemyWithWeapon") ||
            IsStepType(step, "KillWithWeapon"))
        {
            if (QuestAssetSpawner.Instance != null && playerTransform != null)
            {
                return QuestAssetSpawner.Instance.GetNearestSpawnedPickupTransform(
                    step,
                    playerTransform.position
                );
            }

            return null;
        }

        if (IsStepType(step, "EnterArea") ||
            IsStepType(step, "ReturnToArea") ||
            IsStepType(step, "TimeInArea") ||
            IsStepType(step, "Interact"))
        {
            if (QuestTargetRegistry.Instance != null)
                return QuestTargetRegistry.Instance.GetTargetTransform(step.target);
        }

        return null;
    }

    private void UpdateArrow()
    {
        if (arrowRect == null)
            return;

        bool shouldShowArrow =
            uiRoot != null &&
            uiRoot.activeSelf &&
            playerTransform != null &&
            _currentTarget != null;

        arrowRect.gameObject.SetActive(shouldShowArrow);

        if (!shouldShowArrow)
            return;

        Vector2 dir = (Vector2)(_currentTarget.position - playerTransform.position);

        if (dir.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private bool IsStepType(QuestStepData step, string type)
    {
        if (step == null) return false;

        return string.Equals(
            step.type,
            type,
            System.StringComparison.OrdinalIgnoreCase
        );
    }
}