using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FOPC.Quiz
{
    /// <summary>
    /// Manager Kuis Utama yang mengatur alur kuis dinamis dengan integrasi Supabase.
    /// Dilengkapi feedback visual warna Hijau (Benar) dan Merah (Salah).
    /// </summary>
    public class DynamicQuizManager : MonoBehaviour
    {
        public static DynamicQuizManager Instance { get; private set; }

        [Header("Pengaturan Pertemuan Kuis")]
        [Tooltip("Nomor pertemuan materi kuis (1 = Pertemuan 1, 2 = Pertemuan 2, dst)")]
        public int currentMeetingNumber = 1;

        [Header("Referensi UI Kuis")]
        public DynamicQuizUI quizUI;

        // Data Runtime Kuis
        private Meeting currentMeeting;
        private List<QuestionItem> loadedQuestions = new List<QuestionItem>();
        private int currentQuestionIndex = 0;
        private int correctCount = 0;
        private string studentName = "";
        private string studentClass = "";
        private long studentId = 0;
        private Dictionary<long, Choice> studentSelectedChoices = new Dictionary<long, Choice>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (quizUI == null)
            {
                quizUI = FindFirstObjectByType<DynamicQuizUI>();
            }

            if (quizUI != null)
            {
                quizUI.ShowStudentInputPanel();
            }
        }

        /// <summary>
        /// Memulai proses kuis setelah siswa mengisi Nama & Kelas.
        /// </summary>
        public void StartQuiz(string name, string className)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (quizUI != null) quizUI.ShowErrorMessage("Silakan isi nama Anda terlebih dahulu.");
                return;
            }

            if (string.IsNullOrEmpty(className))
            {
                if (quizUI != null) quizUI.ShowErrorMessage("Silakan isi kelas Anda terlebih dahulu.");
                return;
            }

            studentName = name;
            studentClass = className;
            currentQuestionIndex = 0;
            correctCount = 0;
            studentSelectedChoices.Clear();

            if (quizUI != null) quizUI.ShowLoading("Memuat Soal...");

            StartCoroutine(LoadQuizRoutine(studentName, studentClass));
        }

        private IEnumerator LoadQuizRoutine(string name, string className)
        {
            if (SupabaseQuizService.Instance == null)
            {
                if (quizUI != null) quizUI.ShowErrorMessage("SupabaseQuizService belum aktif di scene.");
                yield break;
            }

            // 1. Dapatkan atau Buat Siswa di Supabase
            bool studentDone = false;
            string studentError = null;

            StartCoroutine(SupabaseQuizService.Instance.GetOrCreateStudent(
                name,
                className,
                (student) =>
                {
                    studentId = student.id;
                    studentDone = true;
                },
                (error) =>
                {
                    studentError = error;
                    studentDone = true;
                }
            ));

            yield return new WaitUntil(() => studentDone);

            if (!string.IsNullOrEmpty(studentError))
            {
                if (quizUI != null) quizUI.ShowErrorMessage(studentError);
                yield break;
            }

            // 2. Ambil Data Pertemuan dari Supabase
            bool meetingDone = false;
            string meetingError = null;

            StartCoroutine(SupabaseQuizService.Instance.GetMeetingByNumber(
                currentMeetingNumber,
                (meeting) =>
                {
                    currentMeeting = meeting;
                    meetingDone = true;
                },
                (error) =>
                {
                    meetingError = error;
                    meetingDone = true;
                }
            ));

            yield return new WaitUntil(() => meetingDone);

            if (!string.IsNullOrEmpty(meetingError))
            {
                if (quizUI != null) quizUI.ShowErrorMessage(meetingError);
                yield break;
            }

            // 3. Ambil Soal dan Pilihan Jawaban
            bool questionsDone = false;
            string questionsError = null;

            StartCoroutine(SupabaseQuizService.Instance.GetQuestionsWithChoices(
                currentMeeting.id,
                (questions) =>
                {
                    if (questions != null && questions.Count > 0)
                    {
                        loadedQuestions = questions;
                    }
                    else
                    {
                        questionsError = $"Belum ada soal untuk {currentMeeting.title}.";
                    }
                    questionsDone = true;
                },
                (error) =>
                {
                    questionsError = error;
                    questionsDone = true;
                }
            ));

            yield return new WaitUntil(() => questionsDone);

            if (!string.IsNullOrEmpty(questionsError))
            {
                if (quizUI != null) quizUI.ShowErrorMessage(questionsError);
                yield break;
            }

            // 4. Tampilkan Soal Nomor 1
            currentQuestionIndex = 0;
            DisplayCurrentQuestion();
        }

        private void DisplayCurrentQuestion()
        {
            if (loadedQuestions == null || loadedQuestions.Count == 0) return;

            if (currentQuestionIndex < loadedQuestions.Count)
            {
                QuestionItem item = loadedQuestions[currentQuestionIndex];

                if (quizUI != null)
                {
                    quizUI.DisplayQuestion(
                        currentMeeting.title,
                        currentQuestionIndex + 1,
                        loadedQuestions.Count,
                        item.question,
                        item.choices
                    );
                }
            }
        }

        // Flag proteksi debounce agar 1 klik hanya memproses 1 soal
        private bool isProcessingAnswer = false;

        /// <summary>
        /// Dipanggil saat siswa memilih salah satu pilihan jawaban (A, B, C, D, E).
        /// </summary>
        public void OnChoiceSelected(Choice choice)
        {
            if (isProcessingAnswer) return;
            if (loadedQuestions == null || currentQuestionIndex >= loadedQuestions.Count) return;

            isProcessingAnswer = true;
            StartCoroutine(AdvanceToNextQuestionRoutine(choice));
        }

        private IEnumerator AdvanceToNextQuestionRoutine(Choice choice)
        {
            QuestionItem currentItem = loadedQuestions[currentQuestionIndex];
            studentSelectedChoices[currentItem.question.id] = choice;

            if (choice.is_correct)
            {
                correctCount++;
            }

            // 1. Tampilkan feedback visual: Hijau (Benar) atau Merah (Salah)
            if (quizUI != null)
            {
                quizUI.ShowAnswerFeedback(choice, currentItem.choices);
            }

            // 2. Berikan jeda 1.2 detik agar siswa melihat hasil jawabannya
            yield return new WaitForSeconds(1.2f);

            // 3. Lanjut ke soal berikutnya
            currentQuestionIndex++;

            if (currentQuestionIndex < loadedQuestions.Count)
            {
                DisplayCurrentQuestion();
            }
            else
            {
                FinishQuiz();
            }

            isProcessingAnswer = false;
        }

        /// <summary>
        /// Menyimpan hasil kuis ke Supabase dan menampilkan skor akhir.
        /// </summary>
        private void FinishQuiz()
        {
            if (quizUI != null) quizUI.ShowLoading("Menyimpan Jawaban...");

            float finalScore = ((float)correctCount / loadedQuestions.Count) * 100f;
            finalScore = Mathf.Round(finalScore * 100f) / 100f;

            StartCoroutine(SubmitQuizRoutine(finalScore));
        }

        private IEnumerator SubmitQuizRoutine(float finalScore)
        {
            if (SupabaseQuizService.Instance == null)
            {
                ShowFinalResultUI(finalScore);
                yield break;
            }

            // 1. Simpan Quiz Attempt
            bool attemptDone = false;
            QuizAttempt createdAttempt = null;

            StartCoroutine(SupabaseQuizService.Instance.SubmitQuizAttempt(
                studentId,
                currentMeeting.id,
                finalScore,
                (attempt) =>
                {
                    createdAttempt = attempt;
                    attemptDone = true;
                },
                (error) =>
                {
                    Debug.LogWarning($"[DynamicQuizManager] {error}");
                    attemptDone = true;
                }
            ));

            yield return new WaitUntil(() => attemptDone);

            // 2. Simpan Detail Jawaban Siswa
            if (createdAttempt != null)
            {
                List<AnswerCreatePayload> answersPayload = new List<AnswerCreatePayload>();

                foreach (var kvp in studentSelectedChoices)
                {
                    long qId = kvp.Key;
                    Choice chosen = kvp.Value;

                    answersPayload.Add(new AnswerCreatePayload
                    {
                        attempt_id = createdAttempt.id,
                        question_id = qId,
                        choice_id = chosen.id
                    });
                }

                bool answersDone = false;
                StartCoroutine(SupabaseQuizService.Instance.SubmitAnswers(
                    answersPayload,
                    () => answersDone = true,
                    (error) =>
                    {
                        Debug.LogWarning($"[DynamicQuizManager] {error}");
                        answersDone = true;
                    }
                ));

                yield return new WaitUntil(() => answersDone);
            }

            // 3. Tampilkan Skor Akhir
            ShowFinalResultUI(finalScore);
        }

        private void ShowFinalResultUI(float finalScore)
        {
            if (quizUI != null)
            {
                quizUI.ShowResultPanel(
                    studentName,
                    studentClass,
                    finalScore,
                    correctCount,
                    loadedQuestions.Count
                );
            }
        }

        public void RestartQuiz()
        {
            currentQuestionIndex = 0;
            correctCount = 0;
            studentSelectedChoices.Clear();

            if (quizUI != null)
            {
                quizUI.ShowStudentInputPanel();
            }
        }
    }
}
