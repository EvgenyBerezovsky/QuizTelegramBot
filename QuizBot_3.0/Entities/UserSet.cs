

namespace QuizBot_3._0.Entities
{
     public class UserSet
    {
        private List<User> users = new();
        public UserSet() { }
        public UserSet(List<User> users) { this.users = users; }
        public List<User>? Users { get { return users; } set { users = value; } }
    }
}
