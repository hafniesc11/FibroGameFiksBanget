using UnityEngine;

public class ExperienceCanvasManager : MonoBehaviour
{
    public static ExperienceCanvasManager Instance;

    public GameObject canvasExperience;

    public GameObject panelFiber;
    public GameObject panelCopper;

    private TunnelPortal currentPortal;

    private void Awake()
    {
        Instance = this;

        if (canvasExperience != null) canvasExperience.SetActive(false);
        if (panelFiber != null) panelFiber.SetActive(false);
        if (panelCopper != null) panelCopper.SetActive(false);
    }

    public void Show(TunnelPortal portal)
    {
        currentPortal = portal;

        if (canvasExperience != null) canvasExperience.SetActive(true);

        if (cablemanager.Instance != null && cablemanager.Instance.selectedCable == cablemanager.CableType.Fiber)
        {
            if (panelFiber != null) panelFiber.SetActive(true);
            if (panelCopper != null) panelCopper.SetActive(false);
        }
        else
        {
            if (panelCopper != null) panelCopper.SetActive(true);
            if (panelFiber != null) panelFiber.SetActive(false);
        }
    }

    private void Update()
    {
        // Jika UI Experience sedang aktif, pemain juga bisa menekan tombol Space / Enter / E untuk mulai
        if (canvasExperience != null && canvasExperience.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                ButtonOke();
            }
        }
    }

    public void ButtonOke()
    {
        if (canvasExperience != null) canvasExperience.SetActive(false);

        if (panelFiber != null) panelFiber.SetActive(false);
        if (panelCopper != null) panelCopper.SetActive(false);

        if (currentPortal != null)
            currentPortal.MulaiPerjalanan();
    }
}