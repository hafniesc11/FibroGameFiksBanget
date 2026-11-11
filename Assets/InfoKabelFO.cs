using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InfoKabelFO : MonoBehaviour
{
    [Header("Referensi Canvas Penjelasan")]
    public GameObject infoCanvas; 
    private XRBaseInteractable interactable;

    void Start()
    {
        interactable = GetComponent<XRBaseInteractable>();
        
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(TampilkanTeks);
            interactable.selectExited.AddListener(SembunyikanTeks);
        }
        
        // Pastikan di awal game canvas-nya mati
        if (infoCanvas != null) infoCanvas.SetActive(false);
    }

    public void TampilkanTeks(SelectEnterEventArgs args = null)
    {
        if (infoCanvas != null) 
        {
            infoCanvas.SetActive(true);

            // Aktifkan semua anak (panel teks/gambar seperti aboutFO/aboutFS) di dalamnya
            for (int i = 0; i < infoCanvas.transform.childCount; i++)
            {
                infoCanvas.transform.GetChild(i).gameObject.SetActive(true);
            }

            Debug.Log("[InfoKabelFO] Canvas penjelasan aktif: " + infoCanvas.name);
        }
    }

    public void SembunyikanTeks(SelectExitEventArgs args = null)
    {
        if (infoCanvas != null) 
        {
            infoCanvas.SetActive(false);
            Debug.Log("[InfoKabelFO] Canvas penjelasan disembunyikan.");
        }
    }

    public void ToggleInfo()
    {
        if (infoCanvas != null)
        {
            bool state = !infoCanvas.activeSelf;
            infoCanvas.SetActive(state);
            if (state)
            {
                for (int i = 0; i < infoCanvas.transform.childCount; i++)
                {
                    infoCanvas.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }
    }
}