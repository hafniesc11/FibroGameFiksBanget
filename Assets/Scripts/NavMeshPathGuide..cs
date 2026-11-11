using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NavMeshPathGuide : MonoBehaviour
{
    public Transform player;       // Isi dengan PCPlayer
    public Transform targetDoor;   // Isi dengan pintu tujuan
    public float distanceToHide = 1.5f;

    private LineRenderer lineRenderer;
    private NavMeshPath path;

    void Start()
    {
        path = new NavMeshPath();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (player == null || targetDoor == null || lineRenderer == null) return;

        // Jarak player ke pintu
        float distToTarget = Vector3.Distance(player.position, targetDoor.position);

        // Jika player sudah di depan pintu, sembunyikan semua garis
        if (distToTarget <= distanceToHide)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // Ambil titik NavMesh paling dekat dari posisi kaki player
        Vector3 startPos = player.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(player.position, out hit, 3.0f, NavMesh.AllAreas))
        {
            startPos = hit.position;
        }

        // Hitung rute dari posisi player saat ini ke target door
        if (NavMesh.CalculatePath(startPos, targetDoor.position, NavMesh.AllAreas, path))
        {
            if (path.corners.Length > 0)
            {
                lineRenderer.positionCount = path.corners.Length;

                for (int i = 0; i < path.corners.Length; i++)
                {
                    Vector3 point = path.corners[i];
                    // Pakai Y lantai paling bawah agar garis pas menempel di atas lantai
                    point.y = startPos.y + 0.05f; 
                    lineRenderer.SetPosition(i, point);
                }
            }
        }
    }
}