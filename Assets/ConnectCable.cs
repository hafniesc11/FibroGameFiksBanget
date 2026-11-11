using UnityEngine;

public class ConnectCable : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform startPoint; // Taruh Anchor_Port_LAN di sini
    public Transform endPoint;   // Taruh Anchor_MediaConverter_LAN di sini

    void OnValidate()
    {
        if (lineRenderer != null && startPoint != null && endPoint != null)
        {
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, endPoint.position);
        }
    }
}