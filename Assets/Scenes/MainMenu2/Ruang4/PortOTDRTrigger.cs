using UnityEngine;

public class PortOTDRTrigger : MonoBehaviour
{
    public GameFlowController gameFlow;

    private bool sudahScan = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("MASUK TRIGGER : " + other.name);

        if (sudahScan) return;

      if (!other.transform.root.CompareTag("OTDR"))
            return;

        sudahScan = true;

        gameFlow.StartOTDRScan();
    }
}