using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ExamResultBuilder.Models;

namespace ExamResultBuilder.Data
{
    public static class QuestionStorage
    {
        private static readonly string FilePath = "questions.json";

        public static List<Question> LoadQuestions()
        {
            if (!File.Exists(FilePath))
                return GetDefaultQuestions(); // если файла нет, создаём стандартные
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Question>>(json) ?? new List<Question>();
        }

        public static void SaveQuestions(List<Question> questions)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(questions, options);
            File.WriteAllText(FilePath, json);
        }

        private static List<Question> GetDefaultQuestions()
        {
            return new List<Question>
            {
            new Question(1, "Что такое имитационное моделирование?", 1),
            new Question(2, "Назовите основные этапы имитационного моделирования.", 1),
            new Question(3, "В чем отличие дискретно-событийного моделирования от системной динамики?", 1),
            new Question(4, "Для чего нужна калибровка модели?", 1),
            new Question(5, "Какие программные продукты используются для имитационного моделирования?", 1),

            // Тема 2
            new Question(6, "Что такое генератор случайных чисел и зачем он нужен в моделировании?", 2),
            new Question(7, "Объясните понятие «время моделирования».", 2),
            new Question(8, "Какие существуют способы сбора статистики?", 2),
            new Question(9, "Что такое переходный период и как его исключить?", 2),
            new Question(10, "Как проверить адекватность модели?", 2),

            // Тема 3
            new Question(11, "Какие существуют методы оптимизации в имитационном моделировании?", 3),
            new Question(12, "Что такое планирование эксперимента?", 3),
            new Question(13, "Назовите основные критерии оценки эффективности системы.", 3),
            new Question(14, "Как учитывать неопределённость исходных данных?", 3),
            new Question(15, "Приведите примеры успешного применения имитационного моделирования в реальных задачах.", 3),
            };
        }
    }
}