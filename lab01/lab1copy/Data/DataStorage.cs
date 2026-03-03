using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ExamResultBuilder.Models;

namespace ExamResultBuilder.Data
{
    public static class DataStorage
    {
        private static readonly string FilePath = "students.json";

        public static List<Student> LoadStudents()
        {
            if (!File.Exists(FilePath))
                return new List<Student>();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Student>>(json) ?? new List<Student>();
        }

        public static void SaveStudents(List<Student> students)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(students, options);
            File.WriteAllText(FilePath, json);
        }
    }
}