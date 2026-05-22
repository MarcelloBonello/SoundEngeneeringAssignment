using System.Collections;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class QuestApiClient : MonoBehaviour
{
    [Header("Gemini")]
    [SerializeField] private string apiKey = "";
    [SerializeField] private string modelName = "";
    [SerializeField] private string endpointBase = "https://generativelanguage.googleapis.com/v1beta/models";

    [Header("Prompt Files")]
    [SerializeField] private TextAsset gameScopeJsonAsset;
    [SerializeField] private TextAsset questRulesJsonAsset;

    private const string fileUploadEndpoint = "https://generativelanguage.googleapis.com/upload/v1beta/files";

    [Header("Quest")]
    [SerializeField] private QuestManager questManager;

    private bool _requestInFlight = false;

    [SerializeField] private QuestGenerationUI questGenerationUI;

    public void RequestQuestFromCurrentProfile()
    {
        //checks
        if (_requestInFlight)
        {
            Debug.Log("[QuestApiClient] Request already in flight. Skipping duplicate request.");
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.LogError("[QuestApiClient] Gemini API key is empty.");
            return;
        }

        if (PlayerProfile.Instance == null)
        {
            Debug.LogError("[QuestApiClient] PlayerProfile.Instance is null.");
            return;
        }

        if (questManager == null)
        {
            Debug.LogError("[QuestApiClient] QuestManager is not assigned.");
            return;
        }

        //start
        StartCoroutine(SendRequestCoroutine(PlayerProfile.Instance.Data));
    }

    private IEnumerator SendRequestCoroutine(PlayerProfileData profileData)
    {
        _requestInFlight = true;

        if (gameScopeJsonAsset == null)
        {
            Debug.LogError("[QuestApiClient] gameScopeJsonAsset is not assigned.");
            _requestInFlight = false;
            yield break;
        }

        if (questRulesJsonAsset == null)
        {
            Debug.LogError("[QuestApiClient] questRulesJsonAsset is not assigned.");
            _requestInFlight = false;
            yield break;
        }

        if (string.IsNullOrWhiteSpace(modelName))
        {
            Debug.LogError("[QuestApiClient] Gemini model name is empty.");
            _requestInFlight = false;
            yield break;
        }

        string playerProfileJson = JsonConvert.SerializeObject(profileData, Formatting.Indented);

         Debug.Log("[QuestApiClient.cs] PlayerProfile: " + playerProfileJson);

        byte[] playerProfileBytes = Encoding.UTF8.GetBytes(playerProfileJson);
        byte[] gameScopeBytes = gameScopeJsonAsset.bytes;
        byte[] questRulesBytes = questRulesJsonAsset.bytes;

        string playerProfileUri = null;
        string gameScopeUri = null;
        string questRulesUri = null;

        yield return StartCoroutine(UploadJsonBytesAsGeminiFile(
            "PlayerProfile.json",
            playerProfileBytes,
            uri => playerProfileUri = uri));

        if (string.IsNullOrWhiteSpace(playerProfileUri))
        {
            _requestInFlight = false;
            yield break;
        }

        yield return StartCoroutine(UploadJsonBytesAsGeminiFile(
            "GameScope.json",
            gameScopeBytes,
            uri => gameScopeUri = uri));

        if (string.IsNullOrWhiteSpace(gameScopeUri))
        {
            _requestInFlight = false;
            yield break;
        }

        yield return StartCoroutine(UploadJsonBytesAsGeminiFile(
            "QuestRules.json",
            questRulesBytes,
            uri => questRulesUri = uri));

        if (string.IsNullOrWhiteSpace(questRulesUri))
        {
            _requestInFlight = false;
            yield break;
        }

        var payload = new
        {
            systemInstruction = new
            {
                parts = new[]
                {
                    new
                    {
                        text = BuildSystemInstruction()
                    }
                }
            },
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            file_data = new
                            {
                                mime_type = "application/json",
                                file_uri = playerProfileUri
                            }
                        },
                        new
                        {
                            file_data = new
                            {
                                mime_type = "application/json",
                                file_uri = gameScopeUri
                            }
                        },
                        new
                        {
                            file_data = new
                            {
                                mime_type = "application/json",
                                file_uri = questRulesUri
                            }
                        },
                        new
                        {
                            text =
                                "Use the attached JSON files to generate exactly one quest JSON object. " +
                                "Return only JSON. Follow the required field names exactly."
                        }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseJsonSchema = BuildQuestJsonSchema(),
                candidateCount = 1,
                maxOutputTokens = 8192,
                temperature = 0.2,

                thinkingConfig = new
                {
                    thinkingLevel = "minimal"
                }
            }
        };

        string requestJson = JsonConvert.SerializeObject(payload, Formatting.Indented);
        string url = $"{endpointBase}/{modelName}:generateContent";

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-goog-api-key", apiKey);

        Debug.Log("[QuestApiClient] Sending Gemini request with uploaded files...");
        Debug.Log(requestJson);

        yield return request.SendWebRequest();

        _requestInFlight = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            if(request.responseCode == 503)
            {
                questGenerationUI.turnOnErrorWarning("503");
            }
            else if(request.responseCode == 429)
            {
                questGenerationUI.turnOnErrorWarning("429");
            }

            Debug.LogError($"[QuestApiClient] Gemini request failed: {request.result}");
            Debug.LogError($"[QuestApiClient] HTTP code: {request.responseCode}");
            Debug.LogError($"[QuestApiClient] Error: {request.error}");
            Debug.LogError($"[QuestApiClient] Body: {request.downloadHandler.text}");
            yield break;
        }

        questGenerationUI.turnOffErrorWarning();

        string rawResponse = request.downloadHandler.text;
        Debug.Log("[QuestApiClient] Raw Gemini response:");
        Debug.Log(rawResponse);

        string questJson = ExtractQuestJson(rawResponse);

        if (string.IsNullOrWhiteSpace(questJson))
        {
            Debug.LogError("[QuestApiClient] Could not extract quest JSON from Gemini response.");
            yield break;
        }

        questJson = StripCodeFences(questJson).Trim();

        Debug.Log("[QuestApiClient] Extracted quest JSON:");
        Debug.Log(questJson);

        QuestData quest = null;

        try
        {
            quest = JsonConvert.DeserializeObject<QuestData>(questJson);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QuestApiClient] Failed to deserialize QuestData.\n{ex}");
            yield break;
        }

        if (!ValidateQuest(quest))
        {
            Debug.LogError("[QuestApiClient] Gemini returned invalid quest data.");
            yield break;
        }

        QuestGenRecord record = new QuestGenRecord
        {
            schema_version = "1.0",
            generation_id = System.Guid.NewGuid().ToString("N"),
            timestamp_utc = System.DateTime.UtcNow.ToString("o"),

            condition = new GenerationCondition
            {
                profile_conditioned = true,
                generator_mode = "gemini_direct_files",
                model = modelName,
                notes = "Unity -> Gemini Files API -> generateContent"
            },

            trigger_context = new TriggerContext
            {
                trigger_type = "time_then_completion_gate",
                trigger_time_seconds = 2f,
                quest_request_allowed = questManager.requestQuestGen,
                active_scene = SceneManager.GetActiveScene().name
            },

            input_snapshot = new GenerationInputSnapshot
            {
                player_profile = PlayerProfile.Instance.Data,
                game_scope_file = gameScopeJsonAsset != null ? gameScopeJsonAsset.name : null,
                quest_rules_file = questRulesJsonAsset != null ? questRulesJsonAsset.name : null
            },

            generated_quest = quest,

            outcome = new GenerationOutcome
            {
                status = "generated",
                started_in_game = false,
                completed = false,
                failed = false,
                completion_time_seconds = 0f,
                completion_timestamp_utc = null
            }
        };

        if (QuestGenRuntime.Instance != null)
            QuestGenRuntime.Instance.BeginRecord(record);
        else
            Debug.LogWarning("[QuestApiClient] QuestGenRuntime.Instance is null.");

        questManager.StartQuest(quest);

        if (QuestGenRuntime.Instance != null)
            QuestGenRuntime.Instance.MarkQuestStarted();
    }

    private IEnumerator UploadJsonBytesAsGeminiFile(string displayName, byte[] fileBytes, System.Action<string> onDone)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.LogError("[QuestApiClient] API key missing before file upload.");
            onDone?.Invoke(null);
            yield break;
        }

        if (fileBytes == null || fileBytes.Length == 0)
        {
            Debug.LogError($"[QuestApiClient] File bytes missing for {displayName}.");
            onDone?.Invoke(null);
            yield break;
        }

        string metadataJson = JsonConvert.SerializeObject(new
        {
            file = new
            {
                display_name = displayName
            }
        });

        using UnityWebRequest startReq = new UnityWebRequest(fileUploadEndpoint, "POST");
        startReq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(metadataJson));
        startReq.downloadHandler = new DownloadHandlerBuffer();
        startReq.SetRequestHeader("x-goog-api-key", apiKey);
        startReq.SetRequestHeader("X-Goog-Upload-Protocol", "resumable");
        startReq.SetRequestHeader("X-Goog-Upload-Command", "start");
        startReq.SetRequestHeader("X-Goog-Upload-Header-Content-Length", fileBytes.Length.ToString());
        startReq.SetRequestHeader("X-Goog-Upload-Header-Content-Type", "application/json");
        startReq.SetRequestHeader("Content-Type", "application/json");

        yield return startReq.SendWebRequest();

        if (startReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[QuestApiClient] Upload start failed for {displayName}: {startReq.error}");
            Debug.LogError(startReq.downloadHandler.text);
            onDone?.Invoke(null);
            yield break;
        }

        string uploadUrl = startReq.GetResponseHeader("X-Goog-Upload-URL");
        if (string.IsNullOrWhiteSpace(uploadUrl))
        {
            Debug.LogError($"[QuestApiClient] Missing X-Goog-Upload-URL for {displayName}.");
            onDone?.Invoke(null);
            yield break;
        }

        using UnityWebRequest uploadReq = new UnityWebRequest(uploadUrl, "POST");
        uploadReq.uploadHandler = new UploadHandlerRaw(fileBytes);
        uploadReq.downloadHandler = new DownloadHandlerBuffer();
        uploadReq.SetRequestHeader("Content-Length", fileBytes.Length.ToString());
        uploadReq.SetRequestHeader("X-Goog-Upload-Offset", "0");
        uploadReq.SetRequestHeader("X-Goog-Upload-Command", "upload, finalize");

        yield return uploadReq.SendWebRequest();

        if (uploadReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[QuestApiClient] File upload failed for {displayName}: {uploadReq.error}");
            Debug.LogError(uploadReq.downloadHandler.text);
            onDone?.Invoke(null);
            yield break;
        }

        try
        {
            JObject root = JObject.Parse(uploadReq.downloadHandler.text);
            string fileUri = root["file"]?["uri"]?.ToString();

            if (string.IsNullOrWhiteSpace(fileUri))
            {
                Debug.LogError($"[QuestApiClient] Upload succeeded but no file.uri returned for {displayName}.");
                onDone?.Invoke(null);
                yield break;
            }

            Debug.Log($"[QuestApiClient] Uploaded {displayName} -> {fileUri}");
            onDone?.Invoke(fileUri);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QuestApiClient] Failed parsing upload response for {displayName}.\n{ex}");
            onDone?.Invoke(null);
        }
    }

    // private string BuildSystemInstruction()
    // {
    //     return
    //         "This prompt was sent to you from a unity6 prototype, you are requested to generate a quest" +
    //         "JSON object.\n The idea is, while the player plays the prototype, eveything is recorded into\n" +
    //         "a player profile JSON. This player profile JSON is then used by an LLM (you) to generate a simple\n" +
    //         "quest that fits the player's profile and playstyle. Now, the prototype will send you two other things \n" +
    //         "a game scope JSON, which contains a short discription about the game; a list of areas, enemies,\n"+
    //         " and items that exist in the game world, giving you an idea of the game. and  quest rules JSON, \n" +
    //         "which contains quest generation limits and guidelines that you should follow when generating the quest.\n" +
    //         "It is very important to only return one json object that fits the schema provided, and make sure\n"+
    //         "to follow the guidelines provided.";
    // }

    private string BuildSystemInstruction()
    {
        return
            "You generate procedural quest JSON for a Unity game.\n" +
            "Return exactly one JSON object and nothing else.\n" +
            "Do not return markdown, code fences, comments, or explanations.\n" +
            "Use only these top-level fields: quest_id, title, description, tags, reward, steps.\n" +
            "Each step must use exactly these fields: step_id, type, target, required, progress, text.\n" +
            "Use target, not target_id.\n" +
            "Use required, not quantity.\n" +
            "Use text, not description.\n" +
            "Set progress to 0 for every step.\n" +
            "Only use these step types: EnterArea, ReturnToArea, TimeInArea, KillEnemy, KillWithWeapon, KillEnemyWithWeapon, CollectItem, Interact.\n" +
            "For KillEnemyWithWeapon, target must be ENEMY_ID:WEAPON_TYPE.\n" +
            "The quest must be completable by the runtime.";
    }

    private string BuildUserPrompt(string playerProfileJson, string gameScopeJson, string questRulesJson)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Create one personalized quest for this player.");
        sb.AppendLine();

        sb.AppendLine("PLAYER PROFILE JSON:");
        sb.AppendLine(playerProfileJson);

        if (!string.IsNullOrWhiteSpace(gameScopeJson))
        {
            sb.AppendLine();
            sb.AppendLine("GAME SCOPE JSON:");
            sb.AppendLine(gameScopeJson);
        }

        if (!string.IsNullOrWhiteSpace(questRulesJson))
        {
            sb.AppendLine();
            sb.AppendLine("QUEST RULES JSON:");
            sb.AppendLine(questRulesJson);
        }

        return sb.ToString();
    }

    // private string ExtractQuestJson(string rawResponse)
    // {
    //     try
    //     {
    //         JObject root = JObject.Parse(rawResponse);

    //         string blockReason = root["promptFeedback"]?["blockReason"]?.ToString();
    //         if (!string.IsNullOrWhiteSpace(blockReason))
    //         {
    //             Debug.LogError($"[QuestApiClient] Prompt blocked by Gemini: {blockReason}");
    //             return null;
    //         }

    //         JToken parts = root["candidates"]?[0]?["content"]?["parts"];
    //         if (parts == null || parts.Type != JTokenType.Array)
    //             return null;

    //         StringBuilder sb = new StringBuilder();

    //         foreach (JToken part in parts)
    //         {
    //             string text = part?["text"]?.ToString();
    //             if (!string.IsNullOrWhiteSpace(text))
    //                 sb.Append(text);
    //         }

    //         return sb.ToString();
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"[QuestApiClient] Failed to parse Gemini response.\n{ex}");
    //         return null;
    //     }
    // }

    private string ExtractQuestJson(string rawResponse)
    {
        try
        {
            JObject root = JObject.Parse(rawResponse);

            string blockReason = root["promptFeedback"]?["blockReason"]?.ToString();
            if (!string.IsNullOrWhiteSpace(blockReason))
            {
                Debug.LogError($"[QuestApiClient] Prompt blocked by Gemini: {blockReason}");
                return null;
            }

            JToken candidate = root["candidates"]?[0];

            string finishReason = candidate?["finishReason"]?.ToString();
            string finishMessage = candidate?["finishMessage"]?.ToString();

            Debug.Log("[QuestApiClient] Gemini finishReason: " + finishReason);

            if (!string.IsNullOrWhiteSpace(finishMessage))
                Debug.Log("[QuestApiClient] Gemini finishMessage: " + finishMessage);

            string totalTokens = root["usageMetadata"]?["totalTokenCount"]?.ToString();
            string promptTokens = root["usageMetadata"]?["promptTokenCount"]?.ToString();
            string candidatesTokens = root["usageMetadata"]?["candidatesTokenCount"]?.ToString();
            string thoughtsTokens = root["usageMetadata"]?["thoughtsTokenCount"]?.ToString();

            Debug.Log(
                $"[QuestApiClient] Tokens - prompt: {promptTokens}, output: {candidatesTokens}, thoughts: {thoughtsTokens}, total: {totalTokens}"
            );

            if (!string.IsNullOrWhiteSpace(finishReason) && finishReason != "STOP")
            {
                Debug.LogError("[QuestApiClient] Gemini did not finish normally. Not parsing quest JSON.");
                questGenerationUI.turnOnErrorWarning("fin#01");
                return null;
            }

            JToken parts = candidate?["content"]?["parts"];
            if (parts == null || parts.Type != JTokenType.Array)
                return null;

            StringBuilder sb = new StringBuilder();

            foreach (JToken part in parts)
            {
                string text = part?["text"]?.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                    sb.Append(text);
            }

            string questJson = sb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(questJson))
                return null;

            try
            {
                JObject.Parse(questJson);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[QuestApiClient] Extracted quest JSON is not valid JSON.");
                Debug.LogError("[QuestApiClient] JSON ended with: " + GetLastChars(questJson, 300));
                Debug.LogError(ex.Message);
                return null;
            }

            return questJson;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QuestApiClient] Failed to parse Gemini response.\n{ex}");
            questGenerationUI.turnOnErrorWarning("Par#01");
            return null;
        }
    }

    private string StripCodeFences(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        text = text.Trim();

        if (text.StartsWith("```"))
        {
            int firstNewLine = text.IndexOf('\n');
            if (firstNewLine >= 0)
                text = text.Substring(firstNewLine + 1);

            if (text.EndsWith("```"))
                text = text.Substring(0, text.Length - 3);
        }

        return text.Trim();
    }

    private object BuildQuestJsonSchema()
    {
        return new
        {
            type = "object",
            required = new[] { "quest_id", "title", "description", "tags", "reward", "steps" },
            properties = new
            {
                quest_id = new { type = "string" },
                title = new { type = "string" },
                description = new { type = "string" },
                tags = new
                {
                    type = "array",
                    items = new { type = "string" }
                },
                reward = new
                {
                    type = "object",
                    required = new[] { "xp", "gold" },
                    properties = new
                    {
                        xp = new { type = "integer" },
                        gold = new { type = "integer" }
                    }
                },
                steps = new
                {
                    type = "array",
                    minItems = 1,
                    maxItems = 4,
                    items = new
                    {
                        type = "object",
                        required = new[] { "step_id", "type", "target", "required", "progress", "text" },
                        properties = new
                        {
                            step_id = new { type = "string" },
                            type = new
                            {
                                type = "string",
                                @enum = new[]
                                {
                                    "EnterArea",
                                    "ReturnToArea",
                                    "TimeInArea",
                                    "KillEnemy",
                                    "KillWithWeapon",
                                    "KillEnemyWithWeapon",
                                    "CollectItem",
                                    "Interact"
                                }
                            },
                            target = new { type = "string" },
                            required = new { type = "integer" },
                            progress = new { type = "integer" },
                            text = new { type = "string" }
                        }
                    }
                }
            }
        };
    }

    private bool ValidateQuest(QuestData quest)
    {
        if (quest == null) return false;
        if (string.IsNullOrWhiteSpace(quest.quest_id)) return false;
        if (string.IsNullOrWhiteSpace(quest.title)) return false;
        if (quest.reward == null) return false;
        if (quest.steps == null || quest.steps.Length == 0) return false;

        for (int i = 0; i < quest.steps.Length; i++)
        {
            QuestStepData step = quest.steps[i];

            if (step == null) return false;
            if (string.IsNullOrWhiteSpace(step.step_id)) return false;
            if (string.IsNullOrWhiteSpace(step.type)) return false;
            if (string.IsNullOrWhiteSpace(step.target)) return false;
            if (string.IsNullOrWhiteSpace(step.text)) return false;
            if (step.required <= 0) return false;
        }

        return true;
    }

    private string GetLastChars(string text, int count)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        if (text.Length <= count)
            return text;

        return text.Substring(text.Length - count);
    }
}