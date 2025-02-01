using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_1._0.Entities
{
    public class QuizSet
    {
        private List<Quiz> quizzes = new();
        public QuizSet() { }
        public QuizSet(List<Quiz> quizzes) { this.quizzes = quizzes; }
        public List<Quiz>? Quizzes { get { return quizzes; } set { quizzes = value; } }
    }
}
