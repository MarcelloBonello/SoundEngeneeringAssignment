// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class MeleeEquipItem : EquipItem
// {
//     [SerializeField] private LayerMask hitLayerMask;
//     [SerializeField] private Animator anim;
//     private float lastAttackTime;

//     [SerializeField] private AudioClip swingSFX;

//     public override void OnUse ()
//     {
//         MeleeWeaponItemData i = item as MeleeWeaponItemData;

//         if(Time.time - lastAttackTime < i.AttackRate)
//             return;

//         lastAttackTime = Time.time;

//         anim.SetTrigger("Attack");

//         RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, i.Range, hitLayerMask);

//         if(hit.collider != null)
//         {

//             PlayerProfile.Instance?.RecordHit(AttackType.Melee, i.weaponType);

//             IDamagable damagable = hit.collider.GetComponent<IDamagable>();

//             if(damagable != null)
//             {
//                 PlayerProfile.Instance?.RecordDamageDealt(i.Damage);
//                 Debug.Log($"Hit {damagable.GetTeam()} for {i.Damage} damage");
//                 damagable.TakeDamage(i.Damage);
//             }
//         }

//         // play sound effect
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEquipItem : EquipItem
{
    [SerializeField] private LayerMask hitLayerMask;
    [SerializeField] private Animator anim;
    private float lastAttackTime;

    [SerializeField] private AudioClip swingSFX;

    public override void OnUse()
    {
        MeleeWeaponItemData i = item as MeleeWeaponItemData;

        if (i == null)
        {
            Debug.LogWarning("[MeleeEquipItem] Current item is not MeleeWeaponItemData.");
            return;
        }

        if (Time.time - lastAttackTime < i.AttackRate)
            return;

        lastAttackTime = Time.time;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            transform.up,
            i.Range,
            hitLayerMask
        );

        if (hit.collider != null)
        {
            IDamagable damagable = hit.collider.GetComponentInParent<IDamagable>();

            if (damagable != null)
            {
                Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

                if (enemy != null)
                {
                    enemy.RegisterHitWeapon(i.weaponType);
                }

                PlayerProfile.Instance?.RecordHit(AttackType.Melee, i.weaponType);
                PlayerProfile.Instance?.RecordDamageDealt(i.Damage);

                Debug.Log($"Hit {damagable.GetTeam()} for {i.Damage} damage using {i.weaponType}");

                damagable.TakeDamage(i.Damage);
            }
        }

        // play sound effect
    }
}