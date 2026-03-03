using System.Collections.Generic;

namespace ExamResultBuilder.Results
{
    public class ExamResult
    {
        public bool GoesToExam { get; }
        public IReadOnlyList<int> TopicsForExam { get; }
        public int QuestionCount { get; }

        public ExamResult(bool goesToExam, List<int> topicsForExam, int questionCount)
        {
            GoesToExam = goesToExam;
            TopicsForExam = topicsForExam?.AsReadOnly() ?? new List<int>().AsReadOnly();
            QuestionCount = questionCount;
        }
    }
}