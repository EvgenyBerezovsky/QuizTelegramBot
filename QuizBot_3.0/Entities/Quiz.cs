

namespace QuizBot_3._0.Entities
{
    public class Quiz
    {
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
        public string? Topic { get; set; }
        public List<QuestionItem>? Questions { get; set; }
        public Quiz() { }
        public Quiz(string topic, List<QuestionItem> questions)
        {
            IsActive = true;
            IsPublished = true;
            Topic = topic;
            Questions = questions;
        }
        public override string ToString()
        {
            return this.Topic.ToString();
        }
    }
}
