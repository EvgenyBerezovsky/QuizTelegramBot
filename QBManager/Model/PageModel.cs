using QuizBot_3._0.Entities;
using QuizBot_3._0.Infrastructure.DbDataService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QBManager.Model
{
    public class PageModel
    {
        public PageModel()
        {
            dataService = new DbDataService();
        }
        public DbDataService dataService { get; set; }
        public Quiz Quiz { get; set; } = new Quiz();
        public ObservableCollection<QuestionItem> Questions { get; set; } = new ObservableCollection<QuestionItem>();

        public QuestionItem Question { get; set; } = new QuestionItem();


        public int CustomerCount { get; set; }
        public string ProductStatus { get; set; }=string.Empty;
        public DateOnly OrderDate { get; set; }
        public decimal TransactionValue { get; set; }
        public TimeOnly ShipmentDelivery { get; set; }
        public bool LocationStatus { get; set; }

    }
}
