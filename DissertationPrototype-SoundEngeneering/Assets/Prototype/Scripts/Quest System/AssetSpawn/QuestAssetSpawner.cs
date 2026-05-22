using System.Collections.Generic;
using UnityEngine;

public class QuestAssetSpawner : MonoBehaviour
{
    public static QuestAssetSpawner Instance { get; private set; }

    private const string EnemyRandomId = "ENEMY_RANDOM";
    private const string EnemyMeleeId = "ENEMY_MELLE";
    private const string EnemyRangedId = "ENEMY_RANGED";

    [Header("Pickup")]
    [SerializeField] private QuestSpawnItem pickupPrefab;

    [Header("Enemies")]
    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;

    [Header("Spawn Points")]
    [SerializeField] private QuestSpawnPoint[] spawnPoints;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform spawnedAssetParent;

    [Header("Fallback")]
    [SerializeField] private string defaultEnemySpawnID = EnemyMeleeId;

    [Header("Multi Spawn")]
    [SerializeField] private float multiSpawnSpacing = 1.5f;

    private readonly List<GameObject> _spawnedAssets = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowForStep(QuestStepData step)
    {
        ClearSpawnedPickups();

        if (step == null) return;

        if (playerTransform == null)
        {
            Debug.LogWarning("[QuestAssetSpawner] Player Transform is missing.");
            return;
        }

        if (SameId(step.type, "CollectItem"))
        {
            SpawnPickupsForStep(step);
        }
        else if (IsEnemyStep(step.type))
        {
            SpawnEnemiesForStep(step);
        }
    }

    private void SpawnPickupsForStep(QuestStepData step)
    {
        string itemSpawnID = NormalizeId(step.target);

        if (string.IsNullOrEmpty(itemSpawnID))
        {
            Debug.LogWarning("[QuestAssetSpawner] CollectItem step has an empty target.");
            return;
        }

        if (pickupPrefab == null)
        {
            Debug.LogWarning("[QuestAssetSpawner] Pickup prefab is missing.");
            return;
        }

        int pickupsToSpawn = Mathf.Max(1, step.required - step.progress);

        QuestSpawnPoint basePoint = GetNearestUsableSpawnPoint(
            itemSpawnID,
            true,
            playerTransform.position
        );

        if (basePoint == null)
        {
            Debug.LogWarning($"[QuestAssetSpawner] No item spawn point found for itemSpawnID '{itemSpawnID}'.");
            return;
        }

        for (int i = 0; i < pickupsToSpawn; i++)
        {
            Vector3 spawnPosition = basePoint.transform.position + GetSpawnOffset(i);

            QuestSpawnItem pickup = Instantiate(
                pickupPrefab,
                spawnPosition,
                Quaternion.identity,
                spawnedAssetParent
            );

            pickup.InitItem(itemSpawnID);
            pickup.gameObject.name = $"QuestPickup_{itemSpawnID}_{i + 1}";

            _spawnedAssets.Add(pickup.gameObject);

            Debug.Log($"[QuestAssetSpawner] Pickup {i + 1}/{pickupsToSpawn} spawned at {spawnPosition}.");
        }

        Debug.Log($"[QuestAssetSpawner] Spawned {pickupsToSpawn} pickup(s) with ID '{itemSpawnID}'.");
    }

