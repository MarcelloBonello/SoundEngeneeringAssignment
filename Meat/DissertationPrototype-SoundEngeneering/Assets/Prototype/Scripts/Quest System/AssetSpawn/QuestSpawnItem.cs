using UnityEngine;

public class QuestSpawnItem : MonoBehaviour
{
    private enum SpawnedAssetType
    {
        None,
        Pickup,
        Enemy
    }

    [SerializeField] private SpawnedAssetType assetType = SpawnedAssetType.None;

    [Header("IDs")]
    [SerializeField] private string enemySpawnId;
    [SerializeField] private string itemSpawnId;

    [Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool reportCollectionOnDestroyIfTouchedByPlayer = true;

    private bool _collected = false;
    private bool _collectionReported = false;
    private bool _suppressCollectionReport = false;
    private bool _playerTouched = false;

    public string ItemId => itemSpawnId;
    public string EnemySpawnId => enemySpawnId;

    public string SpawnId
    {
        get
        {
            if (assetType == SpawnedAssetType.Enemy)
                return enemySpawnId;

            return itemSpawnId;
        }
    }

    public void InitItem(string newItemId)
    {
        assetType = SpawnedAssetType.Pickup;
        itemSpawnId = newItemId;
        enemySpawnId = string.Empty;

        _collected = false;
        _collectionReported = false;
        _suppressCollectionReport = false;
        _playerTouched = false;

        gameObject.name = $"Pickup_{itemSpawnId}";
    }

    public void InitEnemy(string newEnemySpawnId)
    {
        assetType = SpawnedAssetType.Enemy;
        enemySpawnId = newEnemySpawnId;
        itemSpawnId = string.Empty;

        _collected = false;
        _collectionReported = false;
        _suppressCollectionReport = false;
        _playerTouched = false;

        gameObject.name = $"Enemy_{enemySpawnId}";
    }

    public void SuppressQuestCollectionReport()
    {
        _suppressCollectionReport = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollectFromCollider(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollectFromCollider(other);
    }

    private void TryCollectFromCollider(Collider2D other)
    {
        if (assetType != SpawnedAssetType.Pickup) return;
        if (_collected) return;
        if (!IsPlayerCollider(other)) return;

        _playerTouched = true;
        Collect();
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other == null) return false;

        if (other.CompareTag(playerTag))
            return true;

        if (other.transform.root != null && other.transform.root.CompareTag(playerTag))
            return true;

        Player player = other.GetComponentInParent<Player>();
        if (player != null)
            return true;

        return false;
    }

    public void Collect()
    {
        if (!TryReportCollection())
            return;

        Destroy(gameObject);
    }

    private bool TryReportCollection()
    {
        if (assetType != SpawnedAssetType.Pickup) return false;
        if (_collectionReported) return false;
        if (_suppressCollectionReport) return false;

        if (string.IsNullOrEmpty(itemSpawnId))
        {
            Debug.LogWarning("[QuestSpawnItem] Cannot collect item because itemSpawnId is empty.");
            return false;
        }

        _collected = true;
        _collectionReported = true;

        GameEvents.ItemCollected(itemSpawnId, 1);

        Debug.Log($"[QuestSpawnItem] Reported item collected: {itemSpawnId}");

        return true;
    }

    private void OnDestroy()
    {
        if (!reportCollectionOnDestroyIfTouchedByPlayer) return;
        if (_collectionReported) return;
        if (_suppressCollectionReport) return;
        if (assetType != SpawnedAssetType.Pickup) return;
        if (!_playerTouched) return;

        TryReportCollection();
    }
}