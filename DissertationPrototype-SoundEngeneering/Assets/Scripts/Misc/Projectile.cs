// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Projectile : MonoBehaviour
// {
//     [SerializeField] private int damage;
//     [SerializeField] private float speed;
//     [SerializeField] private float lifetime;

//     private Character.Team team;
//     private Rigidbody2D rig;

//     void Awake ()
//     {
//         rig = GetComponent<Rigidbody2D>();
//     }

//     void Start ()
//     {
//         Destroy(gameObject, lifetime);
//     }

//     void FixedUpdate ()
//     {
//         rig.linearVelocity = transform.up * speed;
//     }

//     public void SetTeam (Character.Team t)
//     {
//         team = t;
//     }

//     void OnTriggerEnter2D (Collider2D collision)
//     {
//         if (team == Character.Team.Player)
//                 PlayerProfile.Instance?.RecordHit(AttackType.Ranged, WeaponType.Bow);

//         IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();

//         if(damagable != null && damagable.GetTeam() != team)
//         {
//             damagable.TakeDamage(damage);
//             Destroy(gameObject);

//             if (team == Character.Team.Player)
//                 PlayerProfile.Instance?.RecordDamageDealt(damage);

//             //Debug.Log($"RangedEnemy attacked for {damage} damage.");
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private float lifetime;

    private Character.Team team;
    private Rigidbody2D rig;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        rig.linearVelocity = transform.up * speed;
    }

    public void SetTeam(Character.Team t)
    {
        team = t;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        IDamagable damagable = collision.gameObject.GetComponentInParent<IDamagable>();

        if (damagable == null)
            return;

        if (damagable.GetTeam() == team)
            return;

        if (team == Character.Team.Player)
        {
            Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.RegisterHitWeapon(WeaponType.Bow);
            }

            PlayerProfile.Instance?.RecordHit(AttackType.Ranged, WeaponType.Bow);
            PlayerProfile.Instance?.RecordDamageDealt(damage);
        }

        damagable.TakeDamage(damage);

        Destroy(gameObject);
    }
}