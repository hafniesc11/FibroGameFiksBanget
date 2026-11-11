using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DetachPart : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnGrab);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        transform.SetParent(null);
        Debug.Log(gameObject.name + " sudah terlepas dari induknya!");
    }
}