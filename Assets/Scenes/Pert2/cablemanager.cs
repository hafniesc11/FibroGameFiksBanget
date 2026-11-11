using UnityEngine;

public class cablemanager : MonoBehaviour
{
    public static cablemanager Instance;

    public enum CableType
    {
        None,
        Copper,
        Fiber
    }

    public CableType selectedCable = CableType.None;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetCable()
{
    selectedCable = CableType.Copper; // atau nilai default yang kamu inginkan
}
}