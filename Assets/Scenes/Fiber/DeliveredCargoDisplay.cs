using System.Collections.Generic;
using UnityEngine;

public class DeliveredCargoDisplay : MonoBehaviour
{
    public static DeliveredCargoDisplay Instance;

    [Header("Titik Lokasi di Ruang Server")]
    [Tooltip("Tarik GameObject penanda di Scene (misal TitikKardus) ke sini")]
    public Transform titikLokasiKardus;

    [Header("Objek Manual (Opsional)")]
    public GameObject manualObjectCopper;
    public GameObject manualObjectFiber;

    [Header("Asset Model Box Kardus")]
    [Tooltip("Tarik asset kardusbox.glb ke sini")]
    public GameObject boxPrefab;

    [Header("Bentuk Kotak Tertutup Sempurna (Rapi & Padat)")]
    [Tooltip("Centang agar kardus berbentuk kubus tertutup sempurna tanpa flap/tutup terbuka yang menonjol")]
    public bool gunakanKotakTertutupSempurna = true;
    public Color warnaKardus = new Color(0.76f, 0.55f, 0.33f);

    [Header("Ukuran & Jarak Kardus (Lebih Besar & Mantap)")]
    public Vector3 boxScale = new Vector3(0.52f, 0.45f, 0.52f);
    public Vector3 boxSpacing = new Vector3(0.55f, 0.45f, 0.55f);

    private List<GameObject> spawnedBoxes = new List<GameObject>();
    private Transform displayContainer;
    private Material cachedCardboardMat;

    private void OnValidate()
    {
        // Auto-update ke ukuran lebih besar jika Inspector masih menyimpan ukuran lama
        if (boxScale.x < 0.45f || boxScale.y < 0.40f)
        {
            boxScale = new Vector3(0.52f, 0.45f, 0.52f);
            boxSpacing = new Vector3(0.55f, 0.45f, 0.55f);
        }
    }

    private void Awake()
    {
        Instance = this;
        ResolveBoxPrefab();

        if (manualObjectCopper != null) manualObjectCopper.SetActive(false);
        if (manualObjectFiber != null) manualObjectFiber.SetActive(false);
    }

    private void ResolveBoxPrefab()
    {
        if (boxPrefab != null) return;

        TruckCargoManager[] tcms = FindObjectsByType<TruckCargoManager>(FindObjectsSortMode.None);
        foreach (var tcm in tcms)
        {
            if (tcm != null && tcm.boxPrefab != null)
            {
                boxPrefab = tcm.boxPrefab;
                return;
            }
        }

        TrafficPodSpawner[] sps = FindObjectsByType<TrafficPodSpawner>(FindObjectsSortMode.None);
        foreach (var sp in sps)
        {
            if (sp != null && sp.boxPrefab != null)
            {
                boxPrefab = sp.boxPrefab;
                return;
            }
        }

        boxPrefab = Resources.Load<GameObject>("kardusbox");
    }

    private Material GetCardboardMaterial()
    {
        if (cachedCardboardMat != null) return cachedCardboardMat;

        if (boxPrefab != null)
        {
            Renderer r = boxPrefab.GetComponentInChildren<Renderer>();
            if (r != null && r.sharedMaterial != null)
            {
                cachedCardboardMat = new Material(r.sharedMaterial);
                return cachedCardboardMat;
            }
        }

        Shader standardShader = Shader.Find("Universal Render Pipeline/Lit");
        if (standardShader == null) standardShader = Shader.Find("Standard");

        if (standardShader != null)
        {
            cachedCardboardMat = new Material(standardShader);
            cachedCardboardMat.color = warnaKardus;
        }

        return cachedCardboardMat;
    }

    public void ShowDeliveredCargo(cablemanager.CableType cableType)
    {
        ClearDisplay();

        // Fitur tumpukan kardus server HANYA untuk scene Pertemuan 2
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!currentScene.Contains("Pert2") && !currentScene.Contains("Pertemuan2"))
        {
            return;
        }

