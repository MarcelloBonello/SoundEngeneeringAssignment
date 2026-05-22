using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public EquipController EquipCtrl;
    public static Player Instance;

    void Awake ()
    {
        if(Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Update()
    {
        if(PlayerProfile.Instance == null) return;

        PlayerProfile.Instance?.AddTimerInCurrentArea(Time.deltaTime);
    }

    public override void TakeDamage(global::System.Int32 damageToTake)
    {
        base.TakeDamage(damageToTake);
        PlayerProfile.Instance?.RecordDamageTaken(damageToTake);
    }
}