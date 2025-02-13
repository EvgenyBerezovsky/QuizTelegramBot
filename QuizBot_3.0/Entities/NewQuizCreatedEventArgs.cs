using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Entities
{
    public class NewQuizCreatedEventArgs : EventArgs
    {
        public string NotificationMessage { get; set; }
        public IEnumerable<long> ChatIdCollection { get; set; }
        public NewQuizCreatedEventArgs(IEnumerable<long> chatIdCollection, string notificationMessage)
        {
            ChatIdCollection = chatIdCollection;
            NotificationMessage = notificationMessage;
        }
    }
}
