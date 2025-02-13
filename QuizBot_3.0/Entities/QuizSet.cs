

namespace QuizBot_3._0.Entities
{
    public class QuizSet
    {
        private List<Quiz> quizzes = new();
        public QuizSet() { }
        public QuizSet(List<Quiz> quizzes) { this.quizzes = quizzes; }
        public List<Quiz>? Quizzes { get { return quizzes; } set { quizzes = value; } }
    }
}
