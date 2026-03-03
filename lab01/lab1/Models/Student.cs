// === Student.cs ===
using System.Collections.Generic;

namespace ExamResultBuilder.Models
{
    public class Student
    {
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string Group { get; set; } = "";
        public List<ExamSession> Sessions { get; set; } = new List<ExamSession>();

        public Student() { }

        public Student(string lastName, string firstName, string group)
        {
            LastName = lastName;
            FirstName = firstName;
            Group = group;
        }

        // Для удобства отображения в списках
        public string FullName => $"{LastName} {FirstName} ({Group})";
    }
}