using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot.Entities
{
    public class Progress
    {
        public Progress(string userName)
        {
            UserName = userName;
            Scores = new List<Score>();
        }
        public string UserName { get; set; }
        public List<Score> Scores { get; set; }
        public void AddScore(DateTime time, string topic, float result) 
        {
            var score = new Score() { Time = time, Topic = topic, Result = result };
            Scores.Add(score);
        }

    }
}
