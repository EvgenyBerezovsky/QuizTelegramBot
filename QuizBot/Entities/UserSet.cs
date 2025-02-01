using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot.Entities
{
     public class UserSet
    {
        private List<User> users = new();
        public UserSet() { }
        public UserSet(List<User> users) { this.users = users; }
        public List<User>? Users { get { return users; } set { users = value; } }
        public void AddNewUser(User user) { users.Add(user); }
    }
}
