using UnityEngine;

public class NetworkDevice : MonoBehaviour
{
    public bool isInternetSource = false;
    public bool hasInternet = false;

    public void SetInternet(bool state)
    {
        hasInternet = state;
        Debug.Log($"{name} Internet = {state}");
    }
}