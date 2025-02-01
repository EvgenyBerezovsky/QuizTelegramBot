using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_1._0.Entities
{
    public enum QuizStep
    {
        EnterTitle,
        EnterQuestion,
        EnterOption1,
        EnterOption2,
        EnterOption3,
        EnterOption4,
        EnterCorrectOption,
        EnterQuestionOrFinish
    }
}
