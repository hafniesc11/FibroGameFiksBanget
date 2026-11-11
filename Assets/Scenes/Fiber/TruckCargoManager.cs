using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckCargoManager : MonoBehaviour
{
    [Header("Tipe Media Transmisi")]
    [Tooltip("Centang jika ini truk Fiber Optik (aman dari gangguan). Jangan centang jika Tembaga (kardus akan jatuh).")]
    public bool isFiberOptic = false;

    [Header("Asset Model Box")]
    [Tooltip("Tarik asset kardusbox.glb ke sini")]
    public GameObject boxPrefab;

    [Header("Posisi Presisi di Dalam Bak Kuning Truk")]
    [Tooltip("Titik tengah mangkuk bak kuning dump truck")]
    public Vector3 cargoBedOffset = new Vector3(0f, 1.30f, -1.48f);
    public Vector3 boxScale = new Vector3(0.30f, 0.30f, 0.30f);

    [Header("Grid Kardus (Muatan Penuh 12 Kardus)")]
    public int columns = 2;    // Kiri - Kanan (X)
    public int rows = 3;       // Depan - Belakang (Z)
    public int layers = 2;     // Tingkat Bawah & Atas (Y) -> Total 12 Kardus Penuh
    public Vector3 boxSpacing = new Vector3(0.36f, 0.32f, 0.36f);

    [Header("Pengaturan Guncangan & Kejatuhan (Tembaga)")]
    public float shakeDuration = 2.4f;
    public float shakeIntensity = 0.045f;
    [Tooltip("Jumlah kardus yang terlempar keluar bak saat guncangan (menyisakan sedikit)")]
    public int minBoxesToDrop = 8;
    public int maxBoxesToDrop = 10;
    public float dropForce = 3.4f;

    [Header("Lampu Depan Truk (Penerangan Terowongan)")]
    public bool autoAddHeadlight = true;
    public Color headlightColor = new Color(1f, 0.98f, 0.9f);
    public float headlightRange = 40f;
    public float headlightIntensity = 5f;

    private List<GameObject> activeBoxes = new List<GameObject>();
    private List<GameObject> droppedBoxes = new List<GameObject>();
    private List<Vector3> originalLocalPositions = new List<Vector3>();
    private List<Quaternion> originalLocalRotations = new List<Quaternion>();

    private Transform cargoContainer;
    private bool isDisturbed = false;
    private Coroutine shakeCoroutine;

    private void OnValidate()
    {
        // Auto-upgrade jika di scene masih tersimpan konfigurasi lama (8 kardus / nilai drop sedikit)
        if (rows < 3 || minBoxesToDrop < 6 || boxScale.x < 0.05f)
        {
            boxScale = new Vector3(0.30f, 0.30f, 0.30f);
            cargoBedOffset = new Vector3(0f, 1.30f, -1.48f);
            boxSpacing = new Vector3(0.36f, 0.32f, 0.36f);
            columns = 2;
            rows = 3;
            layers = 2;
            shakeDuration = 2.4f;
            shakeIntensity = 0.045f;
            minBoxesToDrop = 8;
            maxBoxesToDrop = 10;
            dropForce = 3.4f;
        }
    }

    private void Awake()
    {
        if (rows < 3 || minBoxesToDrop < 6 || boxScale.x < 0.05f)
        {
            boxScale = new Vector3(0.30f, 0.30f, 0.30f);
            cargoBedOffset = new Vector3(0f, 1.30f, -1.48f);
            boxSpacing = new Vector3(0.36f, 0.32f, 0.36f);
            columns = 2;
            rows = 3;
            layers = 2;
            shakeDuration = 2.4f;
            shakeIntensity = 0.045f;
            minBoxesToDrop = 8;
            maxBoxesToDrop = 10;
            dropForce = 3.4f;
        }
    }

    void Start()
    {
        SpawnCargo();

        if (autoAddHeadlight)
        {
            EnsureHeadlight();
        }
    }

    void EnsureHeadlight()
    {
        Transform existingLight = transform.Find("TruckHeadlight");
        if (existingLight == null)
        {
            GameObject lightObj = new GameObject("TruckHeadlight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.3f, 0.5f);
            lightObj.transform.localRotation = Quaternion.identity;

            Light lightComp = lightObj.AddComponent<Light>();
            lightComp.type = LightType.Spot;
            lightComp.range = headlightRange;
            lightComp.spotAngle = 80f;
            lightComp.innerSpotAngle = 40f;
            lightComp.intensity = headlightIntensity;
            lightComp.color = headlightColor;
            lightComp.shadows = LightShadows.None;
        }
    }

    public void SpawnCargo()
    {
        ClearAllBoxes();

        if (boxPrefab == null)
        {
            Debug.LogWarning($"[TruckCargoManager] Box Prefab belum dimasukkan pada {gameObject.name}!");
            return;
        }

        cargoContainer = transform.Find("CargoContainer");
        if (cargoContainer == null)
        {
            GameObject containerObj = new GameObject("CargoContainer");
            containerObj.transform.SetParent(transform);
            containerObj.transform.localPosition = cargoBedOffset;
            containerObj.transform.localRotation = Quaternion.identity;
            containerObj.transform.localScale = Vector3.one;
            cargoContainer = containerObj.transform;
        }
        else
        {
            cargoContainer.localPosition = cargoBedOffset;
            cargoContainer.localRotation = Quaternion.identity;
            cargoContainer.localScale = Vector3.one;
        }

        float startX = -(columns - 1) * boxSpacing.x * 0.5f;
        float startZ = -(rows - 1) * boxSpacing.z * 0.5f;

        Random.InitState(gameObject.GetInstanceID());

        // Susun 12 kardus (2 kolom x 3 baris x 2 tingkat)
        for (int y = 0; y < layers; y++)
        {
            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < columns; x++)
                {
                    float jitterX = (y > 0) ? Random.Range(-0.015f, 0.015f) : 0f;
                    float jitterZ = (y > 0) ? Random.Range(-0.015f, 0.015f) : 0f;
                    float rotY = Random.Range(-4f, 4f);
                    float rotTilt = (y > 0) ? Random.Range(-1.5f, 1.5f) : 0f;

                    Vector3 localPos = new Vector3(
                        startX + (x * boxSpacing.x) + jitterX,
                        y * boxSpacing.y,
                        startZ + (z * boxSpacing.z) + jitterZ
                    );

                    Quaternion boxRot = Quaternion.Euler(rotTilt, rotY, rotTilt);
                    Vector3 individualScale = boxScale * Random.Range(0.96f, 1.04f);

                    GameObject box = Instantiate(boxPrefab, cargoContainer);
                    box.name = $"DataBox_L{y}_R{z}_C{x}";
                    box.transform.localPosition = localPos;
                    box.transform.localRotation = boxRot;
                    box.transform.localScale = individualScale;

                    if (box.GetComponent<Collider>() == null)
                    {
                        box.AddComponent<BoxCollider>();
                    }

                    activeBoxes.Add(box);
                    originalLocalPositions.Add(localPos);
                    originalLocalRotations.Add(boxRot);
                }
            }
        }

        isDisturbed = false;
    }

    public void TriggerDisturbance()
    {
        if (isDisturbed) return;
        isDisturbed = true;

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(DisturbanceRoutine());
    }

    private IEnumerator DisturbanceRoutine()
    {
        float elapsed = 0f;

        // Tentukan kardus yang jatuh: prioritaskan seluruh tingkat atas (Layer 1) lalu disusul tingkat bawah belakang
        List<int> dropIndices = new List<int>();
        if (!isFiberOptic && activeBoxes.Count > 0)
        {
            int totalToDrop = Mathf.Clamp(Random.Range(minBoxesToDrop, maxBoxesToDrop + 1), 1, activeBoxes.Count);

            // 1. Ambil seluruh kardus tingkat atas (dari index tertinggi ke bawah)
            List<int> topLayerCandidates = new List<int>();
            List<int> bottomLayerCandidates = new List<int>();

            int halfCount = activeBoxes.Count / 2;
            for (int i = activeBoxes.Count - 1; i >= halfCount; i--)
            {
                if (activeBoxes[i] != null) topLayerCandidates.Add(i);
            }
            for (int i = halfCount - 1; i >= 0; i--)
            {
                if (activeBoxes[i] != null) bottomLayerCandidates.Add(i);
            }

            // Masukkan seluruh tingkat atas
            while (topLayerCandidates.Count > 0 && dropIndices.Count < totalToDrop)
            {
                int r = Random.Range(0, topLayerCandidates.Count);
                dropIndices.Add(topLayerCandidates[r]);
                topLayerCandidates.RemoveAt(r);
            }

            // Jika masih perlu dijatuhkan, ambil dari tingkat bawah
            while (bottomLayerCandidates.Count > 0 && dropIndices.Count < totalToDrop)
            {
                int r = Random.Range(0, bottomLayerCandidates.Count);
                dropIndices.Add(bottomLayerCandidates[r]);
                bottomLayerCandidates.RemoveAt(r);
            }
        }

        int droppedSoFar = 0;
        float dropInterval = (dropIndices.Count > 0) ? (shakeDuration * 0.7f / dropIndices.Count) : 1f;
        float nextDropTime = 0.35f;

        while (elapsed < shakeDuration)
        {
            float t = elapsed / shakeDuration;
            float envelope = Mathf.Sin(t * Mathf.PI);

            float intensity = isFiberOptic ? (shakeIntensity * 0.2f) : shakeIntensity;

            // Jatuhkan kardus bertahap satu per satu secara berurutan
            if (!isFiberOptic && droppedSoFar < dropIndices.Count && elapsed >= nextDropTime)
            {
                int idx = dropIndices[droppedSoFar];
                DropSingleBox(idx);
                droppedSoFar++;
                nextDropTime += dropInterval;
            }

            // Goyangan fisik kardus yang tersisa di dalam bak
            for (int i = 0; i < activeBoxes.Count; i++)
            {
                if (activeBoxes[i] == null) continue;

                float phase = i * 1.4f;
                float wobbleX = Mathf.Sin((elapsed * 16f) + phase) * intensity * envelope;
                float wobbleZ = Mathf.Cos((elapsed * 12f) + phase) * intensity * envelope;
                float wobbleY = Mathf.Abs(Mathf.Sin((elapsed * 20f) + phase)) * (intensity * 0.5f) * envelope;

                float tiltRoll = Mathf.Sin((elapsed * 14f) + phase) * 7f * envelope;
                float tiltPitch = Mathf.Cos((elapsed * 10f) + phase) * 5f * envelope;

                Vector3 targetPos = originalLocalPositions[i] + new Vector3(wobbleX, wobbleY, wobbleZ);
                Quaternion targetRot = originalLocalRotations[i] * Quaternion.Euler(tiltPitch, 0f, tiltRoll);

                activeBoxes[i].transform.localPosition = Vector3.Lerp(activeBoxes[i].transform.localPosition, targetPos, Time.deltaTime * 30f);
                activeBoxes[i].transform.localRotation = Quaternion.Slerp(activeBoxes[i].transform.localRotation, targetRot, Time.deltaTime * 30f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Kembalikan sedikit kardus yang tersisa ke posisi tenang
        float resetTimer = 0f;
        while (resetTimer < 0.3f)
        {
            resetTimer += Time.deltaTime;
            float r = resetTimer / 0.3f;

            for (int i = 0; i < activeBoxes.Count; i++)
            {
                if (activeBoxes[i] != null)
                {
                    activeBoxes[i].transform.localPosition = Vector3.Lerp(activeBoxes[i].transform.localPosition, originalLocalPositions[i], r);
                    activeBoxes[i].transform.localRotation = Quaternion.Slerp(activeBoxes[i].transform.localRotation, originalLocalRotations[i], r);
                }
            }
            yield return null;
        }

        for (int i = 0; i < activeBoxes.Count; i++)
        {
            if (activeBoxes[i] != null)
            {
                activeBoxes[i].transform.localPosition = originalLocalPositions[i];
                activeBoxes[i].transform.localRotation = originalLocalRotations[i];
            }
        }
    }

    private void DropSingleBox(int boxIndex)
    {
        if (boxIndex < 0 || boxIndex >= activeBoxes.Count) return;

        GameObject boxToDrop = activeBoxes[boxIndex];
        activeBoxes[boxIndex] = null;

        if (boxToDrop != null)
        {
            boxToDrop.transform.SetParent(null);
            droppedBoxes.Add(boxToDrop);

            Rigidbody rb = boxToDrop.GetComponent<Rigidbody>();
            if (rb == null)
                rb = boxToDrop.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = 1.5f;

            // Lempar ke belakang dan ke atas/samping agar terlempar keluar dari bak truk
            Vector3 throwDir = -transform.forward * 1.8f + Vector3.up * 2.2f + transform.right * Random.Range(-1.0f, 1.0f);
            rb.AddForce(throwDir * dropForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 14f, ForceMode.Impulse);

            Destroy(boxToDrop, 10f);
            Debug.Log($"[TruckCargoManager] 1 Box data terlempar keluar dari bak {gameObject.name}!");
        }
    }

    public void ResetCargo()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        SpawnCargo();
        Debug.Log($"[TruckCargoManager] Muatan data di {gameObject.name} di-reset kembali utuh.");
    }

    private void ClearAllBoxes()
    {
        for (int i = 0; i < activeBoxes.Count; i++)
        {
            if (activeBoxes[i] != null)
                Destroy(activeBoxes[i]);
        }
        activeBoxes.Clear();
        originalLocalPositions.Clear();
        originalLocalRotations.Clear();

        for (int i = 0; i < droppedBoxes.Count; i++)
        {
            if (droppedBoxes[i] != null)
                Destroy(droppedBoxes[i]);
        }
        droppedBoxes.Clear();
    }
}