        if (manualObjectCopper != null || manualObjectFiber != null)
        {
            if (cableType == cablemanager.CableType.Copper)
            {
                if (manualObjectCopper != null) manualObjectCopper.SetActive(true);
                if (manualObjectFiber != null) manualObjectFiber.SetActive(false);
            }
            else
            {
                if (manualObjectFiber != null) manualObjectFiber.SetActive(true);
                if (manualObjectCopper != null) manualObjectCopper.SetActive(false);
            }
            return;
        }

        ResolveBoxPrefab();

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        if (titikLokasiKardus != null)
        {
            spawnPos = titikLokasiKardus.position;
            spawnRot = titikLokasiKardus.rotation;
        }
        else
        {
            GameObject spawnAkhir = GameObject.Find("SpawnAkhir");
            if (spawnAkhir != null)
            {
                spawnPos = spawnAkhir.transform.position + (spawnAkhir.transform.forward * 2.2f) + (-spawnAkhir.transform.right * 0.8f);
                spawnPos.y = spawnAkhir.transform.position.y + 0.05f;
                spawnRot = spawnAkhir.transform.rotation;
            }
            else
            {
                spawnPos = new Vector3(3.85f, 0.0f, -9.65f);
                spawnRot = Quaternion.Euler(0f, 65f, 0f);
            }
        }

        GameObject containerObj = new GameObject("ServerRoom_CargoStack");
        containerObj.transform.SetParent(null);
        containerObj.transform.position = spawnPos;
        containerObj.transform.rotation = spawnRot;
        containerObj.transform.localScale = Vector3.one;
        displayContainer = containerObj.transform;

