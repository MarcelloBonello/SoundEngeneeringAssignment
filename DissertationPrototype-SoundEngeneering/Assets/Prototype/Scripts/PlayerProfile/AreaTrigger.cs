using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AreaTrigger : MonoBehaviour
{
    [SerializeField] private string areaId;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player")) return;

        GameEvents.AreaEntered(areaId);
        Debug.Log($"[AreaTrigger.cs] Player entered area: {areaId}");
        
        if(PlayerProfile.Instance == null) return;
        PlayerProfile.Instance.SetCurrentArea(areaId);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (PlayerProfile.Instance == null) return;

        GameEvents.AreaExited(areaId);
        Debug.Log($"[AreaTrigger.cs] Player exited area: {areaId}\nTime: ${PlayerProfile.Instance.Data.timeInArea}");

        if(PlayerProfile.Instance.CurrentAreaId == areaId)
            PlayerProfile.Instance.SetCurrentArea(" ");
    }
}
