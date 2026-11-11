using UnityEngine;

public class ServerTrigger : MonoBehaviour
{
    public GameFlowController gameFlow;
    public BlinkLight blinkLight;

    private bool sudahMasuk = false;

   private void OnTriggerEnter(Collider other)
{
    Debug.Log("Masuk Trigger : " + other.name);

    if (sudahMasuk) return;

    if (other.CompareTag("Player"))
    {
        sudahMasuk = true;

        blinkLight.StartBlink();

        gameFlow.ReachServer();
    }
}
}