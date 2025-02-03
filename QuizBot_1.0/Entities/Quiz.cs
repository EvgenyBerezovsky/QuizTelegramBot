

namespace QuizBot_1._0.Entities
{
    public class Quiz
    {
        public bool IsActive;
        public string? Topic { get; set; }
        public List<QuestionItem>? Questions { get; set; }
        public Quiz() { }
        public Quiz(string topic, List<QuestionItem> questions)
        {
            IsActive = true;
            Topic = topic;
            Questions = questions;
        }
    }
}
