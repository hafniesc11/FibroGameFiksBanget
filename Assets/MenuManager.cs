using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomTeleporter : MonoBehaviour
{
    [Header("Transition Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;

    // Panggil fungsi ini dari Event Trigger pintu
    public void OpenDoor(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // Pastikan Canvas Group transisi aktif sebelum pindah scene
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0;

            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1;
        }

        SceneManager.LoadScene(sceneName);
    }
}