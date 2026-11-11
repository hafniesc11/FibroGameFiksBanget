using UnityEngine;

public class PCInteractable : MonoBehaviour
{
    [Header("Teks Interaksi")]
    public string interactionMessage = "[E] Interaksi";

    [Header("Penjelasan")]
    public GameObject penjelasanCanvas;

    public virtual void Interact()
    {
        if (penjelasanCanvas != null)
        {
            penjelasanCanvas.SetActive(true);
        }
    }
}