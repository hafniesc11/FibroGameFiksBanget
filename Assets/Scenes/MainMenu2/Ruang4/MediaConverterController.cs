using UnityEngine;
using System.Collections;

public class MediaConverterController : MonoBehaviour{
    
    [Header("LED Renderer")]
    public Renderer powerLED;
    public Renderer foLED;
    public Renderer utpLED;
    public Renderer linkLED;
    public Renderer fxLED;
    public Renderer actLED;

    [Header("Material LED")]
    public Material ledOn;
    public Material ledOff;
    public GameFlowController gameFlow;

    private Coroutine linkBlink;
    private Coroutine fxBlink;
    private Coroutine actBlink;
    private bool foMasuk = false;
    private bool utpMasuk = false;

    void Start()
    {
        SemuaMati();
    }

    public enum ConverterType{
        Broken,
        Normal
    }

public ConverterType converterType;

    // =========================
    // KONDISI AWAL
    // =========================
    public void SemuaMati(){
        StopAllBlink();

        SetLED(powerLED, false);
        SetLED(foLED, false);
        SetLED(utpLED, false);
        SetLED(linkLED, false);
        SetLED(fxLED, false);
        SetLED(actLED, false);
    }

    // =========================
    // CONVERTER RUSAK
    // =========================
    public void ConverterRusak(){
        StopAllBlink();

        SetLED(powerLED, true);
        SetLED(foLED, true);
        SetLED(utpLED, true);

        SetLED(fxLED, false);
        SetLED(actLED, false);

        linkBlink = StartCoroutine(Blink(linkLED));
    }

   public void KabelFOMasuk(){
        Debug.Log("FO MASUK");
        foMasuk = true;

        SetLED(powerLED, true);
        SetLED(foLED, true);
    }

   public void KabelUTPMasuk(){
    utpMasuk = true;

    SetLED(utpLED, true);

    if (linkBlink == null)
        linkBlink = StartCoroutine(Blink(linkLED));

    if (converterType == ConverterType.Normal)
    {
        if (fxBlink == null)
            fxBlink = StartCoroutine(Blink(fxLED));

        if (actBlink == null)
            actBlink = StartCoroutine(Blink(actLED));
    }

    // CEK APAKAH SUDAH LENGKAP
    if (converterType == ConverterType.Normal && foMasuk && utpMasuk)
    {
        if (gameFlow != null)
        {
            gameFlow.PerbaikanJaringanSelesai();
        }
    }
}

public void KabelFOKeluar(){
    Debug.Log("FO KELUAR");
    foMasuk = false;

    SetLED(powerLED, false);
    SetLED(foLED, false);
}

public void KabelUTPKeluar(){
    Debug.Log("UTP KELUAR");
    utpMasuk = false;

    SetLED(utpLED, false);

    if (linkBlink != null)
    {
        StopCoroutine(linkBlink);
        linkBlink = null;
    }

    SetLED(linkLED, false);
}
    void CekKabel(){
        if (foMasuk && utpMasuk)
        {
            ConverterRusak();
        }
    }

    // =========================
    // CONVERTER NORMAL
    // =========================
    public void ConverterNormal()
    {
        StopAllBlink();

        SetLED(powerLED, true);
        SetLED(foLED, true);
        SetLED(utpLED, true);

        linkBlink = StartCoroutine(Blink(linkLED));
        fxBlink = StartCoroutine(Blink(fxLED));
        actBlink = StartCoroutine(Blink(actLED));
    }

    void SetLED(Renderer led, bool on)
    {
        led.material = on ? ledOn : ledOff;
    }

    IEnumerator Blink(Renderer led)
    {
        while (true)
        {
            led.material = ledOn;
            yield return new WaitForSeconds(0.5f);

            led.material = ledOff;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void StopAllBlink()
    {
        if (linkBlink != null) StopCoroutine(linkBlink);
        if (fxBlink != null) StopCoroutine(fxBlink);
        if (actBlink != null) StopCoroutine(actBlink);
    }
}