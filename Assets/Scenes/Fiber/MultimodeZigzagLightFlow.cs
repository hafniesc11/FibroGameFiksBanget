using UnityEngine;

/// <summary>
/// Mengontrol partikel DataFlowKiri dan DataFlowKanan pada Terowongan Multimode
/// agar mengalir maju di sisi kiri & kanan (seperti SingleMode), namun masing-masing
/// bergerak memantul ZIGZAG di jalurnya sendiri tanpa menyeberang/bercampur di tengah.
/// </summary>
public class MultimodeZigzagLightFlow : MonoBehaviour
{
    [Header("Referensi Particle System DataFlow Multimode")]
    public ParticleSystem dataFlowKiri;
    public ParticleSystem dataFlowKanan;

    [Header("Pengaturan Laju & Pantulan Zigzag")]
    [Tooltip("Kecepatan laju maju partikel di sepanjang terowongan (m/s)")]
    public float kecepatanMaju = 3.5f;

    [Tooltip("Kekuatan ayunan pantulan zigzag di jalurnya masing-masing")]
    public float lebarPantulanJalur = 1.4f;

    [Tooltip("Berapa kali partikel memantul bolak-balik selama masa hidupnya")]
    public int jumlahPantulan = 3;

    [Tooltip("Lama hidup partikel (detik)")]
    public float lifetimePartikel = 4.0f;

    private void Awake()
    {
        CleanOldArtifacts();
        FindMultimodeDataFlows();
        ApplyZigzagToParticleSystems();
    }

    private void OnEnable()
    {
        FindMultimodeDataFlows();
        ApplyZigzagToParticleSystems();
    }

    private void Start()
    {
        FindMultimodeDataFlows();
        ApplyZigzagToParticleSystems();
    }

    private void CleanOldArtifacts()
    {
        string[] oldObjects = new string[] 
        { 
            "Multimode_BlueParticles_Flow", 
            "Multimode_Zigzag_DataFlow", 
            "Multimode_CyanZigzag_Flow",
            "Multimode_ZigzagFlow_Controller"
        };

        foreach (var name in oldObjects)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null && obj != gameObject)
            {
                Destroy(obj);
            }
        }
    }

    public void FindMultimodeDataFlows()
    {
        GameObject tunnelMulti = GameObject.Find("TunnelMultimode");
        if (tunnelMulti != null)
        {
            ParticleSystem[] pss = tunnelMulti.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pss)
            {
                string pName = ps.gameObject.name.ToLower();
                if (pName.Contains("kiri"))
                    dataFlowKiri = ps;
                else if (pName.Contains("kanan"))
                    dataFlowKanan = ps;
            }
        }
    }

    public void ApplyZigzagToParticleSystems()
    {
        if (dataFlowKiri == null && dataFlowKanan == null)
        {
            FindMultimodeDataFlows();
        }

        // Terapkan pantulan zigzag pada DataFlowKiri (di jalur kiri)
        if (dataFlowKiri != null)
        {
            ConfigureLaneStream(dataFlowKiri, false);
            Debug.Log("[MultimodeZigzagLightFlow] Jalur Kiri aktif memantul zigzag di sisi kiri.");
        }

        // Terapkan pantulan zigzag pada DataFlowKanan (di jalur kanan)
        if (dataFlowKanan != null)
        {
            ConfigureLaneStream(dataFlowKanan, true);
            Debug.Log("[MultimodeZigzagLightFlow] Jalur Kanan aktif memantul zigzag di sisi kanan.");
        }
    }

    private void ConfigureLaneStream(ParticleSystem ps, bool invertPhase)
    {
        // 1. Modul Utama (Tetap dalam koordinat lokal jalurnya)
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startLifetime = lifetimePartikel;
        main.startSpeed = 0f;

        // 2. Emisi Partikel
        var emission = ps.emission;
        emission.enabled = true;
        if (emission.rateOverTime.constant < 12f)
        {
            emission.rateOverTime = 16f;
        }

        // 3. Modul Velocity over Lifetime
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;

        // Sumbu Y lokal: Laju maju di sepanjang terowongan
        AnimationCurve forwardCurve = AnimationCurve.Constant(0f, 1f, kecepatanMaju);

        // Sumbu Z lokal: Pantulan osilasi ZIGZAG di jalurnya sendiri
        AnimationCurve zigzagCurve = GenerateOscillationCurve(lebarPantulanJalur, jumlahPantulan, invertPhase);

        // Sumbu X lokal: Sedikit ayunan naik-turun halus (0.2m)
        AnimationCurve verticalCurve = GenerateOscillationCurve(0.25f, jumlahPantulan, !invertPhase);

        // Pastikan seluruh sumbu menggunakan MinMaxCurve dengan mode yang seragam
        vel.x = new ParticleSystem.MinMaxCurve(1.0f, verticalCurve);
        vel.y = new ParticleSystem.MinMaxCurve(1.0f, forwardCurve);
        vel.z = new ParticleSystem.MinMaxCurve(1.0f, zigzagCurve);

        ps.Clear();
        ps.Play();
    }

    private AnimationCurve GenerateOscillationCurve(float amplitude, int cycles, bool invert)
    {
        AnimationCurve curve = new AnimationCurve();
        int sampleCount = cycles * 4;
        float dir = invert ? -1f : 1f;

        for (int i = 0; i <= sampleCount; i++)
        {
            float timeFraction = (float)i / sampleCount;
            // Kecepatan osilasi cosinusoidal: menghasilkan simpangan zigzag bolak-balik
            float speedValue = Mathf.Cos(timeFraction * cycles * Mathf.PI * 2f) * amplitude * dir;
            curve.AddKey(new Keyframe(timeFraction, speedValue));
        }

        for (int i = 0; i < curve.keys.Length; i++)
        {
            curve.SmoothTangents(i, 0f);
        }

        return curve;
    }
}
