using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_1._0.Entities
{
    public class QuizState
    {
        public string Title { get; set; }
        public List<QuestionItem> Questions { get; set; } = new List<QuestionItem>();
        public QuizStep CurrentStep { get; set; }
    }
}
