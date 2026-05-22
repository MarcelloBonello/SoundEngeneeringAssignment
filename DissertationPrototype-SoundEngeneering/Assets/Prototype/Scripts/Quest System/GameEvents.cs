using System;

public static class GameEvents
{
    //areas / zones
    public static Action<string> OnAreaEntered; //areaID
    public static Action<string> OnAreaExited;

    //combat
    public static Action<string, WeaponType> OnEnemyKilled; //enemyID, weponUsed

    //Items / Inventory
    public static Action<string, int> OnItemCollected; // ItemID, amount

    //Interacitons
    public static Action<string> OnInteracted; //interactableID

    public static void AreaEntered(string areaId) => 
        OnAreaEntered?.Invoke(areaId);
    public static void AreaExited(string areaId)  => 
        OnAreaExited?.Invoke(areaId);

    public static void EnemyKilled(string enemyId, WeaponType weaponTypeUsed) =>
        OnEnemyKilled?.Invoke(enemyId, weaponTypeUsed);

    public static void ItemCollected(string itemId, int amount = 1) =>
        OnItemCollected?.Invoke(itemId, amount);

    public static void Interacted(string interactableId) =>
        OnInteracted?.Invoke(interactableId);
}