using UnityEngine;
using System.Collections;

public class TrafficPodMovement : MonoBehaviour
{
    [Header("Arah Gerak")]
    public Vector3 moveDirection = Vector3.forward;

    [Header("Kecepatan")]
    public float baseSpeed = 5f;
    public float speedOffset = 0f;
    public float lifeTime = 120f;

    [Header("Acuan Truk Utama (Player)")]
    public PodMovement mainPodMovement;

    [Header("Penyesuaian Orientasi Model")]
    public Vector3 modelRotationOffset = Vector3.zero;

    [Header("Muatan Box Data")]
    public TruckCargoManager cargoManager;

    [Header("Status Tipe Terowongan")]
    public bool isFiberOptic = false;

    [Header("Visual Shake Truk Traffic")]
    public float maxRollAngle = 2.0f;
    public float maxPitchAngle = 1.2f;
    public float maxBounceHeight = 0.025f;
    public float shakeFrequency = 12f;

    private bool isDisturbed = false;
    private Transform visualChild;
    private Vector3 originalVisualPos;
    private Quaternion originalVisualRot;

    private void Start()
    {
        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }

        visualChild = transform.Find("dump_truck");
        if (visualChild != null)
        {
            originalVisualPos = visualChild.localPosition;
            originalVisualRot = visualChild.localRotation;
        }

        if (cargoManager == null)
            cargoManager = GetComponent<TruckCargoManager>();

        if (cargoManager != null)
        {
            cargoManager.isFiberOptic = isFiberOptic;
        }
    }

    private void Update()
    {
        float currentSpeed = (mainPodMovement != null) ? (mainPodMovement.speed + speedOffset) : (baseSpeed + speedOffset);

        transform.position += moveDirection.normalized * currentSpeed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(modelRotationOffset);
        }
    }

    public void TriggerDisturbance()
    {
        if (isDisturbed) return;
        isDisturbed = true;

        if (cargoManager == null)
            cargoManager = GetComponent<TruckCargoManager>();

        if (cargoManager != null)
        {
            cargoManager.TriggerDisturbance();
        }

        if (visualChild != null)
        {
            StartCoroutine(SmoothTrafficShakeCoroutine());
        }
    }

    private IEnumerator SmoothTrafficShakeCoroutine()
    {
        float elapsed = 0f;
        float duration = 2.4f;

        float intensityMultiplier = isFiberOptic ? 0.35f : 1.0f;
        float seed = Random.Range(0f, 10f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float envelope = Mathf.Sin(t * Mathf.PI);

            float bounceY = Mathf.Sin((elapsed + seed) * shakeFrequency) * maxBounceHeight * envelope * intensityMultiplier;
            float rollZ = Mathf.Sin((elapsed + seed) * (shakeFrequency * 0.8f)) * maxRollAngle * envelope * intensityMultiplier;
            float pitchX = Mathf.Cos((elapsed + seed) * (shakeFrequency * 0.6f)) * maxPitchAngle * envelope * intensityMultiplier;

            // Pre-multiply rotasi dalam ruang induk
            Quaternion parentShakeRot = Quaternion.Euler(pitchX, 0f, rollZ);

            visualChild.localPosition = originalVisualPos + new Vector3(0f, bounceY, 0f);
            visualChild.localRotation = parentShakeRot * originalVisualRot;

            elapsed += Time.deltaTime;
            yield return null;
        }

        visualChild.localPosition = originalVisualPos;
        visualChild.localRotation = originalVisualRot;
    }
}