using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace FOPC.Quiz
{
    /// <summary>
    /// Pengontrol Tampilan UI Kuis Dinamis Berbasis MURNI CROSSHAIR (FPS Game Style).
    /// Tanpa cursor mouse / kursor windows. Pemain memilih jawaban (A, B, C, D, E) dan tombol
    /// langsung membidik dengan titik crosshair tengah layar.
    /// </summary>
    public class DynamicQuizUI : MonoBehaviour
    {
        public static DynamicQuizUI Instance { get; private set; }

        [Header("1. Panel Input Siswa")]
        public GameObject panelInputSiswa;
        public TMP_InputField inputNamaSiswa;
        public TMP_InputField inputKelasSiswa;
        public Button btnMulaiKuis;

        [Header("2. Panel Soal Kuis")]
        public GameObject panelSoal;
        public TMP_Text txtJudulMeeting;
        public TMP_Text txtNomorSoal;
        public TMP_Text txtPertanyaan;

        [System.Serializable]
        public class ChoiceButtonUI
        {
            public Button button;
            public TMP_Text txtLabel; // "A", "B", "C", "D", "E"
            public TMP_Text txtPilihan; // Isi teks pilihan jawaban
        }

        [Tooltip("5 Tombol Pilihan Jawaban (A, B, C, D, E)")]
        public List<ChoiceButtonUI> choiceButtons = new List<ChoiceButtonUI>();

        [Header("Warna Feedback Jawaban")]
        public Color defaultBtnColor = new Color(0.12f, 0.18f, 0.28f, 0.95f);
        public Color correctBtnColor = new Color(0.1f, 0.78f, 0.35f, 1.0f);
        public Color wrongBtnColor = new Color(0.88f, 0.22f, 0.22f, 1.0f);
        public Color dimmedBtnColor = new Color(0.08f, 0.12f, 0.18f, 0.5f);

        [Header("3. Panel Loading & Status")]
        public GameObject panelLoading;
        public TMP_Text txtLoadingMessage;

        [Header("4. Panel Hasil Skor Akhir")]
        public GameObject panelHasil;
        public TMP_Text txtNamaHasil;
        public TMP_Text txtSkorAkhir;
        public TMP_Text txtDetailBenar;
        public Button btnTutupHasil;
        public Button btnCobaLagi;

        [Header("5. Dialog Error / Notifikasi")]
        public GameObject panelError;
        public TMP_Text txtErrorMessage;
        public Button btnTutupError;

        private DynamicQuizManager quizManager;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            quizManager = DynamicQuizManager.Instance != null ? DynamicQuizManager.Instance : FindFirstObjectByType<DynamicQuizManager>();

            // Setup Event Listener Tombol
            if (btnMulaiKuis != null)
            {
                DisableNavigation(btnMulaiKuis);
                btnMulaiKuis.onClick.AddListener(OnClickMulaiKuis);
            }

            if (btnTutupError != null)
            {
                DisableNavigation(btnTutupError);
                btnTutupError.onClick.AddListener(HideAllPanelsAndShowInput);
            }

            if (btnCobaLagi != null)
            {
                DisableNavigation(btnCobaLagi);
                btnCobaLagi.onClick.AddListener(OnClickCobaLagi);
            }

            if (btnTutupHasil != null)
            {
                DisableNavigation(btnTutupHasil);
                btnTutupHasil.onClick.AddListener(OnClickTutupHasil);
            }

            if (inputNamaSiswa != null)
            {
                DisableNavigation(inputNamaSiswa);
            }
            if (inputKelasSiswa != null)
            {
                DisableNavigation(inputKelasSiswa);
            }

            // Matikan navigasi UI Unity secara permanen agar tombol WASD dan Spasi tidak tertahan
            if (EventSystem.current != null)
            {
                EventSystem.current.sendNavigationEvents = false;
            }
        }

        private void DisableNavigation(Selectable selectable)
        {
            if (selectable != null)
            {
                var nav = selectable.navigation;
                nav.mode = Navigation.Mode.None;
                selectable.navigation = nav;
            }
        }

        private void HideAllPanels()
        {
            if (panelInputSiswa != null) panelInputSiswa.SetActive(false);
            if (panelSoal != null) panelSoal.SetActive(false);
            if (panelLoading != null) panelLoading.SetActive(false);
            if (panelHasil != null) panelHasil.SetActive(false);
            if (panelError != null) panelError.SetActive(false);
        }

        public void ShowStudentInputPanel()
        {
            HideAllPanels();
            if (panelInputSiswa != null) panelInputSiswa.SetActive(true);
        }

        private void HideAllPanelsAndShowInput()
        {
            ShowStudentInputPanel();
        }

        public void ShowLoading(string message = "Memuat Soal...")
        {
            HideAllPanels();
            if (panelLoading != null) panelLoading.SetActive(true);
            if (txtLoadingMessage != null) txtLoadingMessage.text = message;
        }

        public void ShowErrorMessage(string error)
        {
            HideAllPanels();
            if (panelError != null) panelError.SetActive(true);
            if (txtErrorMessage != null) txtErrorMessage.text = error;
            Debug.LogError($"[DynamicQuizUI] {error}");
        }

        private void OnClickMulaiKuis()
        {
            string nama = inputNamaSiswa != null ? inputNamaSiswa.text.Trim() : "";
            string kelas = inputKelasSiswa != null ? inputKelasSiswa.text.Trim() : "";

            if (quizManager != null)
            {
                quizManager.StartQuiz(nama, kelas);
            }
        }

        /// <summary>
        /// Menampilkan pertanyaan dan pilihan jawaban (A, B, C, D, E) untuk dipilih via crosshair.
        /// </summary>
        public void DisplayQuestion(string meetingTitle, int currentNum, int totalNum, Question question, List<Choice> choices)
        {
            HideAllPanels();
            if (panelSoal != null) panelSoal.SetActive(true);

            if (txtJudulMeeting != null)
                txtJudulMeeting.text = meetingTitle;

            if (txtNomorSoal != null)
                txtNomorSoal.text = $"Soal {currentNum} / {totalNum}";

            if (txtPertanyaan != null)
                txtPertanyaan.text = question.question_text;

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                ChoiceButtonUI btnUI = choiceButtons[i];

                if (i < choices.Count)
                {
                    Choice choiceData = choices[i];
                    btnUI.button.gameObject.SetActive(true);
                    btnUI.button.interactable = true;
                    DisableNavigation(btnUI.button);

                    Image img = btnUI.button.GetComponent<Image>();
                    if (img != null) img.color = defaultBtnColor;

                    if (btnUI.txtLabel != null)
                    {
                        btnUI.txtLabel.text = $"{choiceData.choice_label}.";
                        btnUI.txtLabel.color = new Color(0.1f, 0.85f, 1.0f);
                    }

                    if (btnUI.txtPilihan != null)
                    {
                        btnUI.txtPilihan.text = choiceData.choice_text;
                        btnUI.txtPilihan.color = Color.white;
                    }

                    btnUI.button.onClick.RemoveAllListeners();
                    btnUI.button.onClick.AddListener(() =>
                    {
                        if (quizManager != null)
                        {
                            quizManager.OnChoiceSelected(choiceData);
                        }
                    });
                }
                else
                {
                    btnUI.button.gameObject.SetActive(false);
                }
            }
        }

        public void ShowAnswerFeedback(Choice selectedChoice, List<Choice> currentChoices)
        {
            for (int i = 0; i < choiceButtons.Count; i++)
            {
                ChoiceButtonUI btnUI = choiceButtons[i];

                if (i < currentChoices.Count)
                {
                    Choice c = currentChoices[i];
                    btnUI.button.interactable = false;

                    Image img = btnUI.button.GetComponent<Image>();
                    if (img != null)
                    {
                        if (c.id == selectedChoice.id)
                        {
                            img.color = selectedChoice.is_correct ? correctBtnColor : wrongBtnColor;
                            if (btnUI.txtLabel != null) btnUI.txtLabel.color = Color.white;
                        }
                        else if (c.is_correct)
                        {
                            img.color = correctBtnColor;
                            if (btnUI.txtLabel != null) btnUI.txtLabel.color = Color.white;
                        }
                        else
                        {
                            img.color = dimmedBtnColor;
                        }
                    }
                }
            }
        }

        public void ShowResultPanel(string studentName, string studentClass, float score, int correctCount, int totalQuestions)
        {
            HideAllPanels();
            if (panelHasil != null) panelHasil.SetActive(true);

            if (txtNamaHasil != null)
                txtNamaHasil.text = $"{studentName} ({studentClass})";

            if (txtSkorAkhir != null)
                txtSkorAkhir.text = $"{score:F1} / 100";

            if (txtDetailBenar != null)
                txtDetailBenar.text = $"Jawaban Benar: {correctCount} dari {totalQuestions} Soal";
        }

        private void OnClickCobaLagi()
        {
            if (quizManager != null)
            {
                quizManager.RestartQuiz();
            }
        }

        private void OnClickTutupHasil()
        {
            HideAllPanels();
            ShowStudentInputPanel();
        }
    }
}
