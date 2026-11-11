using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("UI Panels (Opsional jika scene ada panel)")]
    public GameObject mainPanel;      // P-Mainme
    public GameObject optionsPanel;   // Options
    public GameObject aboutPanel;     // About

    [Header("Audio SFX Names")]
    public string clickSound = "uiclick";
    public string hoverEnterSound = "uihoverenter";
    public string hoverExitSound = "uihoverexit";

    private void Start()
    {
        // Jika scene ini punya panel main menu, reset tampilannya saat mulai
        if (mainPanel != null)
        {
            ShowMainPanel();
        }
    }

    // --- FUNGSI SUARA ---

    public void PlayClickSound()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play(clickSound);
    }

    public void PlayHoverEnterSound()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play(hoverEnterSound);
    }

    public void PlayHoverExitSound()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play(hoverExitSound);
    }

    // --- FUNGSI PINDAH SCENE ---

    public void GoTo2_LobbyRuangan()
    {
        PlayClickSound();
        SceneManager.LoadScene("2_LobbyRuangan"); 
    }

    public void GoToPert2()
    {
        PlayClickSound();
        SceneManager.LoadScene("Pert2"); 
    }

    public void GoToPert3()
    {
        PlayClickSound();
        SceneManager.LoadScene("Pert3");
    }

    public void GoToPert4()
    {
        PlayClickSound();
        SceneManager.LoadScene("Pert4");
    }

    // --- FUNGSI PANEL UI (START SCENE) ---

    public void OpenOptions()
    {
        PlayClickSound();
        if (mainPanel) mainPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
        if (aboutPanel) aboutPanel.SetActive(false);
    }

    public void OpenAbout()
    {
        PlayClickSound();
        if (mainPanel) mainPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (aboutPanel) aboutPanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        PlayClickSound();
        if (mainPanel) mainPanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (aboutPanel) aboutPanel.SetActive(false);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Keluar dari Game");
        Application.Quit();
    }
}