using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_1._0.Entities
{
    public enum ChatCurrentState
    {
        StartState,
        QuizPassingState,
        NewQuizCreationState,
        ResultsProcessState,
        QuizDeletionState
    }
}
