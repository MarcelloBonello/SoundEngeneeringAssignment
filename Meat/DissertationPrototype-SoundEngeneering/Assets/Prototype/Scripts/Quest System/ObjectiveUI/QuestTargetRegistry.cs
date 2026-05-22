using System.Collections.Generic;
using UnityEngine;

public class QuestTargetRegistry : MonoBehaviour
{
    public static QuestTargetRegistry Instance { get; private set; }

    private readonly Dictionary<string, QuestTargetMarker> _markersById = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        QuestTargetMarker[] markers = FindObjectsByType<QuestTargetMarker>(FindObjectsSortMode.None);
        for (int i = 0; i < markers.Length; i++)
        {
            Register(markers[i]);
        }
    }

    public void Register(QuestTargetMarker marker)
    {
        if (marker == null || string.IsNullOrWhiteSpace(marker.TargetId))
            return;

        _markersById[marker.TargetId] = marker;
    }

    public void Unregister(QuestTargetMarker marker)
    {
        if (marker == null || string.IsNullOrWhiteSpace(marker.TargetId))
            return;

        if (_markersById.TryGetValue(marker.TargetId, out var current) && current == marker)
        {
            _markersById.Remove(marker.TargetId);
        }
    }

    public Transform GetTargetTransform(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
            return null;

        if (_markersById.TryGetValue(targetId, out var marker) && marker != null)
            return marker.TargetTransform;

        return null;
    }
}