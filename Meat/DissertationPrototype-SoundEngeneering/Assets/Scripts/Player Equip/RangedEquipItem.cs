using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEquipItem : EquipItem
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private AudioClip shootSFX;
    private float lastAttackTime;

    public override void OnUse ()
    {
        RangedWeaponItemData i = item as RangedWeaponItemData;

        if(Time.time - lastAttackTime < i.FireRate)
            return;

        // if(Inventory.Instance.HasItem(i.ProjectileItemData) == false)
        //     return;

        lastAttackTime = Time.time;

        // spawn the projectile
        i.Fire(muzzle.position, muzzle.rotation, Character.Team.Player);

        // remove the projectile from inventory
        Inventory.Instance.RemoveItem(i.ProjectileItemData);

        // play the sound effect
        AudioManager.Instance.PlayPlayerSound(shootSFX);
    }
}