using UnityEngine;
using System.Collections;

public class GameFlowController : MonoBehaviour{
    public ComputerController computerController;
    [Header("Canvas")]
    public GameObject analysisCanvas;
    public GameObject objectiveCanvas;
    public GameObject serverCanvas;
    public GameObject InOTDRCanvas; //instruksi otdr ketika di grab
    public GameObject InstruksiScanCanvas; //untuk instruksi scan odtr deket dengan toolbox
    public GameObject letakkanOTDRCanvas; // "Letakkan OTDR ke Port"
    public GameObject scanningCanvas; // Animasi/loading scanning
    public GameObject hasilOTDRCanvas; // Hasil OTDR
    // public GameObject inspectConverterCanvas;
   

    [Header("Panel Server")]
    public GameObject panel1;
    public GameObject panel2;
    
    [Header("Objective")]
    public GameObject objective1Check;
    public GameObject objective2Check;
    public GameObject objective3Check;
    public GameObject objective4Check;
    public GameObject objective5Check;
    public GameObject objective6Check;
    public GameObject objective7Check;

    [Header("Alarm")]
    public AudioSource alarmSound;

    bool serverTriggered = false;
    bool perbaikanSelesai = false;
    bool ujiJaringanSelesai = false;

    public void StartAnalysis(){
        analysisCanvas.SetActive(false);
        objectiveCanvas.SetActive(true);
    }

    public void ReachServer(){
        if (serverTriggered) return;
        serverTriggered = true;
        alarmSound.Play();
        objective1Check.SetActive(true);
        serverCanvas.SetActive(true);
        panel1.SetActive(true);
        panel2.SetActive(false);
    }

    public void OTDRPicked(){
        objective2Check.SetActive(true);
        InOTDRCanvas.SetActive(true);
        Debug.Log("OTDR berhasil diambil.");
    }

    public void StartScanInstruction(){
        // Hilangkan instruksi bawa OTDR
        InOTDRCanvas.SetActive(false);

        // Tampilkan instruksi scan
        InstruksiScanCanvas.SetActive(true);
    }

    public void StartOTDRScan(){
        objective3Check.SetActive(true); // Objective 3 selesai karena OTDR sudah dipasang ke port
        letakkanOTDRCanvas.SetActive(false);
        scanningCanvas.SetActive(true);
        StartCoroutine(ScanProcess());
    }

    IEnumerator ScanProcess(){
        yield return new WaitForSeconds(10f);
        scanningCanvas.SetActive(false);
        hasilOTDRCanvas.SetActive(true);
        objective3Check.SetActive(true);
        objective4Check.SetActive(true);   // Objective 4 selesai karena hasil scan sudah tampil
    }

    public void FoundProblemArea(){
        objective5Check.SetActive(true); //objective 5 selesai karena area problem sudah ditemukan [mediaconveter]
        // inspectConverterCanvas.SetActive(true);
    }

    public void PerbaikanJaringanSelesai()
{
    if (perbaikanSelesai) return;

    perbaikanSelesai = true;

    objective6Check.SetActive(true);
    alarmSound.Stop();

    computerController.jaringanNormal = true;

    Debug.Log("Perbaikan jaringan selesai.");
}

    public void UjiJaringanSelesai()
{
    if (ujiJaringanSelesai) return;

    ujiJaringanSelesai = true;

    objective7Check.SetActive(true);

    Debug.Log("Jaringan berhasil diuji.");
}   
}