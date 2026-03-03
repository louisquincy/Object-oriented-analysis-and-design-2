namespace ExamResultBuilder.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public int TopicNumber { get; set; } // к какой теме относится

        public Question() { }

        public Question(int id, string text, int topicNumber)
        {
            Id = id;
            Text = text;
            TopicNumber = topicNumber;
        }
    }
}