using System;
using System.Collections.Generic;

namespace ExamResultBuilder.Models
{
    public class ExamSession
    {
        public DateTime Date { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
        public string TeacherComment { get; set; } = "";

        public ExamSession() { }

        public ExamSession(DateTime date, List<Question> questions, string comment)
        {
            Date = date;
            Questions = questions;
            TeacherComment = comment;
        }
    }
}