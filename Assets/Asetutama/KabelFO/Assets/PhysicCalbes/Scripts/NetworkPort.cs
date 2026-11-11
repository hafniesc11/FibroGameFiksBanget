using UnityEngine;

public class NetworkPort : MonoBehaviour
{
    [SerializeField] private NetworkDevice parentDevice;
    [SerializeField] private NetworkPort connectedPort;

    public NetworkDevice ParentDevice => parentDevice;
    public NetworkPort ConnectedPort => connectedPort;

    private void Awake()
    {
        parentDevice = GetComponentInParent<NetworkDevice>();
    }

    public void ConnectTo(NetworkPort other)
    {
        if (other == null) return;

        connectedPort = other;
        other.connectedPort = this;

        InternetManager.Instance.RefreshInternetState();
    }

    public void Disconnect()
    {
        if (connectedPort == null) return;

        NetworkPort temp = connectedPort;

        connectedPort = null;
        temp.connectedPort = null;

        InternetManager.Instance.RefreshInternetState();
    }
}