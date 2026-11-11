using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CoreTriggerQuiz : MonoBehaviour
{
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    public s_QuizManager quizManager;

    bool sudahDiambil = false;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (sudahDiambil) return;

        sudahDiambil = true;

        StartCoroutine(TampilkanQuizSetelahDelay());
    }

    IEnumerator TampilkanQuizSetelahDelay()
    {
        yield return new WaitForSeconds(5f);

        quizManager.ShowQuiz();
    }
}