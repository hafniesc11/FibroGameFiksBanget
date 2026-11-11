using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Pengontrol Interaksi PC Universal Berbasis MURNI CROSSHAIR (FPS Game Style).
/// Mendukung seluruh scene (Pertemuan 1, 2, 3, dan 4):
/// 1. Interaksi Kuis (Pilihan A-E, Input Nama & Kelas).
/// 2. Interaksi Alat Meja & Fiber Optik (Stripper, Cleaver, Splicer, OPM, Visual Fault Locator).
/// 3. Interaksi Ruang Server & Trouble Ticket Pertemuan 4 (OTDR, Kabel UTP/RJ45, Fiber Box, Media Converter, Komputer Layar, Server Rack).
/// 4. Sistem Pegang Alat (isTrigger tetap aktif agar dapat memicu port scan OTDR & snap kabel RJ45).
/// </summary>
public class PCInteractionController : MonoBehaviour
{
    public static PCInteractionController Instance;
    public static bool isTypingText = false;

    [Header("Kamera & Jangkauan Interaksi")]
    public Camera playerCamera;
    public float uiInteractionDistance = 15.0f;
    public float objectInteractionDistance = 15.0f;
    public float raycastRadius = 0.2f;

    [Header("Layer Mask (Objek 3D)")]
    public LayerMask interactableLayers = ~0;

