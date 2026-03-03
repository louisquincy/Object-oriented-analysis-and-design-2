// === IExamResultBuilder.cs ===
using System.Collections.Generic;
using ExamResultBuilder.Models;
using ExamResultBuilder.Results;

namespace ExamResultBuilder.Builders
{
    public interface IExamResultBuilder
    {
        IExamResultBuilder ProcessTopic(int topicNumber, IEnumerable<LabWork> labs);
        ExamResult Build();
    }
}