using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OTDRController : MonoBehaviour
{
    public GameFlowController gameFlow;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private bool sudahDiambil = false;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrab);
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (sudahDiambil) return;

        sudahDiambil = true;

        gameFlow.OTDRPicked();
    }
}