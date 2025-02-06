using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using QuizBot_1._0.Entities;
using QuizBot_1._0.Infrastructure;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using User = QuizBot_1._0.Entities.User;

namespace QuizBot_1._0.BusinessLogic
{
    public class BotUpdateHandler
    {
        public event Func<object, NewQuizCreatedEventArgs, Task> NewQuizCreated;

        Dictionary<long, ChatCurrentState> userChatCurrentState = new(); // ChatId -> текущее состояние чата
        Dictionary<long, QuizState> userCreateQuizState = new();         // ChatId -> текущая создаваемая викторина

        Dictionary<long, Quiz> userQuizState = new();                    // ChatId -> текущая викторина
        Dictionary<long, User> userProgressState = new();                // ChatId -> Progress текущего User

        Dictionary<long, int> userCorrectAnswers = new();                // ChatId -> правильные ответы
        Dictionary<long, int> userQuizQuestionState = new();             // ChatId -> текущий вопрос

        DataService _dataService;

        public BotUpdateHandler()
        {
            _dataService = new DataService();
        }
        public string HandleMessage(Update update, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = "Невірний ввод.";

            var message = update.Message;
            var chatId = message.Chat.Id;

            switch (message.Text)
            {
                case "/start":
                    response = GiveResponseToStart(message, chatId, out menu);
                    break;
                case "/info":
                    response = GiveResponseToInfo(update, out menu);
                    break;
                case "/create_new":
                    response = GiveResponseToCreateNew(update, out menu);
                    break;
                default:
                    response = ProcessQuizStep(update, out menu);
                    break;
            }
            return response;
        }
        public string HandleCallbackQuery(Update update, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = string.Empty;

            var callbackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var message = callbackData.ToLower();

            switch (message)
            {
                case var mes when mes.StartsWith("startquiz"):
                    response = GiveResponseToStartQuizCallback(message, chatId, out menu);
                    break;
                case var mes when mes.StartsWith("answer"):
                    response = GiveResponseToAnswerCallback(message, chatId, out menu);
                    break;
                case var mes when mes == "nextquestion":
                    response = GiveResponseToNextQuestionCallback(chatId);
                    break;
                case var mes when mes == "finishquiz":
                    response = GiveResponseToFinishQuizCallback(chatId);
                    break;
                case var mes when mes.ToLower() == "showusersinfo":
                    response = GiveResponseToShowUserInfoCallback(chatId);
                    break;
                case var mes when mes.ToLower() == "cleanusersinfo":
                    response = GiveResponseToCleanUserInfoCallback(chatId);
                    break;
                case var mes when mes.ToLower() == "deletequiz":
                    response = GiveResponseToDeleteQuizCallback(chatId, out menu);
                    break;
                case var mes when mes.StartsWith("deletequiznumber"):
                    response = GiveResponseToDeleteQuizNumberCallback(chatId, message, out menu);
                    break;
                case var mes when mes.ToLower() == "yes":
                    response = GiveResponseToDeleteQuizNumberYesCallback(chatId);
                    break;
                case var mes when mes.ToLower() == "no":
                    response = GiveResponseToDeleteQuizNumberNoCallback(chatId);
                    break;

                default:
                    response = "No recognized callback message found.";
                    break;
            }
            return response;

        }

