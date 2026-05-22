// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public abstract class Enemy : Character
// {
//     public enum State
//     {
//         Idle,
//         Chase,
//         Attack
//     }

//     protected State curState;

//     [SerializeField] protected float moveSpeed;
//     [SerializeField] protected float chaseDistance;

//     [SerializeField] protected ItemData[] dropItems;
//     [SerializeField] protected GameObject dropItemPrefab;

//     [Header("Quest")]
//     [SerializeField] private string questEnemyId = "";

//     protected GameObject target;

//     protected float lastAttackTime;
//     protected float targetDistance;

//     [Header("Components")]
//     [SerializeField] protected SpriteRenderer spriteRenderer;

//     private bool hasDied = false;

//     private WeaponType lastWeaponTypeUsed;
//     private bool hasRegisteredWeaponHit = false;

//     protected virtual void Start()
//     {
//         Player player = FindObjectOfType<Player>();

//         if (player != null)
//         {
//             target = player.gameObject;
//         }
//         else
//         {
//             Debug.LogWarning("[Enemy] No Player found in scene.");
//         }
//     }

//     protected virtual void Update()
//     {
//         if (target == null) return;

//         targetDistance = Vector2.Distance(transform.position, target.transform.position);

//         if (spriteRenderer != null)
//         {
//             spriteRenderer.flipX = GetTargetDirection().x < 0;
//         }

//         switch (curState)
//         {
//             case State.Idle:
//                 IdleUpdate();
//                 break;

//             case State.Chase:
//                 ChaseUpdate();
//                 break;

//             case State.Attack:
//                 AttackUpdate();
//                 break;
//         }
//     }

//     void ChangeState(State newState)
//     {
//         curState = newState;
//     }

//     void IdleUpdate()
//     {
//         if (targetDistance <= chaseDistance)
//         {
//             ChangeState(State.Chase);

//             PlayerProfile.Instance?.RecordAggro();

//             if (PlayerProfile.Instance != null)
//             {
//                 Debug.Log($"Enemy aggroed. Total aggroes: {PlayerProfile.Instance.Data.enemiesAggroed}");
//             }
//             else
//             {
//                 Debug.Log("Enemy aggroed.");
//             }
//         }
//     }

//     void ChaseUpdate()
//     {
//         if (InAttackRange())
//         {
//             ChangeState(State.Attack);
//         }
//         else if (targetDistance > chaseDistance)
//         {
//             ChangeState(State.Idle);
//         }

//         transform.position = Vector3.MoveTowards(
//             transform.position,
//             target.transform.position,
//             moveSpeed * Time.deltaTime
//         );
//     }

//     void AttackUpdate()
//     {
//         if (targetDistance > chaseDistance)
//         {
//             ChangeState(State.Idle);
//         }
//         else if (!InAttackRange())
//         {
//             ChangeState(State.Chase);
//         }

//         if (CanAttack())
//         {
//             lastAttackTime = Time.time;
//             AttackTarget();
//         }
//     }

//     protected virtual void AttackTarget()
//     {

//     }

//     protected virtual bool CanAttack()
//     {
//         return false;
//     }

//     protected virtual bool InAttackRange()
//     {
//         return false;
//     }

//     protected Vector2 GetTargetDirection()
//     {
//         if (target == null) return Vector2.zero;
//         return (target.transform.position - transform.position).normalized;
//     }

//     public void SetQuestEnemyId(string newQuestEnemyId)
//     {
//         if (string.IsNullOrWhiteSpace(newQuestEnemyId)) return;

//         questEnemyId = newQuestEnemyId.Trim();
//     }

//     public void RegisterHitWeapon(WeaponType weaponType)
//     {
//         lastWeaponTypeUsed = weaponType;
//         hasRegisteredWeaponHit = true;

//         Debug.Log($"[Enemy] Registered hit weapon: {weaponType}");
//     }

//     public override void Die()
//     {
//         if (hasDied) return;
//         hasDied = true;

//         PlayerProfile.Instance?.RecordKill();

