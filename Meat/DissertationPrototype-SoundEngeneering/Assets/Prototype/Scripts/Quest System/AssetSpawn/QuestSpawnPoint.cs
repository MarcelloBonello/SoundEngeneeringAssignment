using UnityEngine;

public class QuestSpawnPoint : MonoBehaviour
{
    [Header("Item Spawn Settings")]
    [SerializeField] private string itemSpawnID = "SUPPLY_CRATE";

    [Header("Enemy Spawn Settings")]
    [SerializeField] private string enemySpawnID = "ENEMY_RANDOM";

    public string ItemSpawnID => itemSpawnID;
    public string EnemySpawnID => enemySpawnID;

    private const string EnemyRandomId = "ENEMY_RANDOM";
    private const string EnemyMeleeId = "ENEMY_MELLE";
    private const string EnemyRangedId = "ENEMY_RANGED";

    public bool CanSpawnItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        if (string.IsNullOrEmpty(itemSpawnID)) return false;

        return itemSpawnID == itemId;
    }

    public bool CanSpawnEnemy(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId)) return false;
        if (string.IsNullOrEmpty(enemySpawnID)) return false;

        if (enemySpawnID == EnemyRandomId)
        {
            return enemyId == EnemyMeleeId || enemyId == EnemyRangedId;
        }

        return enemySpawnID == enemyId;
    }
}