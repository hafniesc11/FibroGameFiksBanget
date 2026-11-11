using UnityEngine;

public class CableInfoManager : MonoBehaviour
{
    public static CableInfoManager Instance;

    [Header("Canvas")]
    public GameObject canvasCableInfo;

    [Header("Panel Kabel")]
    public GameObject cnTembaga;
    public GameObject cnFO;

    private void Awake()
    {
        Instance = this;

        canvasCableInfo.SetActive(false);
        cnTembaga.SetActive(false);
        cnFO.SetActive(false);
    }

    public void ShowCableInfo()
    {
        canvasCableInfo.SetActive(true);

        if (cablemanager.Instance.selectedCable == cablemanager.CableType.Fiber)
        {
            cnFO.SetActive(true);
            cnTembaga.SetActive(false);
        }
        else if (cablemanager.Instance.selectedCable == cablemanager.CableType.Copper)
        {
            cnTembaga.SetActive(true);
            cnFO.SetActive(false);
        }
    }

    public void CloseCableInfo()
    {
        canvasCableInfo.SetActive(false);
        cnFO.SetActive(false);
        cnTembaga.SetActive(false);
    }

    
}