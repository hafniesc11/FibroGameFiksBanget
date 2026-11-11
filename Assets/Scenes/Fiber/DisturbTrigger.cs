using UnityEngine;
using System.Collections;

public class DisturbTrigger : MonoBehaviour
{
    [Header("Efek Gangguan")]
    public GameObject disturbanceEffect;

    [Header("Mobil Utama")]
    public PodMovement podMovement;

    [Header("Pengaturan Hambatan")]
    public float slowFactor = 0.5f;
    private float speedAwal;

    [Header("Traffic Spawner")]
    public TrafficPodSpawner trafficSpawner;

    [Header("Hapus Traffic Acak")]
    public int jumlahTrafficDihapus = 2;
    public float delayHapusTraffic = 0.2f;

    [Header("Object Muncul Sementara")]
    public GameObject[] objectMunculSementara;
    public float durasiMuncul = 3f;

    [Header("Quiz")]
    public s_QuizManager quizManager;

    private bool triggered = false;

    private void Start()
    {
        ResetDisturb();
    }

    public void ResetDisturb()
    {
        triggered = false;

        for (int i = 0; i < objectMunculSementara.Length; i++)
        {
            if (objectMunculSementara[i] != null)
                objectMunculSementara[i].SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.transform.root.CompareTag("Player")) return;

        PodMovement podYangKena = other.GetComponentInParent<PodMovement>();

        if (podYangKena == null || podYangKena != podMovement)
            return;

        triggered = true;

        // Simpan kecepatan asli
        speedAwal = podMovement.speed;

        // Kurangi kecepatan
        podMovement.speed *= slowFactor;

        if (disturbanceEffect != null)
            disturbanceEffect.SetActive(true);

        podMovement.StartShake();

        if (trafficSpawner != null)
            trafficSpawner.TriggerDisturbOnTraffic();

        StartCoroutine(TampilkanObjectSementara());
        StartCoroutine(HapusTrafficSetelahDelay());
    }

    private IEnumerator TampilkanObjectSementara()
    {
        // Tampilkan objek gangguan
        for (int i = 0; i < objectMunculSementara.Length; i++)
        {
            if (objectMunculSementara[i] != null)
                objectMunculSementara[i].SetActive(true);
        }

        // Tunggu durasi gangguan
        yield return new WaitForSeconds(durasiMuncul);

        // Matikan objek gangguan
        for (int i = 0; i < objectMunculSementara.Length; i++)
        {
            if (objectMunculSementara[i] != null)
                objectMunculSementara[i].SetActive(false);
        }

        // Kembalikan kecepatan
        if (podMovement != null)
        {
            podMovement.speed = speedAwal;
        }

        // Munculkan Quiz
        if (quizManager != null)
        {
            quizManager.ShowQuiz();
        }
    }

    private IEnumerator HapusTrafficSetelahDelay()
    {
        yield return new WaitForSeconds(delayHapusTraffic);

        if (trafficSpawner != null)
            trafficSpawner.HilangkanTrafficAcak(jumlahTrafficDihapus);
    }
}