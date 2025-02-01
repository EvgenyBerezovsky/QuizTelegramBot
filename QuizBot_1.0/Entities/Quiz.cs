using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_1._0.Entities
{
    public class Quiz
    {
        public string? Topic { get; set; }
        public List<QuestionItem>? Questions { get; set; }
        public Quiz() { }
        public Quiz(string topic, List<QuestionItem> questions)
        {
            Topic = topic;
            Questions = questions;
        }
    }
}
