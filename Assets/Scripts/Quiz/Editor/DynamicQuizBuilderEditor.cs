using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace FOPC.Quiz
{
    /// <summary>
    /// Editor Tool untuk membuat & men-setup Canvas Kuis 3D ukuran COMPACT & RAMPING
    /// Mendukung 5 Pilihan Ganda (A, B, C, D, E) secara dinamis.
    /// </summary>
    public class DynamicQuizBuilderEditor : MonoBehaviour
    {
        [MenuItem("Tools/Kuis/⚡ Buat Canvas Kuis Otomatis (1 Klik)", false, 1)]
        public static void GenerateQuizCanvasAutomatically()
        {
            // Pastikan ada EventSystem di scene agar interaksi klik/ketik berfungsi
            EnsureEventSystem();

            // Hapus canvas kuis lama jika ada agar bersih
            GameObject oldCanvas = GameObject.Find("C_Kuis");
            if (oldCanvas != null)
            {
                DestroyImmediate(oldCanvas);
            }

            // 1. Buat Root Canvas 3D (Ukuran Ringkas & Pas: 860 x 580 pixel)
            GameObject canvasObj = new GameObject("C_Kuis");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            Camera mainCam = Camera.main;
            if (mainCam != null) canvas.worldCamera = mainCam;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;
            scaler.referencePixelsPerUnit = 100f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Pasang TrackedDeviceGraphicRaycaster jika ada package XR Interaction Toolkit
            Type xrRaycasterType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
            if (xrRaycasterType != null)
            {
                canvasObj.AddComponent(xrRaycasterType);
            }

            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(860f, 580f);
            canvasRect.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f); // Skala pas di ruang 3D (1.55m x 1.0m)
            
            // Posisikan pas setinggi mata player di ruang server
            canvasRect.position = new Vector3(1.68f, 1.15f, -10.58f);
            canvasRect.rotation = Quaternion.Euler(0f, -90f, 0f);

            // 2. Buat Background Utama (Dark Glass Transparan Futuristik)
            GameObject bgObj = CreateUIElement("Background_Utama", canvasObj.transform);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.10f, 0.17f, 0.95f);
            bgImg.raycastTarget = false;
            SetStretch(bgObj.GetComponent<RectTransform>());

            // Pasang Script DynamicQuizUI
            DynamicQuizUI quizUI = canvasObj.AddComponent<DynamicQuizUI>();

            // ========================================================
            // PANEL 1: INPUT IDENTITAS SISWA
            // ========================================================
            GameObject pInput = CreateUIElement("1_PanelInputSiswa", canvasObj.transform);
            SetStretch(pInput.GetComponent<RectTransform>());
            quizUI.panelInputSiswa = pInput;

            // Judul Panel Input
            CreateText(pInput.transform, "Text_Judul", "KUIS EVALUASI PEMBELAJARAN", 
                new Vector2(0f, 190f), new Vector2(750f, 50f), 32, TextAlignmentOptions.Center, new Color(0.1f, 0.85f, 1.0f));

            CreateText(pInput.transform, "Text_Subjudul", "Silakan masukkan nama dan kelas Anda untuk memulai kuis evaluasi.", 
                new Vector2(0f, 140f), new Vector2(750f, 35f), 18, TextAlignmentOptions.Center, new Color(0.85f, 0.9f, 0.95f));

            // InputField Nama
            CreateText(pInput.transform, "Label_Nama", "NAMA SISWA:", 
                new Vector2(0f, 85f), new Vector2(600f, 25f), 15, TextAlignmentOptions.Left, Color.yellow);
            TMP_InputField inputNama = CreateInputField(pInput.transform, "InputField_Nama", "Ketik nama lengkap Anda di sini...", new Vector2(0f, 45f), new Vector2(600f, 50f));
            quizUI.inputNamaSiswa = inputNama;

            // InputField Kelas
            CreateText(pInput.transform, "Label_Kelas", "KELAS / JURUSAN:", 
                new Vector2(0f, -12f), new Vector2(600f, 25f), 15, TextAlignmentOptions.Left, Color.yellow);
            TMP_InputField inputKelas = CreateInputField(pInput.transform, "InputField_Kelas", "Contoh: XI TKJ 1 / XI RPL 2...", new Vector2(0f, -52f), new Vector2(600f, 50f));
            quizUI.inputKelasSiswa = inputKelas;

            // Tombol Mulai Kuis
            Button btnMulai = CreateButton(pInput.transform, "Button_MulaiKuis", "MULAI KERJAKAN KUIS >", 
                new Vector2(0f, -155f), new Vector2(600f, 60f), new Color(0.0f, 0.65f, 0.35f), Color.white, 22);
            quizUI.btnMulaiKuis = btnMulai;


            // ========================================================
            // PANEL 2: SOAL & PILIHAN GANDA (A, B, C, D, E)
            // ========================================================
            GameObject pSoal = CreateUIElement("2_PanelSoal", canvasObj.transform);
            SetStretch(pSoal.GetComponent<RectTransform>());
            pSoal.SetActive(false);
            quizUI.panelSoal = pSoal;

            // Header Meeting & Nomor Soal
            quizUI.txtJudulMeeting = CreateText(pSoal.transform, "Text_JudulMeeting", "Pertemuan 1: Pengenalan Fiber Optik", 
                new Vector2(-80f, 235f), new Vector2(550f, 35f), 20, TextAlignmentOptions.Left, new Color(0.1f, 0.85f, 1.0f));

            quizUI.txtNomorSoal = CreateText(pSoal.transform, "Text_NomorSoal", "Soal 1 / 5", 
                new Vector2(290f, 235f), new Vector2(180f, 35f), 18, TextAlignmentOptions.Right, Color.yellow);

            // Kotak Pertanyaan
            GameObject qBox = CreateUIElement("Box_Pertanyaan", pSoal.transform);
            Image qBoxImg = qBox.AddComponent<Image>();
            qBoxImg.color = new Color(0.11f, 0.16f, 0.25f, 0.95f);
            qBoxImg.raycastTarget = false;
            RectTransform qBoxRect = qBox.GetComponent<RectTransform>();
            qBoxRect.anchoredPosition = new Vector2(0f, 155f);
            qBoxRect.sizeDelta = new Vector2(780f, 95f);

            quizUI.txtPertanyaan = CreateText(qBox.transform, "Text_Pertanyaan", "Pertanyaan akan dimuat otomatis dari database Supabase...", 
                Vector2.zero, new Vector2(740f, 80f), 19, TextAlignmentOptions.Left, Color.white);
            quizUI.txtPertanyaan.enableWordWrapping = true;

            // 5 Tombol Pilihan Jawaban (A, B, C, D, E)
            quizUI.choiceButtons.Clear();
            string[] labels = new string[] { "A", "B", "C", "D", "E" };
            float startY = 60f;
            float stepY = 56f;

            for (int i = 0; i < 5; i++)
            {
                float yPos = startY - (i * stepY);
                GameObject btnChoiceObj = CreateChoiceButtonElement(pSoal.transform, $"Button_{labels[i]}", labels[i], 
                    new Vector2(0f, yPos), new Vector2(780f, 48f));

                DynamicQuizUI.ChoiceButtonUI choiceUI = new DynamicQuizUI.ChoiceButtonUI();
                choiceUI.button = btnChoiceObj.GetComponent<Button>();
                choiceUI.txtLabel = btnChoiceObj.transform.Find("Text_Label").GetComponent<TMP_Text>();
                choiceUI.txtPilihan = btnChoiceObj.transform.Find("Text_Pilihan").GetComponent<TMP_Text>();

                quizUI.choiceButtons.Add(choiceUI);
            }


            // ========================================================
            // PANEL 3: LOADING SCREEN
            // ========================================================
            GameObject pLoading = CreateUIElement("3_PanelLoading", canvasObj.transform);
            SetStretch(pLoading.GetComponent<RectTransform>());
            pLoading.SetActive(false);
            quizUI.panelLoading = pLoading;

            quizUI.txtLoadingMessage = CreateText(pLoading.transform, "Text_LoadingMessage", "Memuat Soal...", 
                Vector2.zero, new Vector2(700f, 100f), 28, TextAlignmentOptions.Center, new Color(0.1f, 0.85f, 1.0f));


            // ========================================================
            // PANEL 4: HASIL SKOR AKHIR
            // ========================================================
            GameObject pHasil = CreateUIElement("4_PanelHasil", canvasObj.transform);
            SetStretch(pHasil.GetComponent<RectTransform>());
            pHasil.SetActive(false);
            quizUI.panelHasil = pHasil;

            CreateText(pHasil.transform, "Text_HeaderHasil", "HASIL EVALUASI KUIS", 
                new Vector2(0f, 190f), new Vector2(750f, 45f), 32, TextAlignmentOptions.Center, Color.yellow);

            quizUI.txtNamaHasil = CreateText(pHasil.transform, "Text_NamaHasil", "Budi Santoso (XI TKJ 1)", 
                new Vector2(0f, 130f), new Vector2(750f, 35f), 20, TextAlignmentOptions.Center, Color.white);

            quizUI.txtSkorAkhir = CreateText(pHasil.transform, "Text_SkorAkhir", "100.0 / 100", 
                new Vector2(0f, 40f), new Vector2(750f, 90f), 55, TextAlignmentOptions.Center, new Color(0.15f, 1.0f, 0.4f));

            quizUI.txtDetailBenar = CreateText(pHasil.transform, "Text_DetailBenar", "Jawaban Benar: 5 dari 5 Soal", 
                new Vector2(0f, -45f), new Vector2(750f, 35f), 18, TextAlignmentOptions.Center, new Color(0.85f, 0.9f, 0.95f));

            quizUI.btnCobaLagi = CreateButton(pHasil.transform, "Button_CobaLagi", "ULANGI KUIS", 
                new Vector2(-180f, -150f), new Vector2(300f, 55f), new Color(0.15f, 0.45f, 0.85f), Color.white, 18);

            quizUI.btnTutupHasil = CreateButton(pHasil.transform, "Button_Selesai", "SELESAI", 
                new Vector2(180f, -150f), new Vector2(300f, 55f), new Color(0.0f, 0.65f, 0.35f), Color.white, 18);


            // ========================================================
            // PANEL 5: ERROR SCREEN
            // ========================================================
            GameObject pError = CreateUIElement("5_PanelError", canvasObj.transform);
            SetStretch(pError.GetComponent<RectTransform>());
            pError.SetActive(false);
            quizUI.panelError = pError;

            CreateText(pError.transform, "Text_HeaderError", "TERJADI KESALAHAN", 
                new Vector2(0f, 140f), new Vector2(750f, 45f), 28, TextAlignmentOptions.Center, Color.red);

            quizUI.txtErrorMessage = CreateText(pError.transform, "Text_ErrorMessage", "Gagal memuat soal kuis.", 
                new Vector2(0f, 20f), new Vector2(700f, 120f), 18, TextAlignmentOptions.Center, Color.white);
            quizUI.txtErrorMessage.enableWordWrapping = true;

            quizUI.btnTutupError = CreateButton(pError.transform, "Button_TutupError", "KEMBALI KE MENU", 
                new Vector2(0f, -140f), new Vector2(400f, 55f), new Color(0.75f, 0.2f, 0.2f), Color.white, 18);


            // ========================================================
            // 3. BUAT GAME OBJECT QUIZSYSTEM (MANAGER)
            // ========================================================
            GameObject managerObj = GameObject.Find("QuizSystem");
            if (managerObj == null)
            {
                managerObj = new GameObject("QuizSystem");
            }

            DynamicQuizManager quizManager = managerObj.GetComponent<DynamicQuizManager>();
            if (quizManager == null)
            {
                quizManager = managerObj.AddComponent<DynamicQuizManager>();
            }

            quizManager.quizUI = quizUI;

            Selection.activeGameObject = canvasObj;

            EditorUtility.DisplayDialog("Sukses!", "Canvas Kuis C_Kuis berhasil di-setup dengan 5 Tombol Pilihan Ganda (A, B, C, D, E)!", "Mantap!");
            Debug.Log("<color=green>[DynamicQuizBuilder] Canvas Kuis C_Kuis siap untuk 5 pilihan jawaban (A, B, C, D, E)!</color>");
        }

        #region Helper UI Factory
        private static void EnsureEventSystem()
        {
            EventSystem es = FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                
                Type inputModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputModuleType != null)
                {
                    esObj.AddComponent(inputModuleType);
                }
                else
                {
                    esObj.AddComponent<StandaloneInputModule>();
                }
            }
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");
            return go;
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions align, Color color)
        {
            GameObject obj = CreateUIElement(name, parent);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = fontSize * 0.65f;
            tmp.fontSizeMax = fontSize * 1.1f;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Color btnColor, Color txtColor, float fontSize)
        {
            GameObject obj = CreateUIElement(name, parent);
            Image img = obj.AddComponent<Image>();
            img.color = btnColor;
            img.raycastTarget = true;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            CreateText(obj.transform, "Text_Button", text, Vector2.zero, size * 0.9f, fontSize, TextAlignmentOptions.Center, txtColor);
            return btn;
        }

        private static TMP_InputField CreateInputField(Transform parent, string name, string placeholderText, Vector2 pos, Vector2 size)
        {
            GameObject obj = CreateUIElement(name, parent);
            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.17f, 0.25f, 0.95f);
            bg.raycastTarget = true;

            TMP_InputField input = obj.AddComponent<TMP_InputField>();
            input.targetGraphic = bg;
            input.interactable = true;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // Text Area
            GameObject textArea = CreateUIElement("Text Area", obj.transform);
            RectTransform taRect = textArea.GetComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.sizeDelta = new Vector2(-16f, -8f);
            taRect.anchoredPosition = Vector2.zero;
            textArea.AddComponent<RectMask2D>();

            // Placeholder Text
            TextMeshProUGUI placeholder = CreateText(textArea.transform, "Placeholder", placeholderText, Vector2.zero, size, 16, TextAlignmentOptions.Left, new Color(0.6f, 0.7f, 0.8f, 0.6f));
            SetStretch(placeholder.GetComponent<RectTransform>());

            // Input Text
            TextMeshProUGUI textComp = CreateText(textArea.transform, "Text", "", Vector2.zero, size, 18, TextAlignmentOptions.Left, Color.white);
            SetStretch(textComp.GetComponent<RectTransform>());

            input.textViewport = taRect;
            input.textComponent = textComp;
            input.placeholder = placeholder;
            input.pointSize = 18;

            return input;
        }

        private static GameObject CreateChoiceButtonElement(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            GameObject obj = CreateUIElement(name, parent);
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.12f, 0.18f, 0.28f, 0.95f);
            img.raycastTarget = true;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.18f, 0.35f, 0.65f, 1.0f);
            cb.pressedColor = new Color(0.1f, 0.6f, 0.4f, 1.0f);
            btn.colors = cb;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // Text Label "A.", "B.", "C.", "D.", "E."
            CreateText(obj.transform, "Text_Label", $"{label}.", 
                new Vector2(-345f, 0f), new Vector2(50f, 38f), 19, TextAlignmentOptions.Center, new Color(0.1f, 0.85f, 1.0f));

            // Text Pilihan Jawaban
            TextMeshProUGUI txtPilihan = CreateText(obj.transform, "Text_Pilihan", $"Pilihan Jawaban {label}...", 
                new Vector2(30f, 0f), new Vector2(670f, 40f), 17, TextAlignmentOptions.Left, Color.white);
            txtPilihan.enableWordWrapping = true;

            return obj;
        }
        #endregion
    }
}
