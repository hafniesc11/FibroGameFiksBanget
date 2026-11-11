using UnityEngine;
using System.Collections;

public class AlarmServer : MonoBehaviour
{
    public GameObject alarmLight;

    private AudioSource alarmSound;
    Coroutine blinkRoutine;

    void Start()
    {
        alarmSound = alarmLight.GetComponent<AudioSource>();
    }

    public void AktifkanAlarm()
    {
        alarmSound.Play();
        blinkRoutine = StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            alarmLight.SetActive(true);
            yield return new WaitForSeconds(0.4f);

            alarmLight.SetActive(false);
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void MatikanAlarm()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        alarmLight.SetActive(false);
        alarmSound.Stop();
    }
}