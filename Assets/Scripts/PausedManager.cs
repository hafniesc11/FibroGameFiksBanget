using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [Header("PAUSE")]
    public GameObject pausePanel;

    [Header("PLAYER")]
    public Behaviour playerController;

    [Header("VOLUME")]
    public Slider volumeSlider;
    public Image volumeIcon;

    [Header("VOLUME ICON")]
    public Sprite volumeOnIcon;
    public Sprite volumeMuteIcon;

    private bool isPaused = false;

    private void Awake()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Start()
    {
        // PausePanel ditutup saat scene mulai
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Setup volume
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetVolume);

            volumeSlider.value = AudioListener.volume;

            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        UpdateVolumeIcon(AudioListener.volume);

        // Gameplay: mouse terkunci
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Tekan ESC untuk Pause / Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        // Selama pause, paksa mouse tetap muncul
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // =====================================================
    // PAUSE
    // =====================================================

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        // Stop waktu game
        Time.timeScale = 0f;

        // Matikan kontrol player
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Tampilkan pause panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Hilangkan focus tombol
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Tampilkan mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // =====================================================
    // LANJUTKAN
    // =====================================================

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        // Aktifkan player
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Sembunyikan pause
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Hilangkan focus tombol
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Kunci mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // =====================================================
    // MULAI ULANG
    // =====================================================

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // Ambil scene yang sedang dimainkan
        string currentScene = SceneManager.GetActiveScene().name;

        // Reload scene tersebut
        SceneManager.LoadScene(currentScene);
    }

    // =====================================================
    // MAIN MENU
    // =====================================================

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("1_MainMenu");
    }

    // =====================================================
    // LOBBY
    // =====================================================

    public void GoToLobby()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("2_LobbyRuangan");
    }

    // =====================================================
    // EXIT GAME
    // =====================================================

    public void ExitGame()
    {
        Time.timeScale = 1f;

        #if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;

        #else

        Application.Quit();

        #endif
    }

    // =====================================================
    // VOLUME
    // =====================================================

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        UpdateVolumeIcon(volume);
    }

    private void UpdateVolumeIcon(float volume)
    {
        if (volumeIcon == null)
            return;

        if (volume <= 0.001f)
        {
            if (volumeMuteIcon != null)
            {
                volumeIcon.sprite = volumeMuteIcon;
            }
        }
        else
        {
            if (volumeOnIcon != null)
            {
                volumeIcon.sprite = volumeOnIcon;
            }
        }
    }
}