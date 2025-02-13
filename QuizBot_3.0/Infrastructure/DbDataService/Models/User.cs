using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Infrastructure.DbDataService.Models
{
    public class User
    {
        public int Id { get; set; } 
        public long ChatID { get; set; }
        public string UserName { get; set; }
        public IEnumerable<Score> Scores { get; set; }
    }
}
