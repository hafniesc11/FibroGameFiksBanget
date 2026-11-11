using UnityEngine;
using System.Collections;

/// <summary>
/// Trigger untuk membelokkan mobil/pod tepat di tikungan terowongan Multimode (Pertemuan 3).
/// Menghitung kurva lengkung presisi berdasarkan titik pusat tikungan (Apex) sehingga mobil
/// 100% berbelok tepat di dalam terowongan tanpa pernah menabrak atau keluar jalur.
/// </summary>
public class TriggerMulti : MonoBehaviour
{
    [Header("Pengaturan Belok")]
    [Tooltip("Sudut belok dalam derajat (90 = belok kanan, -90 = belok kiri)")]
    public float turnAngle = 90f;

    [Tooltip("Durasi transisi belokan")]
    public float turnDuration = 1.8f;

    [Tooltip("Radius kelengkungan belokan di dalam terowongan (meter)")]
    public float cornerRadius = 2.5f;

    [Header("Titik Tujuan Baru Setelah Belok")]
    public Transform newEndPointAfterTurn;

    private bool sudahBelok = false;

    private void OnTriggerEnter(Collider other)
    {
        PodMovement pod = other.GetComponentInParent<PodMovement>();
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        Transform carTransform = pod != null ? pod.transform : (rb != null ? rb.transform : other.transform.root);

        if (carTransform == null || sudahBelok)
            return;

        sudahBelok = true;
        Debug.Log("[TriggerMulti] Mobil menyentuh area tikungan terowongan multimode.");

        // Cari titik End milik TunnelMultimode (1) secara presisi
        if (newEndPointAfterTurn == null)
        {
            GameObject tunnel2 = GameObject.Find("TunnelMultimode (1)");
            if (tunnel2 != null)
            {
                Transform endTr = tunnel2.transform.Find("End");
                if (endTr != null) newEndPointAfterTurn = endTr;
            }
        }

        StartCoroutine(BelokPresisiApex(carTransform, pod, rb));
    }

    private IEnumerator BelokPresisiApex(Transform mobil, PodMovement pod, Rigidbody rb)
    {
        if (pod != null)
        {
            pod.isTurning = true;
        }

        // Titik pusat tikungan terowongan (Apex)
        Vector3 apex = transform.position;
        apex.y = mobil.position.y;

        Quaternion R_start = mobil.rotation;
        Quaternion R_end = Quaternion.AngleAxis(turnAngle, Vector3.up) * R_start;

        Vector3 forwardStart = R_start * Vector3.forward;
        Vector3 forwardEnd = R_end * Vector3.forward;

        // P0: Titik mulai lengkungan di lorong 1 (sebelum apex)
        Vector3 P0 = apex - (forwardStart * cornerRadius);
        // P2: Titik selesai lengkungan di lorong 2 (setelah apex mengikuti arah baru)
        Vector3 P2 = apex + (forwardEnd * cornerRadius);
        // P1: Titik sudut apex tikungan
        Vector3 P1 = apex;

        P0.y = mobil.position.y;
        P1.y = mobil.position.y;
        P2.y = mobil.position.y;

        // 1. Pastikan mobil melaju lurus di lorong 1 sampai tiba di titik P0
        float carSpeed = pod != null ? pod.speed : 5f;
        Vector3 toP0 = P0 - mobil.position;
        float distToP0 = Vector3.Dot(toP0, mobil.forward);

        if (distToP0 > 0.05f)
        {
            float maxApproachTime = (distToP0 / Mathf.Max(carSpeed, 1f)) + 1.0f;
            float approachTimer = 0f;

            while (approachTimer < maxApproachTime)
            {
                approachTimer += Time.deltaTime;
                Vector3 curToP0 = P0 - mobil.position;
                float curDist = Vector3.Dot(curToP0, mobil.forward);

                if (curDist <= 0.05f)
                {
                    break;
                }

                mobil.position += mobil.forward * carSpeed * Time.deltaTime;
                yield return null;
            }
        }

        Debug.Log("[TriggerMulti] Mobil tiba di titik P0 tikungan, mulai berbelok 90 derajat di dalam terowongan.");

        // 2. Eksekusi kurva lengkung Quadratic Bezier sempurna dari P0 -> P1 -> P2
        float waktu = 0f;
        while (waktu < turnDuration)
        {
            waktu += Time.deltaTime;
            float t = Mathf.Clamp01(waktu / turnDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            float u = 1f - smoothT;
            Vector3 curvePos = (u * u * P0) + (2f * u * smoothT * P1) + (smoothT * smoothT * P2);

            mobil.position = curvePos;
            mobil.rotation = Quaternion.Slerp(R_start, R_end, smoothT);

            yield return null;
        }

        mobil.position = P2;
        mobil.rotation = R_end;

        // 3. Alihkan target tujuan ke ujung terowongan TunnelMultimode (1)
        if (pod != null)
        {
            if (newEndPointAfterTurn != null)
            {
                pod.endPoint = newEndPointAfterTurn;
            }
            pod.isTurning = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = mobil.forward * carSpeed;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log("[TriggerMulti] Belokan tikungan selesai 100% di dalam terowongan, mobil meluncur lurus ke ujung keluar.");
    }
}