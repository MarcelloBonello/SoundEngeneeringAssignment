using UnityEngine;

public class QuestGenCoordinator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestApiClient apiClient;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestGenerationUI questGenerationUI;

    [Header("Auto Generation Timing")]
    [SerializeField] private float firstQuestGenerateAfterSeconds = 60f;
    [SerializeField] private float nextQuestGenerateAfterSeconds = 60f;

    [Header("Safety")]
    [SerializeField] private float questResponseTimeoutSeconds = 30f;

    private float timer = 0f;
    private float responseTimer = 0f;

    private bool waitingForQuestResponse = false;
    private bool waitingAfterQuestCompletion = false;
    private bool questIsActive = false;

    private void Start()
    {
        questIsActive = false;
        waitingForQuestResponse = false;
        waitingAfterQuestCompletion = false;
        timer = 0f;
        responseTimer = 0f;

        ShowNoQuestInstructions();

        Debug.Log($"[QuestGenCoordinator] Started. First quest will auto-generate after {firstQuestGenerateAfterSeconds} seconds.");
    }

    private void OnEnable()
    {
        if (questManager != null)
        {
            questManager.OnQuestStarted += HandleQuestStarted;
            questManager.OnQuestEnded += HandleQuestEnded;
        }
        else
        {
            Debug.LogWarning("[QuestGenCoordinator] QuestManager is not assigned.");
        }
    }

    private void OnDisable()
    {
        if (questManager != null)
        {
            questManager.OnQuestStarted -= HandleQuestStarted;
            questManager.OnQuestEnded -= HandleQuestEnded;
        }
    }

    public void Tick(PlayerProfileData data, float deltaTime)
    {
        if (questManager == null)
            return;

        if (waitingForQuestResponse)
        {
            TickQuestResponseTimeout(deltaTime);
            return;
        }

        if (questIsActive)
            return;

        timer += deltaTime;

        float targetTime = waitingAfterQuestCompletion
            ? nextQuestGenerateAfterSeconds
            : firstQuestGenerateAfterSeconds;

        if (timer >= targetTime)
        {
            RequestQuestNow(data);
        }
    }

    private void TickQuestResponseTimeout(float deltaTime)
    {
        responseTimer += deltaTime;

        if (responseTimer < questResponseTimeoutSeconds)
            return;

        Debug.LogWarning("[QuestGenCoordinator] Quest request timed out. Returning to no-quest instructions.");

        waitingForQuestResponse = false;
        responseTimer = 0f;
        timer = 0f;

        if (questManager != null)
        {
            questManager.requestQuestGen = true;
        }

        ShowNoQuestInstructions();
    }

    private void RequestQuestNow(PlayerProfileData data)
    {
        if (apiClient == null)
        {
            Debug.LogError("[QuestGenCoordinator] QuestApiClient is not assigned.");
            return;
        }

        if (waitingForQuestResponse)
            return;

        waitingForQuestResponse = true;
        waitingAfterQuestCompletion = false;
        questIsActive = false;

        timer = 0f;
        responseTimer = 0f;

        ShowGeneratingQuestUI();

        PlayerProfileSave.Save(data);
        Debug.Log("[QuestGenCoordinator] Saved player profile before quest request.");

        if (questManager != null)
        {
            questManager.requestQuestGen = false;
        }

        apiClient.RequestQuestFromCurrentProfile();

        Debug.Log("[QuestGenCoordinator] Requesting quest. AutoGen UI should now be visible until the quest starts.");
    }

    private void HandleQuestStarted(QuestData quest)
    {
        waitingForQuestResponse = false;
        waitingAfterQuestCompletion = false;
        questIsActive = true;

        timer = 0f;
        responseTimer = 0f;

        HideGenerationUI();

        Debug.Log("[QuestGenCoordinator] Quest started. Generation UI hidden.");
    }

    private void HandleQuestEnded()
    {
        waitingForQuestResponse = false;
        waitingAfterQuestCompletion = true;
        questIsActive = false;

        timer = 0f;
        responseTimer = 0f;

        if (questManager != null)
        {
            questManager.requestQuestGen = true;
        }

        ShowNoQuestInstructions();

        Debug.Log($"[QuestGenCoordinator] Quest complete. Instructions shown. Next quest will auto-generate after {nextQuestGenerateAfterSeconds} seconds.");
    }

    private void ShowNoQuestInstructions()
    {
        if (questGenerationUI == null)
        {
            Debug.LogWarning("[QuestGenCoordinator] QuestGenerationUI is not assigned.");
            return;
        }

        questGenerationUI.turnOffAutoGenerationUI();
        questGenerationUI.turnOffPreInstructions();

        questGenerationUI.turnOnInstructions();
    }

    private void ShowGeneratingQuestUI()
    {
        if (questGenerationUI == null)
        {
            Debug.LogWarning("[QuestGenCoordinator] QuestGenerationUI is not assigned.");
            return;
        }

        questGenerationUI.turnOffInstructions();
        questGenerationUI.turnOffPreInstructions();

        questGenerationUI.turnOnAutoGenerationUI();
    }

    private void HideGenerationUI()
    {
        if (questGenerationUI == null)
            return;

        questGenerationUI.turnOffInstructions();
        questGenerationUI.turnOffPreInstructions();
        questGenerationUI.turnOffAutoGenerationUI();
    }
}