//         if (PlayerProfile.Instance != null)
//         {
//             Debug.Log($"Enemy died. Total kills: {PlayerProfile.Instance.Data.enemiesKilled}");
//         }
//         else
//         {
//             Debug.Log("Enemy died.");
//         }

//         ReportQuestEnemyKilled();

//         DropItems();

//         Destroy(gameObject);
//     }

//     private void ReportQuestEnemyKilled()
//     {
//         string enemyIdToReport = GetQuestEnemyId();

//         if (string.IsNullOrWhiteSpace(enemyIdToReport))
//         {
//             Debug.LogWarning("[Enemy] Could not report quest kill because enemy ID is empty.");
//             return;
//         }

//         if (!hasRegisteredWeaponHit)
//         {
//             Debug.LogWarning(
//                 $"[Enemy] Enemy died but no weapon hit was registered. " +
//                 $"Quest kill will not be reported. EnemyID='{enemyIdToReport}'"
//             );

//             return;
//         }

//         GameEvents.EnemyKilled(enemyIdToReport, lastWeaponTypeUsed);

//         Debug.Log($"[Enemy] Reported quest kill: {enemyIdToReport} using {lastWeaponTypeUsed}");
//     }

//     private string GetQuestEnemyId()
//     {
//         QuestSpawnItem questSpawnItem = GetComponent<QuestSpawnItem>();

//         if (questSpawnItem != null && !string.IsNullOrWhiteSpace(questSpawnItem.EnemySpawnId))
//         {
//             return questSpawnItem.EnemySpawnId;
//         }

//         return questEnemyId;
//     }

//     protected void DropItems()
//     {
//         if (dropItems == null || dropItems.Length == 0) return;
//         if (dropItemPrefab == null) return;

//         for (int i = 0; i < dropItems.Length; i++)
//         {
//             if (dropItems[i] == null) continue;

