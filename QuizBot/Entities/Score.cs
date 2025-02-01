using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot.Entities
{
    public class Score
    {
        public DateTime Time { get; set; }
        public string Topic { get; set; }
        public float Result { get; set; }
    }
}
