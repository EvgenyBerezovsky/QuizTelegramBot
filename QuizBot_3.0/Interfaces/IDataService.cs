using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Interfaces
{
    public interface IDataService<Q,U>
    {
        public List<Q> Quizzes { get; set; }
        public List<U> Users { get; set; }
        public void SaveNewQuiz(Q quiz);
        public void RemoveQuiz(Q quize);
        public void SaveAllQuizzes();
        public void AddNewUserOrUpdate(U newUser);
        public void CleanUsersData();
    }
}