//             GameObject obj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
//             obj.GetComponent<WorldItem>().SetItem(dropItems[i]);
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Character
{
    public enum State
    {
        Idle,
        Chase,
        Attack
    }

    protected State curState;

    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float chaseDistance;

    [SerializeField] protected ItemData[] dropItems;
    [SerializeField] protected GameObject dropItemPrefab;

    [Header("Quest")]
    [SerializeField] private string questEnemyId = "";

    protected GameObject target;

    protected float lastAttackTime;
    protected float targetDistance;

    [Header("Components")]
    [SerializeField] protected SpriteRenderer spriteRenderer;

    private bool hasDied = false;

    private bool hasRecordedAvoidedThisAggro = false;

    private WeaponType lastWeaponTypeUsed;
    private bool hasRegisteredWeaponHit = false;

    protected virtual void Start()
    {
        Player player = FindObjectOfType<Player>();

        if (player != null)
        {
            target = player.gameObject;
        }
        else
        {
            Debug.LogWarning("[Enemy] No Player found in scene.");
        }
    }

    protected virtual void Update()
    {
        if (target == null) return;
        if (hasDied) return;

        targetDistance = Vector2.Distance(transform.position, target.transform.position);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = GetTargetDirection().x < 0;
        }

        switch (curState)
        {
            case State.Idle:
                IdleUpdate();
                break;

            case State.Chase:
                ChaseUpdate();
                break;

            case State.Attack:
                AttackUpdate();
                break;
        }
    }

    void ChangeState(State newState)
    {
        curState = newState;
    }

    void IdleUpdate()
    {
        if (targetDistance <= chaseDistance)
        {
            ChangeState(State.Chase);

            hasRecordedAvoidedThisAggro = false;

            PlayerProfile.Instance?.RecordAggro();

            if (PlayerProfile.Instance != null)
            {
                Debug.Log($"Enemy aggroed. Total aggroes: {PlayerProfile.Instance.Data.enemiesAggroed}");
            }
            else
            {
                Debug.Log("Enemy aggroed.");
            }
        }
    }

    void ChaseUpdate()
    {
        if (InAttackRange())
        {
            ChangeState(State.Attack);
            return;
        }

        if (targetDistance > chaseDistance)
        {
            RecordAvoidedAndReturnToIdle();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.transform.position,
            moveSpeed * Time.deltaTime
        );
    }

    void AttackUpdate()
    {
        if (targetDistance > chaseDistance)
        {
            RecordAvoidedAndReturnToIdle();
            return;
        }

        if (!InAttackRange())
        {
            ChangeState(State.Chase);
            return;
        }

        if (CanAttack())
        {
            lastAttackTime = Time.time;
            AttackTarget();
        }
    }

    private void RecordAvoidedAndReturnToIdle()
    {
        if (!hasDied && !hasRecordedAvoidedThisAggro)
        {
            hasRecordedAvoidedThisAggro = true;

            PlayerProfile.Instance?.RecordEnemyAvoided();

            if (PlayerProfile.Instance != null)
            {
                Debug.Log($"Enemy avoided. Total avoided: {PlayerProfile.Instance.Data.enemiesAvoided}");
            }
            else
            {
                Debug.Log("Enemy avoided.");
            }
        }

        ChangeState(State.Idle);
    }

    protected virtual void AttackTarget()
    {

    }

    protected virtual bool CanAttack()
    {
        return false;
    }

    protected virtual bool InAttackRange()
    {
        return false;
    }

    protected Vector2 GetTargetDirection()
    {
        if (target == null) return Vector2.zero;
        return (target.transform.position - transform.position).normalized;
    }

    public void SetQuestEnemyId(string newQuestEnemyId)
    {
        if (string.IsNullOrWhiteSpace(newQuestEnemyId)) return;

        questEnemyId = newQuestEnemyId.Trim();
    }

    public void RegisterHitWeapon(WeaponType weaponType)
    {
        lastWeaponTypeUsed = weaponType;
        hasRegisteredWeaponHit = true;

        Debug.Log($"[Enemy] Registered hit weapon: {weaponType}");
    }

    public override void Die()
    {
        if (hasDied) return;
        hasDied = true;

        PlayerProfile.Instance?.RecordKill();

        if (PlayerProfile.Instance != null)
        {
            Debug.Log($"Enemy died. Total kills: {PlayerProfile.Instance.Data.enemiesKilled}");
        }
        else
        {
            Debug.Log("Enemy died.");
        }

        ReportQuestEnemyKilled();

        DropItems();

        Destroy(gameObject);
    }

    private void ReportQuestEnemyKilled()
    {
        string enemyIdToReport = GetQuestEnemyId();

        if (string.IsNullOrWhiteSpace(enemyIdToReport))
        {
            Debug.LogWarning("[Enemy] Could not report quest kill because enemy ID is empty.");
            return;
        }

        if (!hasRegisteredWeaponHit)
        {
            Debug.LogWarning(
                $"[Enemy] Enemy died but no weapon hit was registered. " +
                $"Quest kill will not be reported. EnemyID='{enemyIdToReport}'"
            );

            return;
        }

        GameEvents.EnemyKilled(enemyIdToReport, lastWeaponTypeUsed);

        Debug.Log($"[Enemy] Reported quest kill: {enemyIdToReport} using {lastWeaponTypeUsed}");
    }

    private string GetQuestEnemyId()
    {
        QuestSpawnItem questSpawnItem = GetComponent<QuestSpawnItem>();

        if (questSpawnItem != null && !string.IsNullOrWhiteSpace(questSpawnItem.EnemySpawnId))
        {
            return questSpawnItem.EnemySpawnId;
        }

        return questEnemyId;
    }

    protected void DropItems()
    {
        if (dropItems == null || dropItems.Length == 0) return;
        if (dropItemPrefab == null) return;

        for (int i = 0; i < dropItems.Length; i++)
        {
            if (dropItems[i] == null) continue;

            GameObject obj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            obj.GetComponent<WorldItem>().SetItem(dropItems[i]);
        }
    }
}