    [Header("Visual Crosshair (Titik Tengah Layar)")]
    public bool showCrosshair = true;
    public float crosshairSize = 8f;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.2f, 1f, 0.4f, 1f);

    [Header("Sistem Pegang Objek (PC Grab / Hold)")]
    public bool enableObjectPickup = true;
    public float holdDistance = 1.25f;
    public float holdVerticalOffset = -0.36f;
    public float smoothHoldSpeed = 18f;

    // Status Objek yang Sedang Disorot
    private Button hoveredButton;
    private TMP_InputField hoveredTMPInput;
    private InputField hoveredInput;
    private GameObject hoveredUIObj;
    private Component hoveredXRObject;
    private PCInteractable hoveredPCInteractable;
    private InfoKabelFO hoveredInfoKabel;
    private FiberBoxController hoveredFiberBox;
    private ComputerController hoveredComputer;
    private Rigidbody hoveredRigidbody;
    private string customPromptMessage = "";
    private bool isHovering = false;

    // Objek yang Sedang Dipegang
    private Rigidbody heldRigidbody;
    private Transform heldTransform;
    private Transform originalParent;
    private Vector3 initialWorldPosition;
    private Quaternion initialWorldRotation;
    private Quaternion initialRelativeRotation;
    private Collider[] heldColliders;
    private Transform holdPoint;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable heldXRInteractable;
    private List<GameObject> activeExplanationCanvases = new List<GameObject>();

    // Input Text Box Aktif
    private TMP_InputField activeTMPInput;
    private InputField activeStdInput;

    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private Texture2D crosshairTex;
    private GUIStyle promptStyle;
    private bool fiberBoxIsOpen = false;

    void Awake()
    {
        Instance = this;
        isTypingText = false;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
        {
            if (playerCamera.nearClipPlane > 0.05f)
            {
                playerCamera.nearClipPlane = 0.05f;
            }

            GameObject hp = new GameObject("PCHoldPoint");
            hp.transform.SetParent(playerCamera.transform);
            hp.transform.localPosition = new Vector3(0f, holdVerticalOffset, holdDistance);
            hp.transform.localRotation = Quaternion.identity;
            holdPoint = hp.transform;
        }

        crosshairTex = new Texture2D(1, 1);
        crosshairTex.SetPixel(0, 0, Color.white);
        crosshairTex.Apply();

        // Kunci kursor Windows agar selalu dalam mode FPS First-Person murni
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        EnsureEventSystemAndCanvases();
        FixAndLockAllTableToolsOnStart();
    }

    void FixAndLockAllTableToolsOnStart()
    {
        // 1. Objek XR Grab Interactable
        var allGrabInteractables = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>(FindObjectsSortMode.None);
        foreach (var grab in allGrabInteractables)
        {
            Rigidbody rb = grab.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            EnsurePreciseCollidersOnMeshes(grab.gameObject);
        }

        // 2. Info Kabel
        var allInfoKabels = FindObjectsByType<InfoKabelFO>(FindObjectsSortMode.None);
        foreach (var ik in allInfoKabels)
        {
            Rigidbody rb = ik.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            EnsurePreciseCollidersOnMeshes(ik.gameObject);
        }

        // 3. Objek Pertemuan 4 (OTDR, FiberBox, MediaConverter, Computer, RJ45)
        var allOTDRs = FindObjectsByType<OTDRController>(FindObjectsSortMode.None);
        foreach (var otdr in allOTDRs)
        {
            EnsurePreciseCollidersOnMeshes(otdr.gameObject);
        }

        var allFiberBoxes = FindObjectsByType<FiberBoxController>(FindObjectsSortMode.None);
        foreach (var fb in allFiberBoxes)
        {
            EnsurePreciseCollidersOnMeshes(fb.gameObject);
        }

        var allComputers = FindObjectsByType<ComputerController>(FindObjectsSortMode.None);
        foreach (var comp in allComputers)
        {
            EnsurePreciseCollidersOnMeshes(comp.gameObject);
        }

        // 4. Pastikan objek ber-tag OTDR dan UTP memiliki collider
        GameObject[] otdrTags = GameObject.FindGameObjectsWithTag("OTDR");
        foreach (var go in otdrTags) EnsurePreciseCollidersOnMeshes(go);

        GameObject[] utpTags = GameObject.FindGameObjectsWithTag("UTP");
        foreach (var go in utpTags) EnsurePreciseCollidersOnMeshes(go);
    }

    void EnsurePreciseCollidersOnMeshes(GameObject target)
    {
        if (target == null) return;

        Collider rootCol = target.GetComponent<Collider>();
        if (rootCol == null)
        {
            MeshFilter rootMf = target.GetComponent<MeshFilter>();
            if (rootMf != null && rootMf.sharedMesh != null)
            {
                MeshCollider mc = target.AddComponent<MeshCollider>();
                mc.sharedMesh = rootMf.sharedMesh;
                mc.convex = true;
            }
            else
            {
                target.AddComponent<BoxCollider>();
            }
        }

        MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in renderers)
        {
            if (mr.GetComponent<Collider>() == null)
            {
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    MeshCollider mc = mr.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = true;
                }
                else
                {
                    mr.gameObject.AddComponent<BoxCollider>();
                }
            }
        }
    }

    void Update()
    {
        // A. Penanganan Khusus Saat Sedang Mengetik Teks (Nama / Kelas)
        if (isTypingText)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.sendNavigationEvents = false;
            }

            // Tekan Enter, Tab, atau Escape untuk menyelesaikan ketikan
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
            {
                FinishTyping();
                return;
            }

            // Jika mengklik dengan mouse ke tombol lain (misal tombol Mulai Kuis / kotak kelas)
            if (Input.GetMouseButtonDown(0))
            {
                DetectUnderCrosshair();
                if (hoveredButton != null || (hoveredTMPInput != null && hoveredTMPInput != activeTMPInput))
                {
                    FinishTyping();
                    TriggerInteraction();
                    return;
                }
            }
            return;
        }

        // B. Jika sedang memegang objek 3D di depan kamera
        if (heldTransform != null)
        {
            HandleHoldingObject();

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q))
            {
                DropHeldObject();
            }
            return;
        }

        // C. Deteksi Objek / UI di bawah titik crosshair
        DetectUnderCrosshair();

        // D. Eksekusi interaksi saat tombol Klik Kiri atau E ditekan
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            TriggerInteraction();
        }
    }

    void FinishTyping()
    {
        isTypingText = false;
        if (activeTMPInput != null)
        {
            activeTMPInput.DeactivateInputField();
            activeTMPInput = null;
        }
        if (activeStdInput != null)
        {
            activeStdInput.DeactivateInputField();
            activeStdInput = null;
        }
    }

    void DetectUnderCrosshair()
    {
        hoveredButton = null;
        hoveredTMPInput = null;
        hoveredInput = null;
        hoveredUIObj = null;
        hoveredXRObject = null;
        hoveredPCInteractable = null;
        hoveredInfoKabel = null;
        hoveredFiberBox = null;
        hoveredComputer = null;
        hoveredRigidbody = null;
        customPromptMessage = "";
        isHovering = false;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        // A. Raycast ke UI Canvas di tengah layar (Soal Kuis, Tombol Pilihan A-E, Input Nama/Kelas, Layar Komputer)
        if (EventSystem.current != null)
        {
            if (pointerEventData == null)
                pointerEventData = new PointerEventData(EventSystem.current);

            pointerEventData.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            foreach (var result in raycastResults)
            {
                if (result.distance <= uiInteractionDistance || result.distance == 0)
                {
                    Button btn = result.gameObject.GetComponentInParent<Button>();
                    if (btn != null && btn.interactable && btn.gameObject.activeInHierarchy)
                    {
                        hoveredButton = btn;
                        hoveredUIObj = result.gameObject;
                        
                        TMPro.TMP_Text btnTxt = btn.GetComponentInChildren<TMPro.TMP_Text>();
                        Text stdTxt = btn.GetComponentInChildren<Text>();
                        string label = btnTxt != null && !string.IsNullOrEmpty(btnTxt.text) ? btnTxt.text : (stdTxt != null && !string.IsNullOrEmpty(stdTxt.text) ? stdTxt.text : "Pilih");
                        customPromptMessage = $"[Klik / E] {label}";
                        isHovering = true;
                        return;
                    }

                    TMP_InputField tmpInput = result.gameObject.GetComponentInParent<TMP_InputField>();
                    if (tmpInput != null && tmpInput.interactable && tmpInput.gameObject.activeInHierarchy)
                    {
                        hoveredTMPInput = tmpInput;
                        hoveredUIObj = result.gameObject;
                        customPromptMessage = "[Klik / E] Ketik Teks";
                        isHovering = true;
                        return;
                    }

                    InputField stdInput = result.gameObject.GetComponentInParent<InputField>();
                    if (stdInput != null && stdInput.interactable && stdInput.gameObject.activeInHierarchy)
                    {
                        hoveredInput = stdInput;
                        hoveredUIObj = result.gameObject;
                        customPromptMessage = "[Klik / E] Ketik Teks";
                        isHovering = true;
                        return;
                    }
                }
            }
        }

        // B. Raycast & SphereCast ke Objek Fisika 3D
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit, objectInteractionDistance, interactableLayers);

        if (!hasHit)
        {
            hasHit = Physics.SphereCast(ray, raycastRadius, out hit, objectInteractionDistance, interactableLayers);
        }

        if (hasHit && hit.collider != null)
        {
            GameObject hitObj = hit.collider.gameObject;

            // 1. Cek Tombol UI dalam Canvas 3D
            Button colBtn = hitObj.GetComponentInParent<Button>();
            if (colBtn == null) colBtn = hitObj.GetComponentInChildren<Button>();
            if (colBtn != null && colBtn.interactable && colBtn.gameObject.activeInHierarchy)
            {
                hoveredButton = colBtn;
                hoveredUIObj = colBtn.gameObject;
                customPromptMessage = "[Klik / E] Pilih";
                isHovering = true;
                return;
            }

            // 2. Cek Komponen FiberBox (Pertemuan 4)
            FiberBoxController fb = hitObj.GetComponentInParent<FiberBoxController>();
            if (fb == null) fb = hitObj.GetComponentInChildren<FiberBoxController>();
            if (fb != null)
            {
                hoveredFiberBox = fb;
                hoveredUIObj = fb.gameObject;
                customPromptMessage = fiberBoxIsOpen ? "[Klik / E] Tutup Fiber Box" : "[Klik / E] Buka Fiber Box";
                isHovering = true;
                return;
            }

            // 3. Cek Komponen Komputer (Pertemuan 4)
            ComputerController comp = hitObj.GetComponentInParent<ComputerController>();
            if (comp == null) comp = hitObj.GetComponentInChildren<ComputerController>();
            if (comp != null)
            {
                hoveredComputer = comp;
                hoveredUIObj = comp.gameObject;
                customPromptMessage = "[Klik / E] Nyalakan PC / Gunakan Komputer";
                isHovering = true;
                return;
            }

            // 4. Cek Komponen InfoKabelFO
            InfoKabelFO infoKabel = hitObj.GetComponentInParent<InfoKabelFO>();
            if (infoKabel == null) infoKabel = hitObj.GetComponentInChildren<InfoKabelFO>(true);
            if (infoKabel != null)
            {
                hoveredInfoKabel = infoKabel;
                hoveredUIObj = hitObj;
                customPromptMessage = $"[Klik / E] Ambil & Pelajari: {CleanObjectName(infoKabel.gameObject.name)}";
                isHovering = true;
                return;
            }

            // 5. Cek Komponen PCInteractable
            PCInteractable pcInt = hitObj.GetComponentInParent<PCInteractable>();
            if (pcInt != null && pcInt.enabled)
            {
                hoveredPCInteractable = pcInt;
                customPromptMessage = !string.IsNullOrEmpty(pcInt.interactionMessage) ? pcInt.interactionMessage : $"[Klik / E] {CleanObjectName(pcInt.gameObject.name)}";
                isHovering = true;
                return;
            }

            // 6. Cek Objek OTDR Controller / Tag OTDR (Pertemuan 4)
            OTDRController otdr = hitObj.GetComponentInParent<OTDRController>();
            if (otdr == null) otdr = hitObj.GetComponentInChildren<OTDRController>();
            if (otdr != null || hitObj.CompareTag("OTDR") || (hitObj.transform.root != null && hitObj.transform.root.CompareTag("OTDR")))
            {
                GameObject otdrObj = otdr != null ? otdr.gameObject : (hitObj.CompareTag("OTDR") ? hitObj : hitObj.transform.root.gameObject);
                hoveredXRObject = otdrObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
                hoveredUIObj = otdrObj;
                hoveredRigidbody = otdrObj.GetComponent<Rigidbody>();
                customPromptMessage = "[Klik / E] Ambil OTDR";
                isHovering = true;
                return;
            }

            // 7. Cek Objek Kabel RJ45 / Tag UTP (Pertemuan 4)
            if (hitObj.CompareTag("UTP") || (hitObj.transform.root != null && hitObj.transform.root.CompareTag("UTP")) || hitObj.name.Contains("RJ45") || hitObj.name.Contains("Kabel"))
            {
                GameObject rjObj = hitObj.CompareTag("UTP") ? hitObj : (hitObj.transform.root.CompareTag("UTP") ? hitObj.transform.root.gameObject : hitObj);
                hoveredXRObject = rjObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
                hoveredUIObj = rjObj;
                hoveredRigidbody = rjObj.GetComponent<Rigidbody>();
                customPromptMessage = $"[Klik / E] Ambil: {CleanObjectName(rjObj.name)}";
                isHovering = true;
                return;
            }

            // 8. Cek Objek XR Grab / Base Interactable (Stripper, OPM, Cleaver, Splicer, dll)
            var xrBase = hitObj.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
            if (xrBase != null)
            {
                hoveredXRObject = xrBase;
                hoveredRigidbody = xrBase.GetComponent<Rigidbody>();
                customPromptMessage = $"[Klik / E] Ambil: {CleanObjectName(xrBase.gameObject.name)}";
                isHovering = true;
                return;
            }

            // 9. Cek Skrip Interaksi Khusus seperti DetachPart, DoorController, dll
            DetachPart dp = hitObj.GetComponentInParent<DetachPart>();
            if (dp != null)
            {
                hoveredRigidbody = hitObj.GetComponentInParent<Rigidbody>();
                hoveredUIObj = dp.gameObject;
                customPromptMessage = $"[Klik / E] Lepas: {CleanObjectName(dp.gameObject.name)}";
                isHovering = true;
                return;
            }

            DoorController dc = hitObj.GetComponentInParent<DoorController>();
            if (dc != null)
            {
                hoveredUIObj = dc.gameObject;
                customPromptMessage = $"[Klik / E] Buka/Tutup: {CleanObjectName(dc.gameObject.name)}";
                isHovering = true;
                return;
            }
        }
    }

    void TriggerInteraction()
    {
        // 1. Klik Tombol UI (Pilihan A-E, Mulai Kuis, Tombol Power PC, dll)
        if (hoveredButton != null)
        {
            hoveredButton.onClick.Invoke();
            Debug.Log("[PCInteraction] Tombol UI diklik: " + hoveredButton.name);
            return;
        }

        // 2. Fokuskan Kotak Ketik TMP (Nama / Kelas Siswa)
        if (hoveredTMPInput != null)
        {
            activeTMPInput = hoveredTMPInput;
            isTypingText = true;

            if (EventSystem.current != null)
            {
                EventSystem.current.sendNavigationEvents = false;
                EventSystem.current.SetSelectedGameObject(hoveredTMPInput.gameObject);
            }

            hoveredTMPInput.Select();
            hoveredTMPInput.ActivateInputField();

            Debug.Log("[PCInteraction] Mengetik di: " + hoveredTMPInput.name);
            return;
        }

        // 3. Fokuskan Kotak Ketik Standar
        if (hoveredInput != null)
        {
            activeStdInput = hoveredInput;
            isTypingText = true;

            if (EventSystem.current != null)
            {
                EventSystem.current.sendNavigationEvents = false;
                EventSystem.current.SetSelectedGameObject(hoveredInput.gameObject);
            }

            hoveredInput.Select();
            hoveredInput.ActivateInputField();

            Debug.Log("[PCInteraction] Mengetik di: " + hoveredInput.name);
            return;
        }

        // 4. Interaksi Fiber Box (Pertemuan 4)
        if (hoveredFiberBox != null)
        {
            if (!fiberBoxIsOpen)
            {
                hoveredFiberBox.OpenBox();
                fiberBoxIsOpen = true;
            }
            else
            {
                hoveredFiberBox.CloseBox();
                fiberBoxIsOpen = false;
            }
            Debug.Log("[PCInteraction] Fiber Box dibuka/ditutup.");
            return;
        }

        // 5. Interaksi Komputer (Pertemuan 4)
        if (hoveredComputer != null)
        {
            hoveredComputer.TurnOnComputer();
            Debug.Log("[PCInteraction] Komputer dinyalakan.");
            return;
        }

        // 6. Interaksi Objek 3D (InfoKabelFO, PCInteractable, OTDR, RJ45, XRGrabInteractable, DetachPart)
        if (hoveredInfoKabel != null || hoveredPCInteractable != null || hoveredXRObject != null || hoveredRigidbody != null || hoveredUIObj != null)
        {
            GameObject targetObj = null;
            if (hoveredInfoKabel != null) targetObj = hoveredInfoKabel.gameObject;
            else if (hoveredPCInteractable != null) targetObj = hoveredPCInteractable.gameObject;
            else if (hoveredXRObject != null) targetObj = hoveredXRObject.gameObject;
            else if (hoveredRigidbody != null) targetObj = hoveredRigidbody.gameObject;
            else targetObj = hoveredUIObj;

            if (targetObj == null) return;

            var parentXR = targetObj.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
            if (parentXR != null) targetObj = parentXR.gameObject;

            DetachPart dp = targetObj.GetComponent<DetachPart>();
            if (dp != null)
            {
                targetObj.transform.SetParent(null);
            }

            if (enableObjectPickup)
            {
                PickupObject(targetObj);
            }

            if (hoveredPCInteractable != null)
            {
                hoveredPCInteractable.Interact();
            }

            Debug.Log("[PCInteraction] Berhasil berinteraksi dengan: " + targetObj.name);
        }
    }

    #region Sistem Angkat & Pegang Objek 3D (PC Grab / Hold)
    void PickupObject(GameObject obj)
    {
        heldTransform = obj.transform;
        originalParent = heldTransform.parent;

        initialWorldPosition = heldTransform.position;
        initialWorldRotation = heldTransform.rotation;

        if (playerCamera != null)
        {
            initialRelativeRotation = Quaternion.Inverse(playerCamera.transform.rotation) * heldTransform.rotation;
        }
        else
        {
            initialRelativeRotation = heldTransform.localRotation;
        }

        // Atur collider ke isTrigger agar tetap bisa menyentuh PortOTDRTrigger / RJ45 Snap Point
        heldColliders = obj.GetComponentsInChildren<Collider>();
        foreach (var col in heldColliders)
        {
            if (col != null)
            {
                col.isTrigger = true;
                col.enabled = true;
            }
        }

        heldRigidbody = obj.GetComponent<Rigidbody>();
        if (heldRigidbody != null)
        {
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
            heldRigidbody.useGravity = false;
            heldRigidbody.isKinematic = true;
        }

        heldXRInteractable = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (heldXRInteractable != null)
        {
            heldXRInteractable.selectEntered?.Invoke(new UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs());
            heldXRInteractable.firstSelectEntered?.Invoke(new UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs());
        }

        // Panggil event OTDRPicked jika yang diambil adalah OTDR (Pertemuan 4)
        OTDRController otdrCtrl = obj.GetComponentInChildren<OTDRController>();
        if (otdrCtrl != null && otdrCtrl.gameFlow != null)
        {
            otdrCtrl.gameFlow.OTDRPicked();
        }

        activeExplanationCanvases.Clear();

        Canvas[] childCanvases = obj.GetComponentsInChildren<Canvas>(true);
        foreach (var c in childCanvases)
        {
            c.gameObject.SetActive(true);
            for (int i = 0; i < c.transform.childCount; i++)
            {
                c.transform.GetChild(i).gameObject.SetActive(true);
            }
            activeExplanationCanvases.Add(c.gameObject);
        }

        InfoKabelFO ik = obj.GetComponentInChildren<InfoKabelFO>(true);
        if (ik != null && ik.infoCanvas != null)
        {
            ik.infoCanvas.SetActive(true);
            for (int i = 0; i < ik.infoCanvas.transform.childCount; i++)
            {
                ik.infoCanvas.transform.GetChild(i).gameObject.SetActive(true);
            }
            activeExplanationCanvases.Add(ik.infoCanvas);
        }

        PCInteractable pcInt = obj.GetComponentInChildren<PCInteractable>(true);
        if (pcInt != null && pcInt.penjelasanCanvas != null)
        {
            pcInt.penjelasanCanvas.SetActive(true);
            for (int i = 0; i < pcInt.penjelasanCanvas.transform.childCount; i++)
            {
                pcInt.penjelasanCanvas.transform.GetChild(i).gameObject.SetActive(true);
            }
            activeExplanationCanvases.Add(pcInt.penjelasanCanvas);
        }

        heldTransform.SetParent(holdPoint);
        Debug.Log("[PCInteraction] Memegang objek: " + obj.name);
    }

    void HandleHoldingObject()
    {
        if (heldTransform == null || holdPoint == null) return;

        heldTransform.localPosition = Vector3.Lerp(heldTransform.localPosition, Vector3.zero, Time.deltaTime * smoothHoldSpeed);
        
        if (playerCamera != null)
        {
            Quaternion targetWorldRot = playerCamera.transform.rotation * initialRelativeRotation;
            heldTransform.rotation = Quaternion.Slerp(heldTransform.rotation, targetWorldRot, Time.deltaTime * smoothHoldSpeed);
        }
    }

    void DropHeldObject()
    {
        if (heldTransform == null) return;

        heldTransform.SetParent(originalParent);
        heldTransform.position = initialWorldPosition;
        heldTransform.rotation = initialWorldRotation;

        if (heldRigidbody != null)
        {
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
            heldRigidbody.useGravity = false;
            heldRigidbody.isKinematic = true;
            heldRigidbody = null;
        }

        if (heldColliders != null)
        {
            foreach (var col in heldColliders)
            {
                if (col != null)
                {
                    col.isTrigger = false;
                    col.enabled = true;
                }
            }
            heldColliders = null;
        }

        if (heldXRInteractable != null)
        {
            heldXRInteractable.selectExited?.Invoke(new UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs());
            heldXRInteractable.lastSelectExited?.Invoke(new UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs());
            heldXRInteractable = null;
        }

        foreach (var c in activeExplanationCanvases)
        {
            if (c != null) c.SetActive(false);
        }
        activeExplanationCanvases.Clear();

        Debug.Log("[PCInteraction] Objek aman dikembalikan ke meja: " + heldTransform.name);
        heldTransform = null;
    }
    #endregion

    void EnsureEventSystemAndCanvases()
    {
        EventSystem es = FindFirstObjectByType<EventSystem>();
        if (es == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            es = esObj.AddComponent<EventSystem>();
            var sm = esObj.AddComponent<StandaloneInputModule>();
            sm.horizontalAxis = "Horizontal";
            sm.verticalAxis = "Vertical";
            sm.submitButton = "Submit";
            sm.cancelButton = "Cancel";
            Debug.Log("[PCInteraction] EventSystem otomatis dibuat.");
        }
        else
        {
            var standalone = es.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                if (string.IsNullOrEmpty(standalone.horizontalAxis)) standalone.horizontalAxis = "Horizontal";
                if (string.IsNullOrEmpty(standalone.verticalAxis)) standalone.verticalAxis = "Vertical";
                if (string.IsNullOrEmpty(standalone.submitButton)) standalone.submitButton = "Submit";
                if (string.IsNullOrEmpty(standalone.cancelButton)) standalone.cancelButton = "Cancel";
            }
            else
            {
                var inputModule = es.GetComponent("InputSystemUIInputModule") as MonoBehaviour;
                if (inputModule == null)
                {
                    var sm = es.gameObject.AddComponent<StandaloneInputModule>();
                    sm.horizontalAxis = "Horizontal";
                    sm.verticalAxis = "Vertical";
                    sm.submitButton = "Submit";
                    sm.cancelButton = "Cancel";
                }
            }
        }

        if (es != null)
        {
            es.sendNavigationEvents = false;
        }

        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Camera mainCam = playerCamera != null ? playerCamera : Camera.main;

        foreach (var c in allCanvases)
        {
            if (c.renderMode == RenderMode.WorldSpace && mainCam != null)
            {
                if (c.worldCamera == null)
                {
                    c.worldCamera = mainCam;
                }
            }

            if (c.GetComponent<GraphicRaycaster>() == null)
            {
                c.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }

    private string CleanObjectName(string rawName)
    {
        string clean = rawName.Replace("(Clone)", "").Replace("_", " ").Trim();
        return clean;
    }

    private void OnGUI()
    {
        // 1. Gambar Titik Crosshair di Tengah Layar (HANYA jika TIDAK sedang memegang barang dan TIDAK sedang mengetik)
        if (showCrosshair && heldTransform == null && !isTypingText)
        {
            Color prevColor = GUI.color;
            GUI.color = isHovering ? hoverColor : normalColor;

            float currentSize = isHovering ? crosshairSize * 1.6f : crosshairSize;
            Rect rect = new Rect((Screen.width - currentSize) * 0.5f, (Screen.height - currentSize) * 0.5f, currentSize, currentSize);

            if (crosshairTex != null)
                GUI.DrawTexture(rect, crosshairTex, ScaleMode.StretchToFill);

            GUI.color = prevColor;
        }

        // 2. Tampilkan Teks Petunjuk Interaksi di BAGIAN BAWAH LAYAR
        if ((isHovering || heldTransform != null) && !isTypingText)
        {
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.box);
                promptStyle.fontSize = 15;
                promptStyle.normal.textColor = Color.white;
                promptStyle.alignment = TextAnchor.MiddleCenter;
            }

            string textToShow = heldTransform != null 
                ? $"[Klik / E] Letakkan: {CleanObjectName(heldTransform.name)}" 
                : customPromptMessage;

            if (!string.IsNullOrEmpty(textToShow))
            {
                float boxWidth = Mathf.Max(240f, textToShow.Length * 9.5f);
                float boxHeight = 34f;
                Rect promptRect = new Rect((Screen.width - boxWidth) * 0.5f, Screen.height - 75f, boxWidth, boxHeight);

                GUI.backgroundColor = new Color(0.06f, 0.10f, 0.18f, 0.92f);
                GUI.Box(promptRect, textToShow, promptStyle);
            }
        }
    }
}