

namespace QuizBot_1._0.Entities
{
    public class QuestionItem
    {
        public string? Answer { get; set; }
        public int CorrectOptionIndex { get; set; }
        public string? Question { get; set; }
        public string[] Options { get; set; } = new string[4];

        public QuestionItem()
        {
        }
        public QuestionItem(string question, string answer, string[] options, int correctOption)
        {
            Answer = answer;
            Options = options;
            Question = question;
            CorrectOptionIndex = correctOption;
        }
    }
}
