using UnityEngine;

public class QuestTargetMarker : MonoBehaviour
{
    [SerializeField] private string targetId;

    public string TargetId => targetId;
    public Transform TargetTransform => transform;

    private void OnEnable()
    {
        if (QuestTargetRegistry.Instance != null)
            QuestTargetRegistry.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (QuestTargetRegistry.Instance != null)
            QuestTargetRegistry.Instance.Unregister(this);
    }
}