    private void SpawnEnemiesForStep(QuestStepData step)
    {
        string enemySpawnID = NormalizeId(GetEnemySpawnIDFromStep(step));

        if (SameId(enemySpawnID, EnemyRandomId))
        {
            enemySpawnID = GetRandomEnemyId();
            ApplyEnemyIdToStep(step, enemySpawnID);
        }

        if (string.IsNullOrEmpty(enemySpawnID))
        {
            enemySpawnID = defaultEnemySpawnID;
        }

        enemySpawnID = NormalizeId(enemySpawnID);

        GameObject enemyPrefab = GetEnemyPrefab(enemySpawnID);

        if (enemyPrefab == null)
        {
            Debug.LogWarning($"[QuestAssetSpawner] No enemy prefab found for enemySpawnID '{enemySpawnID}'.");
            return;
        }

        int enemiesToSpawn = Mathf.Max(1, step.required - step.progress);

        QuestSpawnPoint basePoint = GetNearestUsableSpawnPoint(
            enemySpawnID,
            false,
            playerTransform.position
        );

        if (basePoint == null)
        {
            Debug.LogWarning($"[QuestAssetSpawner] No enemy spawn point found for enemySpawnID '{enemySpawnID}'.");
            return;
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 spawnPosition = basePoint.transform.position + GetSpawnOffset(i);

            GameObject enemy = Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity,
                spawnedAssetParent
            );

            enemy.name = $"QuestEnemy_{enemySpawnID}_{i + 1}";

            QuestSpawnItem questSpawnItem = enemy.GetComponent<QuestSpawnItem>();

            if (questSpawnItem == null)
            {
                questSpawnItem = enemy.AddComponent<QuestSpawnItem>();
            }

            questSpawnItem.InitEnemy(enemySpawnID);

            enemy.SendMessage(
                "SetQuestEnemyId",
                enemySpawnID,
                SendMessageOptions.DontRequireReceiver
            );

            _spawnedAssets.Add(enemy);

            Debug.Log($"[QuestAssetSpawner] Enemy {i + 1}/{enemiesToSpawn} spawned at {spawnPosition} with ID '{enemySpawnID}'.");
        }

