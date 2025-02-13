

namespace QuizBot_3._0.Entities
{
    public class User
    {
        public User()
        {
        }
        public User(long chatId, string userName)
        {
            ChatId = chatId;
            UserName = userName;
            Scores = new List<Score>();
        }
        public long ChatId { get; set; }
        public string UserName { get; set; }
        public List<Score>Scores { get; set; }
        public void AddScore(DateTime time, string topic, float result)
        {
            var score = new Score() { Time = time, Topic = topic, Result = result };
            Scores.Add(score);
        }
    }
}
