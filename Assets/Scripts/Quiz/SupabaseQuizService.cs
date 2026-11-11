using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FOPC.Quiz
{
    /// <summary>
    /// Service penghubung ke Supabase PostgREST API untuk operasi CRUD data Kuis.
    /// </summary>
    public class SupabaseQuizService : MonoBehaviour
    {
        private static SupabaseQuizService _instance;
        public static SupabaseQuizService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SupabaseQuizService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SupabaseQuizService");
                        _instance = go.AddComponent<SupabaseQuizService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Konfigurasi Supabase Project")]
        [Tooltip("URL Supabase Project")]
        public string supabaseUrl = "https://wvfhihmfotlvxmurjuch.supabase.co";

        [Tooltip("Anon / Publishable API Key")]
        [TextArea(2, 4)]
        public string supabaseAnonKey = "sb_publishable_eLxl6uq7QBKlXrEYQ2ljhQ_jItVSr2v";

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private string BaseRestUrl => supabaseUrl.TrimEnd('/') + "/rest/v1";

        #region Helper Web Request
        private UnityWebRequest CreateRequest(string endpoint, string method, string jsonBody = null)
        {
            string url = BaseRestUrl + endpoint;
            UnityWebRequest request = new UnityWebRequest(url, method);

            if (!string.IsNullOrEmpty(jsonBody))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", "Bearer " + supabaseAnonKey);
            request.SetRequestHeader("Content-Type", "application/json");

            if (method == "POST")
            {
                // Supabase: minta data yang baru dibuat langsung dikembalikan dalam response
                request.SetRequestHeader("Prefer", "return=representation");
            }

            return request;
        }
        #endregion

        #region 1. Ambil Data Pertemuan (Meeting)
        public IEnumerator GetMeetingByNumber(int meetingNumber, Action<Meeting> onSuccess, Action<string> onError)
        {
            string endpoint = $"/meetings?meeting_number=eq.{meetingNumber}&select=*";
            using (UnityWebRequest request = CreateRequest(endpoint, "GET"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    List<Meeting> meetings = JsonHelper.FromJsonList<Meeting>(request.downloadHandler.text);
                    if (meetings.Count > 0)
                    {
                        onSuccess?.Invoke(meetings[0]);
                    }
                    else
                    {
                        onError?.Invoke($"Pertemuan ke-{meetingNumber} tidak ditemukan di database Supabase.");
                    }
                }
                else
                {
                    onError?.Invoke($"Gagal mengambil meeting: {request.error} | {request.downloadHandler.text}");
                }
            }
        }
        #endregion

        #region 2. Ambil Soal & Pilihan Jawaban Lengkap
        public IEnumerator GetQuestionsWithChoices(long meetingId, Action<List<QuestionItem>> onSuccess, Action<string> onError)
        {
            // 1. Ambil daftar soal
            string qEndpoint = $"/questions?meeting_id=eq.{meetingId}&select=*&order=question_order.asc";
            List<Question> questions = new List<Question>();

            using (UnityWebRequest qReq = CreateRequest(qEndpoint, "GET"))
            {
                yield return qReq.SendWebRequest();

                if (qReq.result == UnityWebRequest.Result.Success)
                {
                    questions = JsonHelper.FromJsonList<Question>(qReq.downloadHandler.text);
                }
                else
                {
                    onError?.Invoke($"Gagal mengambil soal: {qReq.error} | {qReq.downloadHandler.text}");
                    yield break;
                }
            }

            if (questions.Count == 0)
            {
                onSuccess?.Invoke(new List<QuestionItem>());
                yield break;
            }

            // 2. Ambil pilihan jawaban untuk semua soal tersebut
            List<string> qIdStrings = new List<string>();
            foreach (var q in questions) qIdStrings.Add(q.id.ToString());
            string inQuery = string.Join(",", qIdStrings);

            string cEndpoint = $"/choices?question_id=in.({inQuery})&select=*&order=choice_label.asc";
            List<Choice> choices = new List<Choice>();

            using (UnityWebRequest cReq = CreateRequest(cEndpoint, "GET"))
            {
                yield return cReq.SendWebRequest();

                if (cReq.result == UnityWebRequest.Result.Success)
                {
                    choices = JsonHelper.FromJsonList<Choice>(cReq.downloadHandler.text);
                }
                else
                {
                    onError?.Invoke($"Gagal mengambil pilihan jawaban: {cReq.error} | {cReq.downloadHandler.text}");
                    yield break;
                }
            }

            // 3. Gabungkan soal dan pilihannya
            List<QuestionItem> items = new List<QuestionItem>();
            foreach (var q in questions)
            {
                QuestionItem item = new QuestionItem { question = q };
                foreach (var c in choices)
                {
                    if (c.question_id == q.id)
                    {
                        item.choices.Add(c);
                    }
                }
                items.Add(item);
            }

            onSuccess?.Invoke(items);
        }
        #endregion

        #region 3. Cari / Daftarkan Siswa (Student)
        public IEnumerator GetOrCreateStudent(string name, string className, Action<Student> onSuccess, Action<string> onError)
        {
            // Cek apakah siswa dengan nama dan kelas yang sama sudah terdaftar
            string checkEndpoint = $"/students?name=eq.{UnityWebRequest.EscapeURL(name)}&class_name=eq.{UnityWebRequest.EscapeURL(className)}&limit=1";
            
            using (UnityWebRequest checkReq = CreateRequest(checkEndpoint, "GET"))
            {
                yield return checkReq.SendWebRequest();

                if (checkReq.result == UnityWebRequest.Result.Success)
                {
                    List<Student> existing = JsonHelper.FromJsonList<Student>(checkReq.downloadHandler.text);
                    if (existing.Count > 0)
                    {
                        onSuccess?.Invoke(existing[0]);
                        yield break;
                    }
                }
            }

            // Jika belum ada, buat data siswa baru
            StudentCreatePayload payload = new StudentCreatePayload
            {
                name = name,
                class_name = className
            };
            string jsonBody = JsonUtility.ToJson(payload);

            using (UnityWebRequest createReq = CreateRequest("/students", "POST", jsonBody))
            {
                yield return createReq.SendWebRequest();

                if (createReq.result == UnityWebRequest.Result.Success)
                {
                    List<Student> created = JsonHelper.FromJsonList<Student>(createReq.downloadHandler.text);
                    if (created.Count > 0)
                    {
                        onSuccess?.Invoke(created[0]);
                    }
                    else
                    {
                        onError?.Invoke("Berhasil membuat siswa, namun data response kosong.");
                    }
                }
                else
                {
                    onError?.Invoke($"Gagal membuat data siswa: {createReq.error} | {createReq.downloadHandler.text}");
                }
            }
        }
        #endregion

        #region 4. Simpan Riwayat Percobaan Kuis (Quiz Attempt)
        public IEnumerator SubmitQuizAttempt(long studentId, long meetingId, float score, Action<QuizAttempt> onSuccess, Action<string> onError)
        {
            QuizAttemptCreatePayload payload = new QuizAttemptCreatePayload
            {
                student_id = studentId,
                meeting_id = meetingId,
                score = score
            };
            string jsonBody = JsonUtility.ToJson(payload);

            using (UnityWebRequest req = CreateRequest("/quiz_attempts", "POST", jsonBody))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    List<QuizAttempt> attempts = JsonHelper.FromJsonList<QuizAttempt>(req.downloadHandler.text);
                    if (attempts.Count > 0)
                    {
                        onSuccess?.Invoke(attempts[0]);
                    }
                    else
                    {
                        onError?.Invoke("Berhasil menyimpan attempt, namun data response kosong.");
                    }
                }
                else
                {
                    onError?.Invoke($"Gagal menyimpan riwayat kuis: {req.error} | {req.downloadHandler.text}");
                }
            }
        }
        #endregion

        #region 5. Simpan Rincian Jawaban Siswa (Answers)
        public IEnumerator SubmitAnswers(List<AnswerCreatePayload> answers, Action onSuccess, Action<string> onError)
        {
            if (answers == null || answers.Count == 0)
            {
                onSuccess?.Invoke();
                yield break;
            }

            string jsonArray = JsonHelper.ToJsonArray(answers.ToArray());

            using (UnityWebRequest req = CreateRequest("/answers", "POST", jsonArray))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke();
                }
                else
                {
                    onError?.Invoke($"Gagal menyimpan detail jawaban: {req.error} | {req.downloadHandler.text}");
                }
            }
        }
        #endregion
    }
}