        Debug.Log($"[QuestAssetSpawner] Spawned {enemiesToSpawn} enemy/enemies with ID '{enemySpawnID}'.");
    }

    private QuestSpawnPoint GetNearestUsableSpawnPoint(string spawnID, bool isItemSpawn, Vector3 fromPosition)
    {
        List<QuestSpawnPoint> usablePoints = GetUsableSpawnPoints(spawnID, isItemSpawn, fromPosition);

        if (usablePoints.Count == 0)
            return null;

        return usablePoints[0];
    }

    private List<QuestSpawnPoint> GetUsableSpawnPoints(string spawnID, bool isItemSpawn, Vector3 fromPosition)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = FindObjectsByType<QuestSpawnPoint>(FindObjectsSortMode.None);
        }

        List<QuestSpawnPoint> usablePoints = new List<QuestSpawnPoint>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            QuestSpawnPoint point = spawnPoints[i];
            if (point == null) continue;

            bool canUsePoint = isItemSpawn
                ? point.CanSpawnItem(spawnID)
                : point.CanSpawnEnemy(spawnID);

            if (canUsePoint)
            {
                usablePoints.Add(point);
            }
        }

        usablePoints.Sort((a, b) =>
        {
            float distA = (a.transform.position - fromPosition).sqrMagnitude;
            float distB = (b.transform.position - fromPosition).sqrMagnitude;
            return distA.CompareTo(distB);
        });

        return usablePoints;
    }

    private Vector3 GetSpawnOffset(int index)
    {
        if (index == 0) return Vector3.zero;

        float angle = index * 137.5f * Mathf.Deg2Rad;
        float ring = Mathf.Ceil(index / 6f);
        float radius = multiSpawnSpacing * ring;

        return new Vector3(
            Mathf.Cos(angle),
            Mathf.Sin(angle),
            0f
        ) * radius;
    }

    public Transform GetNearestSpawnedPickupTransform(string spawnID, Vector3 fromPosition)
    {
        spawnID = NormalizeId(spawnID);

        Transform best = null;
        float bestDistSq = float.MaxValue;

        for (int i = _spawnedAssets.Count - 1; i >= 0; i--)
        {
            GameObject go = _spawnedAssets[i];

            if (go == null)
            {
                _spawnedAssets.RemoveAt(i);
                continue;
            }

            QuestSpawnItem spawnedAsset = go.GetComponent<QuestSpawnItem>();
            if (spawnedAsset == null) continue;
            if (!SameId(spawnedAsset.SpawnId, spawnID)) continue;

            float distSq = (go.transform.position - fromPosition).sqrMagnitude;

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = go.transform;
            }
        }

        return best;
    }

    public Transform GetNearestSpawnedPickupTransform(QuestStepData step, Vector3 fromPosition)
    {
        if (step == null) return null;

        string spawnID = SameId(step.type, "KillEnemyWithWeapon")
            ? GetEnemySpawnIDFromStep(step)
            : step.target;

        if (SameId(step.type, "KillWithWeapon"))
        {
            spawnID = defaultEnemySpawnID;
        }

        return GetNearestSpawnedPickupTransform(spawnID, fromPosition);
    }

    private bool IsEnemyStep(string stepType)
    {
        return SameId(stepType, "KillEnemy") ||
               SameId(stepType, "KillWithWeapon") ||
               SameId(stepType, "KillEnemyWithWeapon");
    }

    private string GetEnemySpawnIDFromStep(QuestStepData step)
    {
        if (step == null) return string.Empty;

        if (SameId(step.type, "KillEnemy"))
        {
            return step.target;
        }

        if (SameId(step.type, "KillEnemyWithWeapon"))
        {
            if (string.IsNullOrEmpty(step.target)) return string.Empty;

            int colonIndex = step.target.IndexOf(':');

            if (colonIndex > 0)
            {
                return step.target.Substring(0, colonIndex);
            }

            return step.target;
        }

        if (SameId(step.type, "KillWithWeapon"))
        {
            return defaultEnemySpawnID;
        }

        return step.target;
    }

    private GameObject GetEnemyPrefab(string enemySpawnID)
    {
        enemySpawnID = NormalizeId(enemySpawnID);

        if (string.IsNullOrEmpty(enemySpawnID))
            return meleeEnemyPrefab;

        if (SameId(enemySpawnID, EnemyRangedId))
            return rangedEnemyPrefab;

        if (SameId(enemySpawnID, EnemyMeleeId))
            return meleeEnemyPrefab;

        string lower = enemySpawnID.ToLowerInvariant();

        if (lower.Contains("ranged"))
            return rangedEnemyPrefab;

        return meleeEnemyPrefab;
    }

    private string GetRandomEnemyId()
    {
        int randomIndex = Random.Range(0, 2);

        if (randomIndex == 0)
            return EnemyMeleeId;

        return EnemyRangedId;
    }

    private void ApplyEnemyIdToStep(QuestStepData step, string enemySpawnID)
    {
        if (step == null) return;

        enemySpawnID = NormalizeId(enemySpawnID);

        if (SameId(step.type, "KillEnemy"))
        {
            step.target = enemySpawnID;
        }
        else if (SameId(step.type, "KillEnemyWithWeapon"))
        {
            if (string.IsNullOrEmpty(step.target))
            {
                step.target = enemySpawnID;
                return;
            }

            int colonIndex = step.target.IndexOf(':');

            if (colonIndex > 0)
            {
                string weaponPart = NormalizeId(step.target.Substring(colonIndex + 1));
                step.target = $"{enemySpawnID}:{weaponPart}";
            }
            else
            {
                step.target = enemySpawnID;
            }
        }

        Debug.Log($"[QuestAssetSpawner] Enemy step resolved to '{enemySpawnID}'.");
    }

    public void ClearSpawnedPickups()
    {
        for (int i = 0; i < _spawnedAssets.Count; i++)
        {
            GameObject spawnedObject = _spawnedAssets[i];

            if (spawnedObject == null)
                continue;

            QuestSpawnItem questSpawnItem = spawnedObject.GetComponent<QuestSpawnItem>();

            if (questSpawnItem != null)
            {
                questSpawnItem.SuppressQuestCollectionReport();
            }

            Destroy(spawnedObject);
        }

        _spawnedAssets.Clear();
    }

    private bool SameId(string a, string b)
    {
        return string.Equals(NormalizeId(a), NormalizeId(b), System.StringComparison.OrdinalIgnoreCase);
    }

    private string NormalizeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return string.Empty;

        id = id.Trim().ToUpperInvariant();

        if (id == "ENEMY_MELEE")
            return EnemyMeleeId;

        return id;
    }
}