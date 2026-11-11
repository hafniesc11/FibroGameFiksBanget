using UnityEngine;
using UnityEngine.UI; // Diperlukan jika menggunakan UI Button standar

public class TriggerINF : MonoBehaviour
{
    [Header("Canvas yang akan dikontrol")]
    public GameObject targetCanvas; // Seret Canvas kamu ke sini lewat Inspector

    [Header("Tombol Lanjut pada Canvas")]
    public Button lanjutButton;     // Seret tombol 'Lanjut' ke sini lewat Inspector

    private bool playerDiDalamArea = false;

    private void Start()
    {
        // Pastikan tombol mendengarkan klik dari player
        if (lanjutButton != null)
        {
            lanjutButton.onClick.AddListener(TutupCanvas);
        }

        // Sembunyikan canvas di awal permainan
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
{
    Debug.Log("Objek yang menyentuh: " + other.gameObject.name + " dengan Tag: " + other.tag);
    
    if (other.CompareTag("Player"))
    {
        Debug.Log("Player terdeteksi! Memunculkan Canvas.");
        playerDiDalamArea = true;
        if (targetCanvas != null) targetCanvas.SetActive(true);
    }
}

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDiDalamArea = false;

            // (Opsional) Sembunyikan canvas jika player menjauh
            if (targetCanvas != null)
            {
                targetCanvas.SetActive(false);
            }
        }
    }

    // Fungsi yang dipanggil saat tombol Lanjut diklik
    void TutupCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false); // Menyembunyikan canvas
        }
    }
}