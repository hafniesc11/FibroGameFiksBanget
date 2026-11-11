using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty triggerValue;
    public InputActionProperty gripValue;

    private Animator handAnimator; // Diubah jadi private biar otomatis nyari

    void Start()
    {
        // Baris ini penting biar scriptnya nyari sendiri komponen Animator di tangan
        handAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        float trigger = triggerValue.action.ReadValue<float>();
        float grip = gripValue.action.ReadValue<float>();

        handAnimator.SetFloat("Trigger", trigger);
        handAnimator.SetFloat("Grip", grip);
    }
}