using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("Panel Utama")]
    public GameObject pMainMenu;

    [Header("Panel Tentang")]
    public GameObject pAbout;

    [Header("Panel CP / ATP")]
    public GameObject pCPATP;

    [Header("Panel Pengembang")]
    public GameObject pPengembang;

    [Header("Panel Petunjuk")]
    public GameObject pPetunjuk;    // Panel Petunjuk Utama
    public GameObject pPetunjuk2;   // Panel Petunjuk Halaman 2

    [Header("Panel Asset")]
    public GameObject pAsset;       // Panel Asset Game / Visual
    public GameObject pMateri;      // Panel Asset Materi (Pengganti pAsset2)

    [Header("Panel Audio")]
    public GameObject pAudio;

    [Header("Volume Musik")]
    public Slider volumeSlider;
    public AudioSource musicSource;

    [Header("Icon Volume")]
    public Image volumeIcon;
    public Sprite volumeOnIcon;
    public Sprite volumeMuteIcon;

    [Header("Nama Scene Lobby")]
    public string lobbySceneName = "MainMenu2";


    private void Start()
    {
        // Pastikan cursor bisa digunakan untuk klik UI di menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tampilkan menu utama
        TampilkanMainMenu();

        // Atur volume awal
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        UpdateVolumeIcon(AudioListener.volume);
    }

    // START GAME
    public void StartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(lobbySceneName);
    }

    // PETUNJUK (HALAMAN 1 & 2)
    public void OpenPetunjuk()
    {
        TutupSemuaPanel();
        if (pPetunjuk != null) pPetunjuk.SetActive(true);
    }

    public void OpenPetunjuk2()
    {
        TutupSemuaPanel();
        if (pPetunjuk2 != null) pPetunjuk2.SetActive(true);
    }

    /// Pindah dari Petunjuk 2 kembali ke Petunjuk Utama (Tombol Prev)
    public void PrevPetunjuk()
    {
        if (pPetunjuk2 != null) pPetunjuk2.SetActive(false);
        if (pPetunjuk != null) pPetunjuk.SetActive(true);
    }

    // ASSET

    public void OpenAsset()
    {
        TutupSemuaPanel();
        if (pAsset != null) pAsset.SetActive(true);
    }

    public void OpenAssetFromAbout()
    {
        TutupSemuaPanel();
        if (pAsset != null) pAsset.SetActive(true);
    }

    // ASSET MATERI
    public void OpenMateri()
    {
        TutupSemuaPanel();
        if (pMateri != null) pMateri.SetActive(true);
    }

    // TENTANG, CP / ATP, PENGEMBANG
    public void OpenAbout()
    {
        TutupSemuaPanel();
        if (pAbout != null) pAbout.SetActive(true);
    }

    public void OpenCPATP()
    {
        TutupSemuaPanel();
        if (pCPATP != null) pCPATP.SetActive(true);
    }

    public void OpenPengembang()
    {
        TutupSemuaPanel();
        if (pPengembang != null) pPengembang.SetActive(true);
    }

    public void OpenAudio()
    {
        TutupSemuaPanel();

        if (pAudio != null)
            pAudio.SetActive(true);
}
    // TOMBOL KEMBALI (BACK)
    public void BackFromPetunjuk()
    {
        TutupSemuaPanel();
        if (pMainMenu != null) pMainMenu.SetActive(true);
    }

    public void BackFromAsset()
    {
        TutupSemuaPanel();
        if (pMainMenu != null) pMainMenu.SetActive(true);
    }

    public void BackFromAssetToAbout()
    {
        TutupSemuaPanel();
        if (pAbout != null) pAbout.SetActive(true);
    }

    public void BackFromMateri()
    {
        TutupSemuaPanel();
        if (pMainMenu != null) pMainMenu.SetActive(true);
    }

    public void BackFromMateriToAbout()
    {
        TutupSemuaPanel();
        if (pAbout != null) pAbout.SetActive(true);
    }

    public void BackFromCPATP()
    {
        TutupSemuaPanel();
        if (pAbout != null) pAbout.SetActive(true);
    }

    public void BackFromPengembang()
    {
        TutupSemuaPanel();
        if (pAbout != null) pAbout.SetActive(true);
    }

    public void BackFromAbout()
    {
        TutupSemuaPanel();
        if (pMainMenu != null) pMainMenu.SetActive(true);
    }

    // HOME & UTILITY
    public void Home()
    {
        TampilkanMainMenu();
    }

    public void SetVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        AudioListener.volume = volume;
        UpdateVolumeIcon(volume);
    }

    private void UpdateVolumeIcon(float volume)
    {
        if (volumeIcon == null)
            return;

        if (volume <= 0.001f)
        {
            volumeIcon.sprite = volumeMuteIcon;
        }
        else
        {
            volumeIcon.sprite = volumeOnIcon;
        }
    }
    
    public void ExitGame()
    {
        Debug.Log("[GameStartMenu] Menutup dan keluar dari aplikasi...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // HELPER TUTUP SEMUA PANEL
    private void TutupSemuaPanel()
    {
        if (pMainMenu != null) pMainMenu.SetActive(false);
        if (pAbout != null) pAbout.SetActive(false);
        if (pCPATP != null) pCPATP.SetActive(false);
        if (pPengembang != null) pPengembang.SetActive(false);
        if (pPetunjuk != null) pPetunjuk.SetActive(false);
        if (pPetunjuk2 != null) pPetunjuk2.SetActive(false);
        if (pAsset != null) pAsset.SetActive(false);
        if (pMateri != null) pMateri.SetActive(false);
        if (pAudio != null) pAudio.SetActive(false);
    }

    private void TampilkanMainMenu()
    {
        TutupSemuaPanel();
        if (pMainMenu != null) pMainMenu.SetActive(true);
    }
}