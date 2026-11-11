using UnityEngine;
using System.Collections.Generic;

public class InternetManager : MonoBehaviour
{
    public static InternetManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void RefreshInternetState()
    {
        NetworkDevice[] devices = FindObjectsByType<NetworkDevice>(FindObjectsSortMode.None);

        foreach (var d in devices)
            d.SetInternet(false);

        foreach (var d in devices)
        {
            if (d.isInternetSource)
                Spread(d);
        }
    }

    void Spread(NetworkDevice start)
    {
        Queue<NetworkDevice> queue = new();
        HashSet<NetworkDevice> visited = new();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            current.SetInternet(true);

            NetworkPort[] ports = current.GetComponentsInChildren<NetworkPort>();

            foreach (var p in ports)
            {
                if (p.ConnectedPort != null)
                {
                    var next = p.ConnectedPort.ParentDevice;

                    if (next != null && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
        }
    }
}