using UnityEngine;
using System.Collections;

public class TunnelPortal : MonoBehaviour
{
    [Header("Teleport Provider")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportProvider;

    [Header("XR Origin / Player")]
    public GameObject xrOrigin;

    [Header("Portal Ini")]
    public PortalType portalType;

    [Header("Titik Duduk di Mobil")]
    public Transform seatPoint;

    [Header("Mobil / Pod Movement")]
    public PodMovement podMovement;

    [Header("Traffic Spawner")]
    public TrafficPodSpawner trafficPodSpawner;

    [Header("Object Yang Dimatikan Setelah Teleport")]
    public GameObject[] objectYangDimatikan;

    [Header("Komponen Movement Yang Dimatikan")]
    public Behaviour[] komponenYangDimatikan;

    [Header("Delay Setelah Teleport")]
    public float delaySetelahTeleport = 0.3f;

    [Header("Portal Bisa Dipakai Ulang")]
    public bool bisaDipakaiUlang = true;
    public float cooldownPortal = 1f;

    [Header("Disturb Trigger")]
    public DisturbTrigger disturbTrigger;

    private bool sedangProses = false;

    private void OnTriggerEnter(Collider other)
    {
        if (sedangProses) return;

        Debug.Log("Trigger kena oleh: " + other.name + " | Root: " + other.transform.root.name);

        bool isPlayer = other.transform.root.CompareTag("Player") 
                     || other.CompareTag("Player") 
                     || other.name.Contains("PCPlayer") 
                     || other.transform.root.name.Contains("PCPlayer")
                     || other.GetComponentInParent<PCPlayerController>() != null
                     || other.GetComponentInParent<CharacterController>() != null;

        if (isPlayer)
        {
            sedangProses = true;
            
            PCPlayerController pc = other.GetComponentInParent<PCPlayerController>();
            if (pc != null)
                xrOrigin = pc.gameObject;
            else
                xrOrigin = other.transform.root.gameObject;

            if (portalType == PortalType.Copper)
                cablemanager.Instance.selectedCable = cablemanager.CableType.Copper;
            else
                cablemanager.Instance.selectedCable = cablemanager.CableType.Fiber;

            Debug.Log("Player memilih portal: " + portalType);
            Debug.Log("Player terdeteksi, memposisikan kamera di atas mobil...");

            LakukanTeleportKeMobil();

            if (bisaDipakaiUlang)
                StartCoroutine(ResetPortalCooldown());
        }
        else
        {
            Debug.Log("Bukan Player. Root = " + other.transform.root.name);
        }
    }

    private IEnumerator ResetPortalCooldown()
    {
        yield return new WaitForSeconds(cooldownPortal);
        sedangProses = false;
    }

    private void LakukanTeleportKeMobil()
    {
        if (xrOrigin == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                xrOrigin = player;
        }

        if (xrOrigin == null)
        {
            Debug.LogWarning("Player / XR Origin belum dimasukkan!");
            return;
        }

        // Reset mobil ke awal
        if (podMovement != null)
        {
            podMovement.ResetPod();
        }

        if (trafficPodSpawner != null)
        {
            trafficPodSpawner.isFiberOptic = (portalType == PortalType.Fiber);
            trafficPodSpawner.ResetSpawner();
        }

        if (disturbTrigger != null)
        {
            disturbTrigger.ResetDisturb();
        }

        // Matikan CharacterController & Player Controller saat di mobil
        CharacterController cc = xrOrigin.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        PCPlayerController pc = xrOrigin.GetComponent<PCPlayerController>();
        if (pc != null)
            pc.enabled = false;

        // Posisikan kamera di atas mobil
        xrOrigin.transform.SetParent(null);
        xrOrigin.transform.localScale = Vector3.one;

        if (podMovement != null)
        {
            xrOrigin.transform.position = podMovement.GetCameraPosition(xrOrigin);
            Vector3 forward = podMovement.transform.forward;
            forward.y = 0;
            if (forward != Vector3.zero)
                xrOrigin.transform.rotation = Quaternion.LookRotation(forward);
        }

        // Aktifkan visualisasi pantulan berkas sinar ZIGZAG pada partikel kotak biru (Pertemuan 3)
        if (portalType == PortalType.Fiber)
        {
            MultimodeZigzagLightFlow flow = FindFirstObjectByType<MultimodeZigzagLightFlow>();
            if (flow == null)
            {
                GameObject tm = GameObject.Find("TunnelMultimode");
                if (tm != null)
                {
                    flow = tm.AddComponent<MultimodeZigzagLightFlow>();
                }
                else
                {
                    GameObject flowObj = new GameObject("Multimode_ZigzagFlow_Controller");
                    flow = flowObj.AddComponent<MultimodeZigzagLightFlow>();
                }
            }

            if (flow != null)
            {
                flow.ApplyZigzagToParticleSystems();
            }
        }
        else if (seatPoint != null)
        {
            xrOrigin.transform.position = seatPoint.position + Vector3.up * 1f;
        }

        // Reset sudut kamera agar lurus ke depan terowongan (tidak menunduk)
        MouseLook ml = xrOrigin.GetComponentInChildren<MouseLook>();
        if (ml != null)
        {
            ml.ResetView(0f);
        }

        StartCoroutine(MulaiMobilSetelahTeleport());
    }

    private IEnumerator MulaiMobilSetelahTeleport()
    {
        yield return new WaitForSeconds(delaySetelahTeleport);

        MatikanObjectSetelahTeleport();
        MatikanKomponenMovement();

        if (podMovement != null)
        {
            podMovement.SetPassenger(xrOrigin, seatPoint);

            if (ExperienceCanvasManager.Instance != null)
            {
                ExperienceCanvasManager.Instance.Show(this);
            }
            Debug.Log("Player sudah di mobil. Menunggu klik Oke.");
        }
    }

    private void MatikanObjectSetelahTeleport()
    {
        for (int i = 0; i < objectYangDimatikan.Length; i++)
        {
            if (objectYangDimatikan[i] != null)
                objectYangDimatikan[i].SetActive(false);
        }
    }

    private void MatikanKomponenMovement()
    {
        for (int i = 0; i < komponenYangDimatikan.Length; i++)
        {
            if (komponenYangDimatikan[i] != null)
                komponenYangDimatikan[i].enabled = false;
        }

        if (xrOrigin != null)
        {
            PCPlayerController pc = xrOrigin.GetComponent<PCPlayerController>();
            if (pc != null)
                pc.enabled = false;
        }
    }

    public void MulaiPerjalanan()
    {
        if (trafficPodSpawner != null)
        {
            trafficPodSpawner.isFiberOptic = (portalType == PortalType.Fiber);
            trafficPodSpawner.StartSpawn();
        }

        if (podMovement != null)
            podMovement.StartMove();

        Debug.Log("Mobil dan konvoi traffic mulai berjalan bersamaan setelah klik Oke.");
    }

    public enum PortalType
    {
        Copper,
        Fiber
    }

    public void ResetPortal()
    {
        sedangProses = false;
    }
}