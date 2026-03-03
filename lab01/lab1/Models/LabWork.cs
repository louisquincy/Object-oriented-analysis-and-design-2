// === LabWork.cs ===
namespace ExamResultBuilder.Models
{
    public class LabWork
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int TopicNumber { get; set; }
        public bool IsOverdue { get; set; }

        public LabWork(int id, string title, int topicNumber, bool isOverdue)
        {
            Id = id;
            Title = title;
            TopicNumber = topicNumber;
            IsOverdue = isOverdue;
        }

        // Для BindingList нужен пустой конструктор
        public LabWork() { }
    }
}