        SpawnStack(cableType);
    }

    private void SpawnStack(cablemanager.CableType cableType)
    {
        Material mat = GetCardboardMaterial();

        if (cableType == cablemanager.CableType.Copper)
        {
            // === TEMBAGA: Susunan 8 Kardus (6 di lantai + 2 di atas, tingkat atas banyak yang hilang) ===
            Debug.Log("[DeliveredCargoDisplay] Menampilkan Data Tembaga (8 Kardus Besar, tampak ada paket hilang).");

            int cols = 3;
            int rows = 2;

            float startX = -(cols - 1) * boxSpacing.x * 0.5f;
            float startZ = -(rows - 1) * boxSpacing.z * 0.5f;

            // 1. Lapisan Dasar (6 Kardus Lengkap di Lantai)
            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Vector3 localPos = new Vector3(
                        startX + (x * boxSpacing.x),
                        boxScale.y * 0.5f,
                        startZ + (z * boxSpacing.z)
                    );

                    GameObject box = CreateSingleBox(mat, $"Delivered_CopperBox_Base_{z}_{x}");
                    box.transform.SetParent(displayContainer);
                    box.transform.localPosition = localPos;
                    box.transform.localRotation = Quaternion.identity;
                    box.transform.localScale = boxScale;

                    spawnedBoxes.Add(box);
                }
            }

            // 2. Lapisan Atas (Hanya 2 Kardus, sisa 4 hilang akibat packet loss)
            Vector3[] topPositions = new Vector3[]
            {
                new Vector3(startX + (0 * boxSpacing.x), (1 * boxSpacing.y) + (boxScale.y * 0.5f), startZ + (0 * boxSpacing.z)),
                new Vector3(startX + (2 * boxSpacing.x), (1 * boxSpacing.y) + (boxScale.y * 0.5f), startZ + (1 * boxSpacing.z))
            };

            for (int i = 0; i < topPositions.Length; i++)
            {
                GameObject box = CreateSingleBox(mat, $"Delivered_CopperBox_Top_{i}");
                box.transform.SetParent(displayContainer);
                box.transform.localPosition = topPositions[i];
                box.transform.localRotation = Quaternion.identity;
                box.transform.localScale = boxScale;

                spawnedBoxes.Add(box);
            }

            // BoxCollider penghalang fisik
            BoxCollider col = displayContainer.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, boxSpacing.y * 1.0f, 0f);
            col.size = new Vector3(cols * boxSpacing.x * 1.05f, 2 * boxSpacing.y * 1.05f, rows * boxSpacing.z * 1.05f);
            col.isTrigger = false;
        }
        else
        {
            // === FIBER: Susunan 26 Kardus Palet Gudang Megah (4 Kolom x 3 Baris x 2 Tingkat Dasar = 24 + 2 Puncak = 26 Kardus) ===
            Debug.Log("[DeliveredCargoDisplay] Menampilkan Data Fiber (26 Kardus Palet Gudang Megah & Utuh 100%).");

            int cols = 4; // 4 Kolom
            int rows = 3; // 3 Baris
            int baseLayers = 2; // 2 Tingkat Dasar (4x3x2 = 24 kardus)

            float startX = -(cols - 1) * boxSpacing.x * 0.5f;
            float startZ = -(rows - 1) * boxSpacing.z * 0.5f;

            // 1. Lapisan Dasar & Tengah (24 Kardus Padat)
            for (int y = 0; y < baseLayers; y++)
            {
                for (int z = 0; z < rows; z++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        Vector3 localPos = new Vector3(
                            startX + (x * boxSpacing.x),
                            (y * boxSpacing.y) + (boxScale.y * 0.5f),
                            startZ + (z * boxSpacing.z)
                        );

                        GameObject box = CreateSingleBox(mat, $"Delivered_FiberBox_L{y}_R{z}_C{x}");
                        box.transform.SetParent(displayContainer);
                        box.transform.localPosition = localPos;
                        box.transform.localRotation = Quaternion.identity;
                        box.transform.localScale = boxScale;

                        spawnedBoxes.Add(box);
                    }
                }
            }

            // 2. Lapisan Puncak (2 Kardus Simetris di Tengah Atas)
            Vector3 peakPos1 = new Vector3(-boxSpacing.x * 0.5f, (baseLayers * boxSpacing.y) + (boxScale.y * 0.5f), 0f);
            Vector3 peakPos2 = new Vector3( boxSpacing.x * 0.5f, (baseLayers * boxSpacing.y) + (boxScale.y * 0.5f), 0f);

            GameObject peakBox1 = CreateSingleBox(mat, "Delivered_FiberBox_Peak_1");
            peakBox1.transform.SetParent(displayContainer);
            peakBox1.transform.localPosition = peakPos1;
            peakBox1.transform.localRotation = Quaternion.identity;
            peakBox1.transform.localScale = boxScale;
            spawnedBoxes.Add(peakBox1);

            GameObject peakBox2 = CreateSingleBox(mat, "Delivered_FiberBox_Peak_2");
            peakBox2.transform.SetParent(displayContainer);
            peakBox2.transform.localPosition = peakPos2;
            peakBox2.transform.localRotation = Quaternion.identity;
            peakBox2.transform.localScale = boxScale;
            spawnedBoxes.Add(peakBox2);

            // BoxCollider penghalang fisik besar
            BoxCollider col = displayContainer.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, boxSpacing.y * 1.5f, 0f);
            col.size = new Vector3(cols * boxSpacing.x * 1.05f, (baseLayers + 1) * boxSpacing.y * 1.05f, rows * boxSpacing.z * 1.05f);
            col.isTrigger = false;
        }
    }

    private GameObject CreateSingleBox(Material mat, string name)
    {
        if (gunakanKotakTertutupSempurna)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;

            if (mat != null)
            {
                Renderer rend = box.GetComponent<Renderer>();
                if (rend != null) rend.material = mat;
            }

            BoxCollider bc = box.GetComponent<BoxCollider>();
            if (bc != null) bc.isTrigger = false;

            return box;
        }
        else if (boxPrefab != null)
        {
            GameObject box = Instantiate(boxPrefab);
            box.name = name;

            if (box.GetComponent<Collider>() == null)
            {
                BoxCollider bc = box.AddComponent<BoxCollider>();
                bc.isTrigger = false;
            }

            return box;
        }
        else
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            return box;
        }
    }

    public void ClearDisplay()
    {
        StopAllCoroutines();

        if (manualObjectCopper != null) manualObjectCopper.SetActive(false);
        if (manualObjectFiber != null) manualObjectFiber.SetActive(false);

        for (int i = 0; i < spawnedBoxes.Count; i++)
        {
            if (spawnedBoxes[i] != null)
                Destroy(spawnedBoxes[i]);
        }
        spawnedBoxes.Clear();

        if (displayContainer != null)
        {
            Destroy(displayContainer.gameObject);
            displayContainer = null;
        }
    }
}
