// === ExamResultBuilder.cs ===
using System.Collections.Generic;
using System.Linq;
using ExamResultBuilder.Models;
using ExamResultBuilder.Results;

namespace ExamResultBuilder.Builders
{
    public class ExamResultBuilder : IExamResultBuilder
    {
        private int _overdueCount = 0;
        private HashSet<int> _topicsForExam = new HashSet<int>();

        public IExamResultBuilder ProcessTopic(int topicNumber, IEnumerable<LabWork> labs)
        {
            if (labs == null) return this;
            var overdueInTopic = labs.Where(l => l.TopicNumber == topicNumber && l.IsOverdue).ToList();
            if (overdueInTopic.Any())
            {
                _topicsForExam.Add(topicNumber);
                _overdueCount += overdueInTopic.Count;
            }
            return this;
        }

        public ExamResult Build()
        {
            bool goesToExam = _overdueCount > 0;
            return new ExamResult(goesToExam, _topicsForExam.ToList(), _overdueCount);
        }
    }
}