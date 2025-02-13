using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Infrastructure.DbDataService.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
        public ICollection<QuestionItem> Questions { get; set; }
    }
}
