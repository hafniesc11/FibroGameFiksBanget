using UnityEngine;
using System.Collections;

public class ComputerController : MonoBehaviour{
    public GameObject powerCanvas;
    public GameObject windowsCanvas;
    public GameObject errorCanvas;
    public GameObject analysisCanvas;
    public GameObject objectiveCanvas;
    // public GameObject alarmObject;
    public GameObject browserNormalCanvas;

    public GameFlowController gameFlow;
    public float analysisDelay = 2f;
    public bool jaringanNormal = false;

    public void TurnOnComputer(){
    Debug.Log("BUTTON POWER DIKLIK");

    powerCanvas.SetActive(false);
    Debug.Log("Power OFF");

    windowsCanvas.SetActive(true);
    Debug.Log("Windows ON");

    Debug.Log(windowsCanvas.activeSelf);
}

   public void OpenChrome()
{
    windowsCanvas.SetActive(false);

    errorCanvas.SetActive(true);

    if (!jaringanNormal)
    {
        StartCoroutine(ShowAnalysis());
    }
}

    public void StartAnalysis(){
        analysisCanvas.SetActive(false);
        objectiveCanvas.SetActive(true);
        // alarmObject.SetActive(true);
}

    IEnumerator ShowAnalysis(){
        yield return new WaitForSeconds(analysisDelay);
        analysisCanvas.SetActive(true);
    }

    public void RefreshBrowser(){
    if (!jaringanNormal)
    {
        Debug.Log("Jaringan masih bermasalah.");

        // Tetap di canvas error
        return;
    }

    errorCanvas.SetActive(false);

    browserNormalCanvas.SetActive(true);

    if(gameFlow != null)
    {
        gameFlow.UjiJaringanSelesai();
    }
}
}