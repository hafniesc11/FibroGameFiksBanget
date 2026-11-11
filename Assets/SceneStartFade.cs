using UnityEngine;
using System.Collections;

public class SceneStartFade : MonoBehaviour 
{
    public CanvasGroup cg;

    void Start() {
        StartCoroutine(MulaiTerang());
    }

    IEnumerator MulaiTerang() {
        cg.alpha = 1; // Start gelap
        yield return new WaitForSeconds(0.5f); // Nunggu bentar biar stabil
        while (cg.alpha > 0) {
            cg.alpha -= Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false); // Matiin kalau udah terang
    }
}