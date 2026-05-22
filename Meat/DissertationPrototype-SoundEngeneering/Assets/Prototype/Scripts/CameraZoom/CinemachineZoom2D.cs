using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CinemachineZoom2D : MonoBehaviour
{
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float zoomStep = 1f;

    private CinemachineVirtualCamera vcam;
    private float targetZoom;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();

        if (vcam == null)
        {
            Debug.LogError("No CinemachineVirtualCamera found on this object.");
            enabled = false;
            return;
        }

        targetZoom = Mathf.Round(vcam.m_Lens.OrthographicSize);
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0.01f)
            targetZoom -= zoomStep;   // zoom in
        else if (scroll < -0.01f)
            targetZoom += zoomStep;   // zoom out

        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        vcam.m_Lens.OrthographicSize = Mathf.Round(targetZoom);
    }
}