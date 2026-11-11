using UnityEngine;

public class BlinkLight : MonoBehaviour
{
    public Renderer ledRenderer;
    public Light pointLight;

    public float blinkSpeed = 0.5f;
    public float intensity = 10f;
    public float lightIntensity = 5f;   

    private Material mat;
    private bool blinking = false;
    private bool lampOn = false;
    private float timer = 0f;

    void Start()
    {
        mat = ledRenderer.material;

        // Pastikan emission aktif
        mat.EnableKeyword("_EMISSION");

        // Awal mati
        mat.SetColor("_EmissionColor", Color.black);

        if(pointLight != null)
        {
            pointLight.intensity = 0;
        }
    }

    void Update()
    {
        if (!blinking) return;

        timer += Time.deltaTime;

        if (timer >= blinkSpeed)
        {
            timer = 0f;

            lampOn = !lampOn;

            if (lampOn){
                mat.SetColor("_EmissionColor", Color.red * intensity);

                if(pointLight != null)
                    pointLight.intensity = lightIntensity;
            }
            else{
                mat.SetColor("_EmissionColor", Color.black);

                if(pointLight != null)
                    pointLight.intensity = 0;
            }
        }
    }

    public void StartBlink()
    {
        blinking = true;
    }

    public void StopBlink(){
        blinking = false;
        lampOn = false;

        mat.SetColor("_EmissionColor", Color.black);

        if(pointLight != null)
            pointLight.intensity = 0;
    }
}