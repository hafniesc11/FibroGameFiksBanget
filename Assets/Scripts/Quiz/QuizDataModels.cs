using System;
using System.Collections.Generic;
using UnityEngine;

namespace FOPC.Quiz
{
    // ==========================================
    // 1. STUDENTS
    // ==========================================
    [Serializable]
    public class Student
    {
        public long id;
        public string name;
        public string class_name;
        public string created_at;
    }

    [Serializable]
    public class StudentCreatePayload
    {
        public string name;
        public string class_name;
    }

    // ==========================================
    // 2. MEETINGS
    // ==========================================
    [Serializable]
    public class Meeting
    {
        public long id;
        public int meeting_number;
        public string title;
        public string created_at;
    }

    // ==========================================
    // 3. QUESTIONS
    // ==========================================
    [Serializable]
    public class Question
    {
        public long id;
        public long meeting_id;
        public string question_text;
        public int question_order;
    }

    // ==========================================
    // 4. CHOICES
    // ==========================================
    [Serializable]
    public class Choice
    {
        public long id;
        public long question_id;
        public string choice_label; // 'A', 'B', 'C', 'D'
        public string choice_text;
        public bool is_correct;
    }

    // ==========================================
    // 5. QUIZ ATTEMPTS
    // ==========================================
    [Serializable]
    public class QuizAttempt
    {
        public long id;
        public long student_id;
        public long meeting_id;
        public float score;
        public string submitted_at;
    }

    [Serializable]
    public class QuizAttemptCreatePayload
    {
        public long student_id;
        public long meeting_id;
        public float score;
    }

    // ==========================================
    // 6. ANSWERS
    // ==========================================
    [Serializable]
    public class Answer
    {
        public long id;
        public long attempt_id;
        public long question_id;
        public long choice_id;
    }

    [Serializable]
    public class AnswerCreatePayload
    {
        public long attempt_id;
        public long question_id;
        public long choice_id;
    }

    // ==========================================
    // STRUKTUR DATA GABUNGAN SOAL & PILIHAN JAWABAN (IN-GAME)
    // ==========================================
    [Serializable]
    public class QuestionItem
    {
        public Question question;
        public List<Choice> choices = new List<Choice>();
    }
}
