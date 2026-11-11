using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class EvaluationCanvasManager : MonoBehaviour
{
    [Header("Panel Utama")]
    public GameObject panelBG;
    public GameObject panelEvaluasi;
    public GameObject panelInfografis;
    public GameObject panelQuiz;

    [Header("Panel Hasil Jawaban")]
    public GameObject panelJawabanBenar;
    public GameObject panelJawabanSalah;

    [Header("Button Navigasi")]
    public Button buttonTampilkanInfografis;
    public Button buttonLanjutQuiz;

    [Header("Button Jawaban Quiz")]
    public Button[] buttonJawaban;
    public int indexJawabanBenar = 0;

    [Header("Button Setelah Hasil Jawaban")]
    public Button buttonLanjutSetelahBenar;
    public Button buttonLanjutSetelahSalah;

    [Header("Object Dinyalakan Ketika Benar")]
    public GameObject[] objectDinyalakanKetikaBenar;

    [Header("Object Dimatikan Ketika Benar")]
    public GameObject[] objectDimatikanKetikaBenar;

    [Header("Object Dinyalakan Ketika Salah")]
    public GameObject[] objectDinyalakanKetikaSalah;

    [Header("Object Dimatikan Ketika Salah")]
    public GameObject[] objectDimatikanKetikaSalah;

    [Header("Teleport Provider")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportProvider;
    public GameObject xrOrigin;

    [Header("Teleport Setelah Panel Benar")]
    public bool teleportKetikaBenar = true;
    public Transform titikTeleportBenar;
    public float delayTeleportBenar = 0.3f;

    [Header("Teleport Setelah Panel Salah")]
    public bool teleportKetikaSalah = true;
    public Transform titikTeleportSalah;
    public float delayTeleportSalah = 0.3f;

    [Header("Canvas Dimatikan Setelah Teleport")]
    public bool matikanCanvasSetelahTeleport = true;
    public GameObject canvasYangDimatikan;
    public float delayMatikanCanvas = 0.2f;

    [Header("Default Saat Canvas Aktif Lagi")]
    public bool resetKeEvaluasiSaatAktif = true;

    private bool sudahMenjawab = false;
    private bool buttonSudahSetup = false;

    private void Awake()
    {
        SetupButton();
    }

    private void OnEnable()
    {
        if (resetKeEvaluasiSaatAktif)
        {
            TampilkanEvaluasi();
        }
    }

    private void SetupButton()
    {
        if (buttonSudahSetup) return;
        buttonSudahSetup = true;

        if (buttonTampilkanInfografis != null)
        {
            buttonTampilkanInfografis.onClick.RemoveAllListeners();
            buttonTampilkanInfografis.onClick.AddListener(TampilkanInfografis);
        }

        if (buttonLanjutQuiz != null)
        {
            buttonLanjutQuiz.onClick.RemoveAllListeners();
            buttonLanjutQuiz.onClick.AddListener(TampilkanQuiz);
        }

        for (int i = 0; i < buttonJawaban.Length; i++)
        {
            if (buttonJawaban[i] != null)
            {
                int index = i;

                buttonJawaban[i].onClick.RemoveAllListeners();
                buttonJawaban[i].onClick.AddListener(() => CekJawaban(index));
            }
        }

        if (buttonLanjutSetelahBenar != null)
        {
            buttonLanjutSetelahBenar.onClick.RemoveAllListeners();
            buttonLanjutSetelahBenar.onClick.AddListener(LanjutSetelahBenar);
        }

        if (buttonLanjutSetelahSalah != null)
        {
            buttonLanjutSetelahSalah.onClick.RemoveAllListeners();
            buttonLanjutSetelahSalah.onClick.AddListener(LanjutSetelahSalah);
        }
    }

    public void TampilkanEvaluasi()
    {
        sudahMenjawab = false;

        SetPanel(panelBG, true);
        SetPanel(panelEvaluasi, true);
        SetPanel(panelInfografis, false);
        SetPanel(panelQuiz, false);
        SetPanel(panelJawabanBenar, false);
        SetPanel(panelJawabanSalah, false);

        Debug.Log("Canvas aktif lagi, default ke Panel Evaluasi");
    }

    public void TampilkanInfografis()
    {
        SetPanel(panelBG, true);
        SetPanel(panelEvaluasi, false);
        SetPanel(panelInfografis, true);
        SetPanel(panelQuiz, false);
        SetPanel(panelJawabanBenar, false);
        SetPanel(panelJawabanSalah, false);

        Debug.Log("Panel Infografis aktif");
    }

    public void TampilkanQuiz()
    {
        sudahMenjawab = false;

        SetPanel(panelBG, true);
        SetPanel(panelEvaluasi, false);
        SetPanel(panelInfografis, false);
        SetPanel(panelQuiz, true);
        SetPanel(panelJawabanBenar, false);
        SetPanel(panelJawabanSalah, false);

        Debug.Log("Panel Quiz aktif");
    }

    private void CekJawaban(int indexJawabanDipilih)
    {
        if (sudahMenjawab) return;

        sudahMenjawab = true;

        if (indexJawabanDipilih == indexJawabanBenar)
        {
            Debug.Log("Jawaban benar, tampil panel benar");
            TampilkanPanelBenar();
        }
        else
        {
            Debug.Log("Jawaban salah, tampil panel salah");
            TampilkanPanelSalah();
        }
    }

    private void TampilkanPanelBenar()
    {
        SetPanel(panelBG, true);
        SetPanel(panelEvaluasi, false);
        SetPanel(panelInfografis, false);
        SetPanel(panelQuiz, false);
        SetPanel(panelJawabanBenar, true);
        SetPanel(panelJawabanSalah, false);
    }

    private void TampilkanPanelSalah()
    {
        SetPanel(panelBG, true);
        SetPanel(panelEvaluasi, false);
        SetPanel(panelInfografis, false);
        SetPanel(panelQuiz, false);
        SetPanel(panelJawabanBenar, false);
        SetPanel(panelJawabanSalah, true);
    }

    private void LanjutSetelahBenar()
    {
        StartCoroutine(ProsesLanjutBenar());
    }

    private void LanjutSetelahSalah()
    {
        StartCoroutine(ProsesLanjutSalah());
    }

    private IEnumerator ProsesLanjutBenar()
    {
        NyalakanObject(objectDinyalakanKetikaBenar);
        MatikanObject(objectDimatikanKetikaBenar);

        TutupSemuaPanel();

        if (teleportKetikaBenar)
        {
            yield return StartCoroutine(TeleportKeTitik(titikTeleportBenar, delayTeleportBenar, "benar"));
        }

        yield return new WaitForSeconds(delayMatikanCanvas);

        MatikanCanvas();
    }

    private IEnumerator ProsesLanjutSalah()
    {
        NyalakanObject(objectDinyalakanKetikaSalah);
        MatikanObject(objectDimatikanKetikaSalah);

        TutupSemuaPanel();

        if (teleportKetikaSalah)
        {
            yield return StartCoroutine(TeleportKeTitik(titikTeleportSalah, delayTeleportSalah, "salah"));
        }

        yield return new WaitForSeconds(delayMatikanCanvas);

        MatikanCanvas();
    }

    private IEnumerator TeleportKeTitik(Transform titikTujuan, float delay, string jenis)
    {
        yield return new WaitForSeconds(delay);

        if (titikTujuan == null)
        {
            Debug.LogWarning("Titik teleport " + jenis + " belum dimasukkan!");
            yield break;
        }

        if (xrOrigin == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                xrOrigin = player;
        }

        CharacterController cc = null;
        if (xrOrigin != null)
        {
            cc = xrOrigin.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;
        }

        if (teleportProvider != null)
        {
            if (!teleportProvider.enabled)
                teleportProvider.enabled = true;

            UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest()
            {
                destinationPosition = titikTujuan.position,
                destinationRotation = titikTujuan.rotation
            };

            teleportProvider.QueueTeleportRequest(request);
        }
        else if (xrOrigin != null)
        {
            // Direct teleport for PC Player
            xrOrigin.transform.SetPositionAndRotation(titikTujuan.position, titikTujuan.rotation);
        }

        yield return new WaitForSeconds(0.2f);

        if (cc != null)
            cc.enabled = true;

        Debug.Log("Teleport jawaban " + jenis + " ke: " + titikTujuan.name);
    }

    private void TutupSemuaPanel()
    {
        SetPanel(panelBG, false);
        SetPanel(panelEvaluasi, false);
        SetPanel(panelInfografis, false);
        SetPanel(panelQuiz, false);
        SetPanel(panelJawabanBenar, false);
        SetPanel(panelJawabanSalah, false);

        Debug.Log("Semua panel ditutup, teleport diproses");
    }

    private void MatikanCanvas()
    {
        if (!matikanCanvasSetelahTeleport) return;

        if (canvasYangDimatikan != null)
        {
            canvasYangDimatikan.SetActive(false);
            Debug.Log("Canvas dimatikan setelah teleport");
        }
        else
        {
            gameObject.SetActive(false);
            Debug.Log("GameObject script dimatikan setelah teleport");
        }
    }

    private void SetPanel(GameObject panel, bool aktif)
    {
        if (panel != null)
            panel.SetActive(aktif);
    }

    private void NyalakanObject(GameObject[] daftarObject)
    {
        for (int i = 0; i < daftarObject.Length; i++)
        {
            if (daftarObject[i] != null)
                daftarObject[i].SetActive(true);
        }
    }

    private void MatikanObject(GameObject[] daftarObject)
    {
        for (int i = 0; i < daftarObject.Length; i++)
        {
            if (daftarObject[i] != null)
                daftarObject[i].SetActive(false);
        }
    }
}