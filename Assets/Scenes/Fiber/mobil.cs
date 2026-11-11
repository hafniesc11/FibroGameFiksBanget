using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PodMovement : MonoBehaviour
{
    [Header("Titik Awal & Tujuan")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Kecepatan & Perjalanan")]
    public float speed = 5f;
    public bool canMove = false;
    public bool arrived = false;
    public bool isTurning = false;

    [Header("Sudut Pandang Kamera Player")]
    [Tooltip("Posisi kamera relatif terhadap mobil")]
    public Vector3 cameraOffset = new Vector3(0f, 0.7f, -0.8f);

    [Header("Visual Model Mobil")]
    public Transform visualMobil;

    [Header("Pengaturan Guncangan Halus & Realistis")]
    public float shakeDuration = 2.4f;
    public float maxRollAngle = 2.5f;       // Kemiringan goyang kiri-kanan (derajat)
    public float maxPitchAngle = 1.5f;      // Anggukan maju-mundur (derajat)
    public float maxBounceHeight = 0.03f;   // Ayunan suspensi naik-turun (meter)
    public float shakeFrequency = 12f;      // Frekuensi getaran suspensi

    [Header("Setelah Sampai Tujuan")]
    public float delayTeleportSetelahSampai = 1.0f;
    public bool teleportSetelahSampai = true;
    public Transform titikTeleportSetelahSampai;
    public GameObject canvasSelamat;
    public GameObject[] objectDimatikanSetelahSampai;
    public GameObject[] objectDinyalakanSetelahSampai;

    [Header("Pengelola Muatan Kardus Data")]
    public TruckCargoManager cargoManager;

    private GameObject passenger;
    private bool isShaking = false;
    private bool teleportRoutineStarted = false;
    private Vector3 originalVisualLocalPos = Vector3.zero;
    private Quaternion originalVisualLocalRot = Quaternion.Euler(-92.4f, 0f, 0.8f);
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        EnsureVisualChild();

        if (cargoManager == null)
        {
            cargoManager = GetComponent<TruckCargoManager>();
        }
    }

    private void EnsureVisualChild()
    {
        // Pastikan visualMobil menunjuk ke anak dump_truck, BUKAN ke root CarFiber/CarUTP
        if (visualMobil == null || visualMobil == transform)
        {
            Transform dt = transform.Find("dump_truck");
            if (dt != null) visualMobil = dt;
        }

        if (visualMobil != null && visualMobil != transform)
        {
            originalVisualLocalPos = visualMobil.localPosition;
            originalVisualLocalRot = visualMobil.localRotation;
        }
    }

    public void ResetPod()
    {
        StopAllCoroutines();

        transform.position = startPosition;
        transform.rotation = startRotation;

        if (passenger != null)
        {
            passenger.transform.SetParent(null);
            passenger.transform.localScale = Vector3.one;
            passenger = null;
        }

        canMove = false;
        arrived = false;
        isShaking = false;
        teleportRoutineStarted = false;

        EnsureVisualChild();

        if (visualMobil != null && visualMobil != transform)
        {
            visualMobil.localPosition = originalVisualLocalPos;
            visualMobil.localRotation = originalVisualLocalRot;
        }

        if (cargoManager == null)
            cargoManager = GetComponent<TruckCargoManager>();

        if (cargoManager != null)
            cargoManager.ResetCargo();

        Debug.Log("Mobil di-reset ke posisi awal");
    }

    public void SetPassenger(GameObject obj, Transform seat = null)
    {
        passenger = obj;

        if (passenger != null)
        {
            passenger.transform.SetParent(null);
            passenger.transform.localScale = Vector3.one;

            passenger.transform.position = GetCameraPosition(passenger);

            Vector3 forward = transform.forward;
            forward.y = 0;
            if (forward != Vector3.zero)
                passenger.transform.rotation = Quaternion.LookRotation(forward);

            CharacterController cc = passenger.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;

            PCPlayerController pc = passenger.GetComponent<PCPlayerController>();
            if (pc != null)
                pc.enabled = false;

            MouseLook ml = passenger.GetComponentInChildren<MouseLook>();
            if (ml != null)
                ml.ResetView(0f);
        }

        Debug.Log("Player mulai memantau dari atas mobil");
    }

    public void MulaiJalan()
    {
        canMove = true;
        arrived = false;
        teleportRoutineStarted = false;
        Debug.Log("Mobil mulai melaju menuju tujuan");
    }

    public void StartMove()
    {
        MulaiJalan();
    }

    public void StopJalan()
    {
        canMove = false;
    }

    public Vector3 GetCameraPosition(GameObject target)
    {
        float camLocalY = 0.8f;
        Camera cam = target.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            camLocalY = cam.transform.localPosition.y;
        }

        Vector3 desiredCameraWorldPos = transform.position + (transform.rotation * cameraOffset);
        return desiredCameraWorldPos - (Vector3.up * camLocalY);
    }

    private void Update()
    {
        if (!canMove || arrived) return;

        // Jika sedang berbelok oleh TriggerMulti di terowongan multimode, biarkan rotasi belokan yang memimpin
        if (isTurning) return;

        if (endPoint == null)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            return;
        }

        Vector3 targetPos = endPoint.position;
        Vector3 moveDir = (targetPos - transform.position);
        float distance = moveDir.magnitude;

        if (distance > 0.05f)
        {
            Vector3 step = moveDir.normalized * speed * Time.deltaTime;
            if (step.magnitude > distance) step = moveDir;

            transform.position += step;

            Vector3 lookDir = moveDir;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 5f
                );
            }
        }
        else
        {
            transform.position = targetPos;
            canMove = false;
            arrived = true;

            Debug.Log("Mobil sampai tujuan");

            if (!teleportRoutineStarted)
            {
                teleportRoutineStarted = true;
                StartCoroutine(TeleportSetelahSampai());
            }
        }
    }

    private void LateUpdate()
    {
        if (passenger != null)
        {
            passenger.transform.position = GetCameraPosition(passenger);
            passenger.transform.localScale = Vector3.one;
        }
    }

    private IEnumerator TeleportSetelahSampai()
    {
        yield return new WaitForSeconds(delayTeleportSetelahSampai);

        canMove = false;
        arrived = true;

        GameObject xrYangDiteleport = passenger;
        passenger = null;

        MatikanObjectSetelahSampai();

        if (canvasSelamat != null)
        {
            canvasSelamat.SetActive(true);
        }

        if (xrYangDiteleport != null)
        {
            xrYangDiteleport.transform.SetParent(null);
            xrYangDiteleport.transform.localScale = Vector3.one;

            CharacterController cc = xrYangDiteleport.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            PCPlayerController pc = xrYangDiteleport.GetComponent<PCPlayerController>();
            if (pc != null) pc.enabled = false;

            Rigidbody rb = xrYangDiteleport.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (teleportSetelahSampai && titikTeleportSetelahSampai != null)
            {
                Vector3 safePos = titikTeleportSetelahSampai.position;
                if (safePos.y < 0.2f) safePos.y = 0.5f;

                xrYangDiteleport.transform.SetPositionAndRotation(
                    safePos,
                    titikTeleportSetelahSampai.rotation
                );
            }

            Physics.SyncTransforms();

            // Tampilkan visual tumpukan paket data yang berhasil terkirim di ruang server (HANYA UNTUK PERTEMUAN 2)
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isPert2 = sceneName.Contains("Pert2") || sceneName.Contains("Pertemuan2");

            if (isPert2)
            {
                if (DeliveredCargoDisplay.Instance != null && cablemanager.Instance != null)
                {
                    DeliveredCargoDisplay.Instance.ShowDeliveredCargo(cablemanager.Instance.selectedCable);
                }
                else if (DeliveredCargoDisplay.Instance == null)
                {
                    GameObject dcdObj = new GameObject("DeliveredCargoDisplay");
                    DeliveredCargoDisplay dcd = dcdObj.AddComponent<DeliveredCargoDisplay>();
                    if (cargoManager != null && cargoManager.boxPrefab != null)
                        dcd.boxPrefab = cargoManager.boxPrefab;
                    if (cablemanager.Instance != null)
                        dcd.ShowDeliveredCargo(cablemanager.Instance.selectedCable);
                }
            }

            if (cc != null)
                cc.enabled = true;

            if (pc != null)
                pc.enabled = true;
        }

        Debug.Log("Player dilepas dari mobil lalu diteleport ke tempat tujuan akhir");
    }

    private void MatikanObjectSetelahSampai()
    {
        for (int i = 0; i < objectDimatikanSetelahSampai.Length; i++)
        {
            if (objectDimatikanSetelahSampai[i] != null)
                objectDimatikanSetelahSampai[i].SetActive(false);
        }
    }

    private void NyalakanObjectSetelahSampai()
    {
        for (int i = 0; i < objectDinyalakanSetelahSampai.Length; i++)
        {
            if (objectDinyalakanSetelahSampai[i] != null)
                objectDinyalakanSetelahSampai[i].SetActive(true);
        }
    }

    public void StartShake()
    {
        if (cargoManager == null)
            cargoManager = GetComponent<TruckCargoManager>();

        if (cargoManager != null)
            cargoManager.TriggerDisturbance();

        if (!isShaking)
            StartCoroutine(SmoothShakeCoroutine());
    }

    private IEnumerator SmoothShakeCoroutine()
    {
        isShaking = true;

        EnsureVisualChild();

        if (visualMobil == null || visualMobil == transform)
        {
            isShaking = false;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float t = elapsed / shakeDuration;
            float envelope = Mathf.Sin(t * Mathf.PI);

            // Ayunan suspensi naik-turun dan miring kiri-kanan secara harmonik
            float bounceY = Mathf.Sin(elapsed * shakeFrequency) * maxBounceHeight * envelope;
            float rollZ = Mathf.Sin(elapsed * (shakeFrequency * 0.8f)) * maxRollAngle * envelope;
            float pitchX = Mathf.Cos(elapsed * (shakeFrequency * 0.6f)) * maxPitchAngle * envelope;

            // Pre-multiply dalam ruang induk agar rotasi miring dihitung dalam koordinat mobil
            Quaternion parentShakeRot = Quaternion.Euler(pitchX, 0f, rollZ);

            visualMobil.localPosition = originalVisualLocalPos + new Vector3(0f, bounceY, 0f);
            visualMobil.localRotation = parentShakeRot * originalVisualLocalRot;

            elapsed += Time.deltaTime;
            yield return null;
        }

        visualMobil.localPosition = originalVisualLocalPos;
        visualMobil.localRotation = originalVisualLocalRot;

        isShaking = false;
    }
}