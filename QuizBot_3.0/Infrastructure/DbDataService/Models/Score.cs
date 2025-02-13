using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Infrastructure.DbDataService.Models
{
    public class Score
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string Topic { get; set; }
        public float Result { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
