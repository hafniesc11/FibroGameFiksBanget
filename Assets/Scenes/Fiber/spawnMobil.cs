using UnityEngine;
using System.Collections.Generic;

public class TrafficPodSpawner : MonoBehaviour
{
    [Header("Prefab Model Truk Traffic")]
    [Tooltip("Tarik asset dump_truck.glb atau prefab truk traffic ke sini")]
    public GameObject trafficPodPrefab;

    [Header("Prefab Box Data")]
    [Tooltip("Tarik asset kardusbox.glb ke sini")]
    public GameObject boxPrefab;

    [Header("Truk Utama Sebagai Acuan")]
    public Transform ridePod;
    public PodMovement mainPodMovement;

    [Header("Tipe Media Transmisi")]
    [Tooltip("Centang jika terowongan Fiber Optik. Jangan centang jika Tembaga.")]
    public bool isFiberOptic = false;

    [Header("Pengaturan Konvoi Rapat / Dekat")]
    public int jumlahTraffic = 5;
    public float trafficLifeTime = 120f;

    private List<GameObject> spawnedTraffic = new List<GameObject>();
    private bool sudahSpawn = false;

    public void StartSpawn()
    {
        if (sudahSpawn) return;
        sudahSpawn = true;

        SpawnKonvoiTraffic();

        Debug.Log($"[TrafficPodSpawner] Konvoi rapat {spawnedTraffic.Count} truk mulai berjalan bersama player! (Fiber: {isFiberOptic})");
    }

    public void ResetSpawner()
    {
        HapusSemuaTraffic();
        sudahSpawn = false;

        Debug.Log("[TrafficPodSpawner] Traffic spawner di-reset");
    }

    private void SpawnKonvoiTraffic()
    {
        if (trafficPodPrefab == null)
        {
            Debug.LogWarning("[TrafficPodSpawner] Traffic Pod Prefab belum dimasukkan!");
            return;
        }

        if (ridePod == null)
        {
            GameObject podObj = GameObject.FindWithTag("Pod");
            if (podObj != null) ridePod = podObj.transform;
        }

        if (ridePod == null)
        {
            Debug.LogWarning("[TrafficPodSpawner] Ride Pod belum ditemukan!");
            return;
        }

        if (mainPodMovement == null)
        {
            mainPodMovement = ridePod.GetComponent<PodMovement>();
        }

        float baseSpeed = (mainPodMovement != null) ? mainPodMovement.speed : 5f;

        // Ambil skala root dan anak visual langsung dari CarUTP
        Vector3 rootScale = ridePod.localScale;
        Vector3 visualLocalScale = new Vector3(0.008215f, 0.008215f, 0.008215f);
        Quaternion visualLocalRot = Quaternion.Euler(-92.4f, 0f, 0.8f);

        Transform mainTruckVisual = ridePod.Find("dump_truck");
        if (mainTruckVisual != null)
        {
            visualLocalRot = mainTruckVisual.localRotation;
            visualLocalScale = mainTruckVisual.localScale;
        }

        // Formasi konvoi rapat di sekeliling POV player:
        Vector2[] tightConvoyFormation = new Vector2[]
        {
            new Vector2(-0.85f, 0.6f),  // Samping kiri player
            new Vector2( 0.85f, 0.6f),  // Samping kanan player
            new Vector2( 0.00f, 1.9f),  // Tepat di depan moncong truk player
            new Vector2(-0.65f, 3.6f),  // Depan kiri
            new Vector2( 0.65f, 3.6f)   // Depan kanan
        };

        int count = Mathf.Min(jumlahTraffic, tightConvoyFormation.Length);

        for (int i = 0; i < count; i++)
        {
            Vector2 formation = tightConvoyFormation[i];

            // Posisi spawn di dalam terowongan
            Vector3 spawnPos = ridePod.position
                             + (ridePod.forward * formation.y)
                             + (ridePod.right * formation.x);

            Vector3 moveDirection = ridePod.forward;
            Quaternion rootRot = Quaternion.LookRotation(moveDirection);

            // Root truk traffic
            GameObject trafficRoot = new GameObject($"Traffic_Truck_{i}");
            trafficRoot.transform.position = spawnPos;
            trafficRoot.transform.rotation = rootRot;
            trafficRoot.transform.localScale = rootScale;

            // Anak visual dump_truck
            GameObject visual = Instantiate(trafficPodPrefab, trafficRoot.transform);
            visual.name = "dump_truck";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = visualLocalRot;
            visual.transform.localScale = visualLocalScale;

            // Komponen pergerakan (disinkronkan dengan truk player)
            TrafficPodMovement movement = trafficRoot.AddComponent<TrafficPodMovement>();
            movement.moveDirection = moveDirection;
            movement.baseSpeed = baseSpeed;
            movement.speedOffset = 0f;
            movement.mainPodMovement = mainPodMovement;
            movement.lifeTime = Mathf.Max(trafficLifeTime, 120f);
            movement.isFiberOptic = isFiberOptic;

            // Komponen muatan kardus data
            TruckCargoManager cargo = trafficRoot.AddComponent<TruckCargoManager>();
            if (boxPrefab != null)
            {
                cargo.boxPrefab = boxPrefab;
            }
            cargo.isFiberOptic = isFiberOptic;
            cargo.autoAddHeadlight = false;
            cargo.SpawnCargo();

            movement.cargoManager = cargo;

            spawnedTraffic.Add(trafficRoot);
        }
    }

    public void TriggerDisturbOnTraffic()
    {
        BersihkanListKosong();

        foreach (var traffic in spawnedTraffic)
        {
            if (traffic != null)
            {
                TrafficPodMovement movement = traffic.GetComponent<TrafficPodMovement>();
                if (movement != null)
                {
                    movement.TriggerDisturbance();
                }
            }
        }

        Debug.Log($"[TrafficPodSpawner] Guncangan dipicu pada seluruh konvoi truk (Fiber: {isFiberOptic})");
    }

    private void BersihkanListKosong()
    {
        for (int i = spawnedTraffic.Count - 1; i >= 0; i--)
        {
            if (spawnedTraffic[i] == null || !spawnedTraffic[i].activeSelf)
            {
                spawnedTraffic.RemoveAt(i);
            }
        }
    }

    public void HilangkanTrafficAcak(int jumlah)
    {
    }

    public void HapusSemuaTraffic()
    {
        for (int i = spawnedTraffic.Count - 1; i >= 0; i--)
        {
            if (spawnedTraffic[i] != null)
                Destroy(spawnedTraffic[i]);
        }

        spawnedTraffic.Clear();
    }
}