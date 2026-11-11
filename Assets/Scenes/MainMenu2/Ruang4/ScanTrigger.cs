using UnityEngine;

public class ScanTrigger : MonoBehaviour{
    public GameFlowController gameFlow;
    private bool sudahMasuk = false;

    private void OnTriggerEnter(Collider other)
    {
        if (sudahMasuk) return;

        if (other.CompareTag("Player"))
        {
            sudahMasuk = true;

            gameFlow.StartScanInstruction();
        }
    }
}