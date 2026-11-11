using UnityEngine;
using System.Collections;

public class Welcomepert1 : MonoBehaviour
{
    public GameObject canvasWelcome; // Tarik Canvas kamu ke sini nanti
    public float jedaWaktu = 2.0f;   // Berapa detik nunggunya

    void Start()
    {
        // Pastikan canvas mati di awal
        canvasWelcome.SetActive(false);
        
        // Jalankan perintah nunggu
        StartCoroutine(MunculkanCanvas());
    }
    IEnumerator MunculkanCanvas()
    {
        yield return new WaitForSeconds(jedaWaktu);
        canvasWelcome.SetActive(true);
    }
}
    