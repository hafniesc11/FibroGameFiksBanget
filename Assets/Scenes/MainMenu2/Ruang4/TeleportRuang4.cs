using UnityEngine;

public class TeleportRuang4 : MonoBehaviour
{
    public Transform xrOrigin;
    public Transform spawnRuang4;

    public void MasukRuang4()
    {
        Debug.Log("Masuk Ruang 4");

        xrOrigin.position = spawnRuang4.position;
        xrOrigin.rotation = spawnRuang4.rotation;
    }
}