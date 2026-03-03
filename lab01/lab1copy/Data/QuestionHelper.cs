using System;
using System.Collections.Generic;
using System.Linq;
using ExamResultBuilder.Models;
using ExamResultBuilder.Data;

public static class QuestionHelper
{
    private static Random _rnd = new Random();

    public static List<Question> GetRandomQuestions(IEnumerable<int> topicNumbers, int count)
    {
        var all = QuestionStorage.LoadQuestions();
        var available = all.Where(q => topicNumbers.Contains(q.TopicNumber)).ToList();
        if (!available.Any() || count <= 0) return new List<Question>();
        return available.OrderBy(x => _rnd.Next()).Take(count).ToList();
    }
}