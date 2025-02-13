using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Infrastructure.DbDataService.Models
{
    public class QuestionItem
    {
        public int Id { get; set; }
        public string Question { get; set; }    
        public string Answer { get; set; }
        public int CorrectOptionIndex { get; set; }
        public int OptionsId { get; set; }
        public Options Options { get; set; }
        public int QuizId { get; set; } 
        public Quiz Quiz { get; set; }
    }
}

