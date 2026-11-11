using UnityEngine;

public class WelcomeCanvas : MonoBehaviour
{
    public GameObject canvasSelamat;
    public GameObject canvasResult;
    public GameObject resultCopper;
    public GameObject resultFiber;

    public GameObject canvasCableInfo;
    public GameObject canvasCopper;
    public GameObject canvasFiber;

    public GameObject canvasEnd;

    [Header("Reset Simulasi")]
    public PodMovement podMovement;
    public TrafficPodSpawner trafficPodSpawner;
    public DisturbTrigger disturbTrigger;

    [Header("Teleport")]
    public GameObject xrOrigin;
    public Transform startPoint;

    public TunnelPortal portalFiber;
    public TunnelPortal portalCopper;

 public void Lanjut()
{
    Debug.Log("Button Seru diklik");
    canvasSelamat.SetActive(false);

    canvasResult.SetActive(true);

    if (cablemanager.Instance.selectedCable == cablemanager.CableType.Copper)
    {
        resultCopper.SetActive(true);
        resultFiber.SetActive(false);
    }
    else
    {
        resultFiber.SetActive(true);
        resultCopper.SetActive(false);
    }
}
    public void NextResult()
{
    resultCopper.SetActive(false);
    resultFiber.SetActive(false);

    canvasResult.SetActive(false);

    canvasCableInfo.SetActive(true);

    if (cablemanager.Instance.selectedCable == cablemanager.CableType.Copper)
    {
        canvasCopper.SetActive(true);
        canvasFiber.SetActive(false);
    }
    else
    {
        canvasFiber.SetActive(true);
        canvasCopper.SetActive(false);
    }
}

public void NextToEnd()
{
    canvasEnd.SetActive(true);
}

public void BackToResult()
{
    canvasCableInfo.SetActive(false);

    canvasCopper.SetActive(false);
    canvasFiber.SetActive(false);

    canvasResult.SetActive(true);

    if (cablemanager.Instance.selectedCable == cablemanager.CableType.Copper)
    {
        resultCopper.SetActive(true);
        resultFiber.SetActive(false);
    }
    else
    {
        resultFiber.SetActive(true);
        resultCopper.SetActive(false);
    }
}

   public void CobaJalurLain()
{
    canvasEnd.SetActive(false);

    canvasCableInfo.SetActive(false);

    canvasCopper.SetActive(false);
    canvasFiber.SetActive(false);

    // reset mobil
    if (podMovement != null)
        podMovement.ResetPod();
    if (portalFiber != null && portalFiber.podMovement != null)
        portalFiber.podMovement.ResetPod();
    if (portalCopper != null && portalCopper.podMovement != null)
        portalCopper.podMovement.ResetPod();

    // reset traffic
    if (trafficPodSpawner != null)
        trafficPodSpawner.ResetSpawner();

    // reset disturb
    if (disturbTrigger != null)
        disturbTrigger.ResetDisturb();

    // bersihkan tumpukan kardus di ruang server
    if (DeliveredCargoDisplay.Instance != null)
        DeliveredCargoDisplay.Instance.ClearDisplay();

    if (xrOrigin == null)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            xrOrigin = player;
    }

    if (xrOrigin != null && startPoint != null)
    {
        // Lepas parent dari mobil jika masih nempel
        xrOrigin.transform.SetParent(null);

        // teleport player
        CharacterController cc = xrOrigin.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        xrOrigin.transform.SetPositionAndRotation(
            startPoint.position,
            startPoint.rotation
        );

        if (cc != null)
            cc.enabled = true;

        PCPlayerController pc = xrOrigin.GetComponent<PCPlayerController>();
        if (pc != null)
            pc.enabled = true;
    }
}
}