        #region Created Menu
        private InlineKeyboardMarkup GetInfoMenu()
        {

            var inlineKeyboard = new InlineKeyboardMarkup();
            var kb = new InlineKeyboardButton[3][];


            kb[0] = new InlineKeyboardButton[1];
            kb[1] = new InlineKeyboardButton[1];
            kb[2] = new InlineKeyboardButton[1];
            kb[0][0] = new InlineKeyboardButton { Text = "Перегляд результатів користувачів", CallbackData = "ShowUsersInfo" };
            kb[1][0] = new InlineKeyboardButton { Text = "Видалення результатів користувачів", CallbackData = "CleanUsersInfo" };
            kb[2][0] = new InlineKeyboardButton { Text = "Видалення вікторини", CallbackData = "DeleteQuiz" };
            inlineKeyboard.InlineKeyboard = kb;
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup GetMainMenu()
        {

            var inlineKeyboard = new InlineKeyboardMarkup();
            var kb = new InlineKeyboardButton[_dataService.Quizzes.Count][];

            for (int i = 0; i < _dataService.Quizzes.Count; i++)
            {
                string callbackData = string.Concat("StartQuiz", i);
                kb[i] = new InlineKeyboardButton[1];
                kb[i][0] = new InlineKeyboardButton { Text = _dataService.Quizzes[i].Topic.ToString(), CallbackData = callbackData };
            }
            inlineKeyboard.InlineKeyboard = kb;
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup GetYesNoMenu()
        {
            var inlineKeyboard = new InlineKeyboardMarkup() { InlineKeyboard = new[] { new[] { new InlineKeyboardButton { Text = "Так", CallbackData = "yes" }, new InlineKeyboardButton { Text = "Ні", CallbackData = "no" } } } };
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup GetDeleteQuizMenu()
        {

            var inlineKeyboard = new InlineKeyboardMarkup();
            var kb = new InlineKeyboardButton[_dataService.Quizzes.Count][];

            for (int i = 0; i < _dataService.Quizzes.Count; i++)
            {
                string text = $"{i + 1} - {_dataService.Quizzes[i].Topic.ToString()}";
                string callbackData = string.Concat("DeleteQuizNumber", i);
                kb[i] = new InlineKeyboardButton[1];
                kb[i][0] = new InlineKeyboardButton { Text = text, CallbackData = callbackData };
            }
            inlineKeyboard.InlineKeyboard = kb;
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup SendNextNewQuestionOrFinishMenu()
        {
            var inlineKeyboard = new InlineKeyboardMarkup();
            var kb = new InlineKeyboardButton[1][];


            kb[0] = new InlineKeyboardButton[2];
            kb[0][0] = new InlineKeyboardButton { Text = "Наступне питання", CallbackData = "NextQuestion" };
            kb[0][1] = new InlineKeyboardButton { Text = "Закінчити вікторину", CallbackData = "FinishQuiz" };

            inlineKeyboard.InlineKeyboard = kb;
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup QuizMenu(QuestionItem questionItem)
        {
            var inlineKeyboard = new InlineKeyboardMarkup();
            var kb = new InlineKeyboardButton[questionItem.Options.Length][];
            string callbackData = "answer";
            for (int i = 0; i < questionItem.Options.Length; i++)
            {
                kb[i] = new InlineKeyboardButton[1];
                kb[i][0] = new InlineKeyboardButton { Text = questionItem.Options[i], CallbackData = callbackData + i.ToString() };
            }
            inlineKeyboard.InlineKeyboard = kb;
            return inlineKeyboard;
        }
        #endregion

        #region GiveResponseToCommand Methods
        private string GiveResponseToInfo(Update update, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = "Невірний ввод.";
            long chatId = update.Message.Chat.Id;

            if (!userChatCurrentState.ContainsKey(chatId)) userChatCurrentState.Add(chatId, ChatCurrentState.StartState);
            if (userChatCurrentState[chatId] == ChatCurrentState.StartState)
            {
                userChatCurrentState[chatId] = ChatCurrentState.ResultsProcessState;
                menu = GetInfoMenu();
                response = $"<b>Виберіть потрібну дію?</b>";
            }
            return response;
        }
        private string GiveResponseToShowUserInfoCallback(long chatId)
        {
            Console.WriteLine(userChatCurrentState[chatId].ToString());
            string response = "Невірний ввод.";

            if (!userChatCurrentState.ContainsKey(chatId)) userChatCurrentState.Add(chatId, ChatCurrentState.StartState);
            if (userChatCurrentState.ContainsKey(chatId) && (userChatCurrentState[chatId] is ChatCurrentState.ResultsProcessState | userChatCurrentState[chatId] is ChatCurrentState.StartState))
            {
                if (_dataService.Users.Count == 0)
                {
                    response = "Нет информации";
                }
                else
                {
                    var sb = new StringBuilder();
                    foreach (var user in _dataService.Users)
                    {
                        sb.AppendLine($"Пользователь {user.UserName}:");
                        foreach (var score in user.Scores)
                        {
                            sb.AppendLine($"Тема: {score.Topic}");
                            sb.AppendLine($"Балл: {score.Result.ToString("F2")}");
                            sb.AppendLine($"Время: {score.Time}");
                            sb.AppendLine(new string('-', 10));
                        }
                    }

                    response = sb.ToString();
                }
                userChatCurrentState[chatId] = ChatCurrentState.StartState;
            }

            return response;
        }
        private string GiveResponseToCreateNew(Update update, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = "Невірний ввод";
            long chatId = update.Message.Chat.Id;

            if (!userChatCurrentState.ContainsKey(chatId)) userChatCurrentState.Add(chatId, ChatCurrentState.StartState);
            if (userChatCurrentState[chatId] == ChatCurrentState.StartState)
            {
                userChatCurrentState[chatId] = ChatCurrentState.NewQuizCreationState;
                userCreateQuizState[chatId] = new QuizState();
                response = StartQuizCreation(chatId);
            }
            return response;
        }
        private string GiveResponseToStart(Message message, long chatId, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = "Невірний ввод";
            if (!userChatCurrentState.ContainsKey(chatId))
            {
                userChatCurrentState.Add(chatId, ChatCurrentState.StartState);
            }
            if (userChatCurrentState[chatId] is ChatCurrentState.StartState)
            {
                userQuizState.Remove(chatId);
                userProgressState.Remove(chatId);
                userCreateQuizState.Remove(chatId);

                if (_dataService.Quizzes == null || _dataService.Quizzes.Count == 0)
                {
                    menu = null;
                    response = "<b>Немає доступних вікторін.</b>";
                    return response;
                }

                else
                {
                    if (!userProgressState.ContainsKey(chatId))
                    {
                        var userName = message.Chat.Username == null ? "Unknown_User" : message.Chat.Username;
                        userProgressState.Add(chatId, new User(chatId, userName));
                    }
                    response = "Виберіть вікторину:";
                    menu = GetMainMenu();
                }
            }
            return response;
        }
        #endregion

        #region GiveResponseToCallback methods
        private string GiveResponseToNextQuestionCallback(long chatId)
        {
            string response = "Невірний ввод";
            if (userCreateQuizState.ContainsKey(chatId))
            {
                var state = userCreateQuizState[chatId];
                if (state.CurrentStep == QuizStep.EnterQuestionOrFinish)
                {
                    state.CurrentStep = QuizStep.EnterQuestion;
                    response = "<b>Будь ласка, введіть наступне питання:</b>";
                }
            }
            return response;
        }
        private string GiveResponseToFinishQuizCallback(long chatId)
        {
            string response = "Невірний ввод.";
            if (userCreateQuizState.ContainsKey(chatId))
            {
                var state = userCreateQuizState[chatId];
                if (state.CurrentStep == QuizStep.EnterQuestionOrFinish)
                {
                    response = $"Вікторина <b> -'{state.Title}'- </b> сворена. \nКількість питань: <b> {state.Questions.Count} </b>.";

                    Quiz quiz = new Quiz();
                    quiz.Topic = state.Title;
                    quiz.Questions = state.Questions;
                    quiz.IsActive = true;
                    _dataService.SaveNewQuiz(quiz);

                    userCreateQuizState.Remove(chatId);
                    userChatCurrentState[chatId] = ChatCurrentState.StartState;


                    string notificationMessage = $"У нас есть новая викторина! \n<b> -{quiz.Topic}- </b> \nПроверьте свои знания.";
                    var chatIdCollection = _dataService.Users.Where(u => u.ChatId != 0).Select(u => u.ChatId).ToList();

                    OnNewQuizCreated(new NewQuizCreatedEventArgs(chatIdCollection, notificationMessage));

                    return response;
                }
            }
            return response;
        }
        private string GiveResponseToCleanUserInfoCallback(long chatId)
        {
            string response = "Невірний ввод.";
            if (!userChatCurrentState.ContainsKey(chatId)) userChatCurrentState.Add(chatId, ChatCurrentState.StartState);
            if (userChatCurrentState.ContainsKey(chatId) && (userChatCurrentState[chatId] is ChatCurrentState.ResultsProcessState || userChatCurrentState[chatId] is ChatCurrentState.StartState))
            {
                _dataService.CleanUsersData();
                userChatCurrentState[chatId] = ChatCurrentState.StartState;
                response = "<b>Дані видалено.</b>";
            }
            return response;
        }
        private string GiveResponseToDeleteQuizNumberNoCallback(long chatId)
        {
            string response = "Невірний ввод.";
            if (userChatCurrentState.ContainsKey(chatId) && userChatCurrentState[chatId] == ChatCurrentState.QuizDeletionState)
            {
                foreach (var quize in _dataService.Quizzes)
                {
                    if (!quize.IsActive) quize.IsActive = true;
                }
                response = "<b>Видалення скасовано.</b>";
            }
            return response;
        }
        private string GiveResponseToDeleteQuizNumberYesCallback(long chatId)
        {
            string response = "Невірний ввод.";
            if (userChatCurrentState.ContainsKey(chatId) && userChatCurrentState[chatId] == ChatCurrentState.QuizDeletionState)
            {
                var quizeToDelete = (from q in _dataService.Quizzes
                                     where q.IsActive == false
                                     select q).FirstOrDefault();
                _dataService.RemoveQuiz(quizeToDelete);
                response = "<b>Дані оновлені</b>";
                userChatCurrentState[chatId] = ChatCurrentState.StartState;
            }

            return response;
        }
        private string GiveResponseToDeleteQuizCallback(long chatId, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = "Невірний ввод.";
            if (userChatCurrentState.ContainsKey(chatId) && (userChatCurrentState[chatId] is ChatCurrentState.ResultsProcessState || userChatCurrentState[chatId] is ChatCurrentState.StartState))
            {
                userChatCurrentState[chatId] = ChatCurrentState.QuizDeletionState;
                menu = GetDeleteQuizMenu();
                response = "Виберіть вікторину для видалення.";
            }
            return response;
        }
        private string GiveResponseToAnswerCallback(string message, long chatId, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = string.Empty;
            string subresponse = string.Empty;

            if (userQuizQuestionState.ContainsKey(chatId) && userChatCurrentState[chatId] == ChatCurrentState.QuizPassingState)
            {
                int questionIndex = userQuizQuestionState[chatId];
                int.TryParse(message.Replace("answer", string.Empty), out int index);

                // Проверяем правильность ответа
                if (userQuizState[chatId].Questions[questionIndex].CorrectOptionIndex == index)
                {
                    userCorrectAnswers[chatId]++;
                    subresponse = "<b>Вірно!</b>";
                }
                else
                {
                    subresponse = $"<b>Невірно!</b> \nВірна відповідь: <b>{userQuizState[chatId].Questions[questionIndex].Answer}</b>";
                }

                // Переход к следующему вопросу
                userQuizQuestionState[chatId]++;
                if (userQuizQuestionState[chatId] < userQuizState[chatId].Questions.Count)
                {
                    response = $"{subresponse}\n{SendNextQuestion(chatId, out menu)}";
                }
                else
                {
                    int correctAnswers = userCorrectAnswers[chatId];
                    float result = (float)correctAnswers / (float)userQuizState[chatId].Questions.Count;
                    userProgressState[chatId].AddScore(DateTime.Now, userQuizState[chatId].Topic, result);


                    _dataService.AddNewUserOrUpdate(userProgressState[chatId]);


                    response = $"Викторина завершена! \nВи вірно відповіли на <b>{correctAnswers}</b> из <b>{userQuizState[chatId].Questions.Count}</b> питань.";

                    // Сбрасываем состояние пользователя
                    userChatCurrentState[chatId] = ChatCurrentState.StartState;
                    userQuizState.Remove(chatId);
                    userCorrectAnswers.Remove(chatId);
                    userQuizQuestionState.Remove(chatId);
                    userProgressState.Remove(chatId);
                }

            }
            else
            {
                response = "Невірний ввод!";
            }
            return response;
        }
        private string GiveResponseToStartQuizCallback(string message, long chatId, out InlineKeyboardMarkup menu)
        {
            int.TryParse(message.Replace("startquiz", string.Empty), out int quizIndex);

            userChatCurrentState[chatId] = ChatCurrentState.QuizPassingState; // устанавливаем текущее состояние чата  
            userQuizState[chatId] = _dataService.Quizzes[quizIndex];          // устанавливаем текущую викторину для текущего чата
            userQuizQuestionState[chatId] = 0;                                // Устанавливаем начальный вопрос
            userCorrectAnswers[chatId] = 0;                                   // Сбрасываем счётчик правильных ответов

            string response = $"{userQuizState[chatId].Topic}! \nОсь ваше перше питання: \n{SendNextQuestion(chatId, out menu)}";
            return response;
        }
        private string GiveResponseToDeleteQuizNumberCallback(long chatId, string message, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = "Невірний ввод.";
            if (userChatCurrentState.ContainsKey(chatId) && userChatCurrentState[chatId] == ChatCurrentState.QuizDeletionState)
            {
                menu = GetYesNoMenu();
                int.TryParse(message.Replace("deletequiznumber", string.Empty), out int quizIndex);
                response = $"Підтвердження видалення викторини: \n<b>{_dataService.Quizzes[quizIndex].Topic}</b>";
                _dataService.Quizzes[quizIndex].IsActive = false;
                Console.WriteLine(_dataService.Quizzes[quizIndex].Topic);
            }
            return response;
        }
        #endregion
        private string StartQuizCreation(long chatId)
        {
            var newQuizState = userCreateQuizState[chatId];
            newQuizState.CurrentStep = QuizStep.EnterTitle;
            string response = "Давайте створимо нову вікторину! \n<b>Будь ласка, введіть назву вікторини:</b>";
            return response;
        }
        private string SendNextQuestion(long chatId, out InlineKeyboardMarkup menu)
        {
            int questionIndex = userQuizQuestionState[chatId];
            var questionItem = userQuizState[chatId].Questions[questionIndex];
            var question = questionItem.Question;

            menu = QuizMenu(questionItem);
            string response = $"Вопрос {questionIndex + 1}: \n<b>{question}</b> ";
            return response;
        }
        private string ProcessQuizStep(Update update, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = "Невірній ввод.";

            long chatId = update.Message.Chat.Id;
            var state = userCreateQuizState[chatId];
            string input = string.Empty;
            string callback = string.Empty;

            if (userCreateQuizState.ContainsKey(chatId) && userChatCurrentState[chatId] == ChatCurrentState.NewQuizCreationState)
            {
                if (update.Message != null)
                {
                    input = update.Message.Text;
                }
                if (update.CallbackQuery != null)
                {
                    callback = update.CallbackQuery.Data.ToLower();
                }

                switch (state.CurrentStep)
                {
                    case QuizStep.EnterTitle:
                        state.Title = input;
                        state.CurrentStep = QuizStep.EnterQuestion;
                        response = "Будь ласка, введіть перше питання:";
                        break;
                    case QuizStep.EnterQuestion:
                        var question = new QuestionItem { Question = input };
                        state.Questions.Add(question);
                        state.CurrentStep = QuizStep.EnterOption1;
                        response = "Введіть варіант 1:";
                        break;
                    case QuizStep.EnterOption1:
                        state.Questions[^1].Options[0] = input;
                        state.CurrentStep = QuizStep.EnterOption2;
                        response = "Введіть варіант 2:";
                        break;
                    case QuizStep.EnterOption2:
                        state.Questions[^1].Options[1] = input;
                        state.CurrentStep = QuizStep.EnterOption3;
                        response = "Введіть варіант 3:";
                        break;
                    case QuizStep.EnterOption3:
                        state.Questions[^1].Options[2] = input;
                        state.CurrentStep = QuizStep.EnterOption4;
                        response = "Введіть варіант 4:";
                        break;
                    case QuizStep.EnterOption4:
                        state.Questions[^1].Options[3] = input;
                        state.CurrentStep = QuizStep.EnterCorrectOption;
                        response = "Будь ласка, введіть номер правильного варіанту (1-4):";
                        break;
                    case QuizStep.EnterCorrectOption:
                        if (int.TryParse(input, out int correctOption) && correctOption >= 1 && correctOption <= 4)
                        {
                            state.Questions[^1].CorrectOptionIndex = correctOption - 1; // Зберігаємо як індекс (0-3)
                            state.Questions[^1].Answer = state.Questions[^1].Options[state.Questions[^1].CorrectOptionIndex];
                            state.CurrentStep = QuizStep.EnterQuestionOrFinish;
                            menu = SendNextNewQuestionOrFinishMenu();
                            response = "Що б ви хотіли зробити далі?";
                        }
                        else
                        {
                            response = "Недійсний варіант. Будь ласка, введіть число між 1 and 4:";
                        }
                        break;
                    case QuizStep.EnterQuestionOrFinish:
                        if (callback == "nextquestion")
                        {
                            state.CurrentStep = QuizStep.EnterQuestion;
                            response = "Введіть наступне питання:";
                        }
                        else if (callback == "finishquiz")
                        {
                            response = GiveResponseToFinishQuizCallback(chatId);
                        }
                        else
                        {
                            menu = SendNextNewQuestionOrFinishMenu();
                            response = "Що б ви хотіли зробити далі?";
                        }
                        break;
                }
            }
            return response;
        }
        protected virtual async Task OnNewQuizCreated(NewQuizCreatedEventArgs e)
        {
            Func<object, NewQuizCreatedEventArgs, Task> handler = NewQuizCreated;
            if (handler != null)
            {
                handler.Invoke(this, e);
            }
        }
    }
}
