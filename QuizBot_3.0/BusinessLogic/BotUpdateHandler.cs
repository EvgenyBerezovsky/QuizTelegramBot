

using QuizBot_3._0.Entities;
using QuizBot_3._0.Infrastructure.DbDataService;
using QuizBot_3._0.Infrastructure.XmlDataService;
using System.Text;
using Telegram.Bot.Extensions.KeyboardBuilders;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = QuizBot_3._0.Entities.User;

namespace QuizBot_3._0.BusinessLogic
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

        DbDataService _dataService;

        public BotUpdateHandler()
        {
            _dataService = new DbDataService();
        }
        public string HandleMessage(Update update, out QuestionItem poll, out IReplyMarkup menu)
        {
            poll = null;
            menu = null;
            string response = "Невірний ввод.";

            Console.WriteLine(update.Message.From.Id);
            var message = update.Message;
            var chatId = message.Chat.Id;

            switch (message.Text)
            {
                case "/start":
                    response = GiveResponseToStart(message, chatId, out menu);
                    break;
                case "🔎 Start quiz":
                    response = GiveResponseToStartQuiz(message, chatId, out menu);
                    break;
                case "📝 Add quiz":
                    response = GiveResponseToCreateNew(update, out menu);
                    break;
                case "🗑 Delete quiz":
                    response = GiveResponseToDeleteQuiz(chatId, out menu);
                    break;
                case "📈 Statistics":
                    response = GiveResponseToInfo(update, out menu);
                    break;

                default:
                    response = ProcessQuizStep(update, out menu);
                    break;
            }
            return response;
        }
        public string HandleCallbackQuery(Update update, out QuestionItem poll, out InlineKeyboardMarkup menu)
        {
            poll = null;
            menu = null;
            string response = "Невірний ввод.";

            var callbackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var message = callbackData.ToLower();

            switch (message)
            {
                case var mes when mes.StartsWith("startquiz"):
                    response = GiveResponseToStartQuizCallback(message, chatId, out poll, out menu);
                    break;
                //case var mes when mes.StartsWith("answer"):
                //    response = GiveResponseToAnswerCallback(message, chatId, out poll, out menu);
                //    break;
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
        public string HandlePollAnswer(Update update, out QuestionItem poll, out InlineKeyboardMarkup menu)
        {
            var pollAnswer = update.PollAnswer;
            var answerIndex = pollAnswer.OptionIds[0];
            long chatId = pollAnswer.User.Id;

            string subresponse = string.Empty;
            string response = string.Empty;
            poll = null;
            menu = null;

            if (userQuizQuestionState.ContainsKey(chatId) && userChatCurrentState[chatId] == ChatCurrentState.QuizPassingState)
            {
                int questionIndex = userQuizQuestionState[chatId];

                // Проверяем правильность ответа
                if (userQuizState[chatId].Questions[questionIndex].CorrectOptionIndex == answerIndex)
                {
                    userCorrectAnswers[chatId]++;
                    subresponse = "<b>✔ Вірно!</b>";
                }
                else
                {
                    subresponse = $"<b>✔ Невірно!</b> \n\nВірна відповідь: \n<b>{userQuizState[chatId].Questions[questionIndex].Answer}</b>";
                }

                // Переход к следующему вопросу
                userQuizQuestionState[chatId]++;
                if (userQuizQuestionState[chatId] < userQuizState[chatId].Questions.Count)
                {
                    response = $"{subresponse}\n{SendNextQuestion(chatId, out poll, out menu)}";
                }
                else
                {
                    int correctAnswers = userCorrectAnswers[chatId];
                    float result = (float)correctAnswers / (float)userQuizState[chatId].Questions.Count;
                    userProgressState[chatId].AddScore(DateTime.Now, userQuizState[chatId].Topic, result);

                    _dataService.AddNewUserOrUpdate(userProgressState[chatId]);

                    response = $"✔ Викторина завершена! \n\nВи вірно відповіли на <b>{correctAnswers}</b> из <b>{userQuizState[chatId].Questions.Count}</b> питань.";

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

        #region Created Menu
        private IReplyMarkup GetInfoMenu()
        {
            var kb = new InlineKeyboardButton[2][];
            kb[0] = new InlineKeyboardButton[1];
            kb[1] = new InlineKeyboardButton[1];
            kb[0][0] = InlineKeyboardButton.WithCallbackData("📖 Перегляд результатів користувачів", "ShowUsersInfo");
            kb[1][0] = InlineKeyboardButton.WithCallbackData("🪓 Видалення результатів користувачів", "CleanUsersInfo");
            var inlineKeyboard = new InlineKeyboardMarkup(kb);
            return inlineKeyboard;
        }
        private ReplyKeyboardMarkup GetNewMainMenu()
        {
            var keyboard = new ReplyKeyboardBuilder().AddRow(row => row.AddButton("🔎 Start quiz").AddButton("📈 Statistics"))
                                                                       .AddRow(row => row.AddButton("📝 Add quiz").AddButton("🗑 Delete quiz"));

            var replyMarkup = new ReplyKeyboardMarkup(keyboard)
            {
                ResizeKeyboard = true,
            };
            return replyMarkup;
        }
        private InlineKeyboardMarkup GetQuizToPassMenu()
        {
            var kb = new InlineKeyboardButton[_dataService.Quizzes.Count][];

            for (int i = 0; i < _dataService.Quizzes.Count; i++)
            {
                string callbackData = string.Concat("StartQuiz", i);
                string button = string.Concat("📌 ", _dataService.Quizzes[i].Topic);
                kb[i] = new InlineKeyboardButton[1];
                kb[i][0] = InlineKeyboardButton.WithCallbackData(button, callbackData);
            }
            var inlineKeyboard = new InlineKeyboardMarkup(kb);
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup GetYesNoMenu()
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("✅ Так", "yes"), InlineKeyboardButton.WithCallbackData("❎ Ні", "no") } });
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup GetDeleteQuizMenu()
        {
            var kb = new InlineKeyboardButton[_dataService.Quizzes.Count][];

            for (int i = 0; i < _dataService.Quizzes.Count; i++)
            {
                string text = $"{"🪓"} {_dataService.Quizzes[i].Topic.ToString()}";
                string callbackData = string.Concat("DeleteQuizNumber", i);
                kb[i] = new InlineKeyboardButton[1];
                kb[i][0] = InlineKeyboardButton.WithCallbackData(text, callbackData);
            }
            var inlineKeyboard = new InlineKeyboardMarkup(kb);
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup SendNextNewQuestionOrFinishMenu()
        {
            var kb = new InlineKeyboardButton[1][];
            kb[0] = new InlineKeyboardButton[2];
            kb[0][0] = InlineKeyboardButton.WithCallbackData("📌 Наступне питання", "NextQuestion");
            kb[0][1] = InlineKeyboardButton.WithCallbackData("✅ Завершити", "FinishQuiz");

            var inlineKeyboard = new InlineKeyboardMarkup(kb);
            return inlineKeyboard;
        }
        //private InlineKeyboardMarkup QuizMenu(QuestionItem questionItem)
        //{

        //    var kb = new InlineKeyboardButton[questionItem.Options.Length][];
        //    string callbackData = "answer";
        //    for (int i = 0; i < questionItem.Options.Length; i++)
        //    {
        //        kb[i] = new InlineKeyboardButton[1];
        //        kb[i][0] = InlineKeyboardButton.WithCallbackData(questionItem.Options[i], callbackData + i.ToString());
        //    }
        //    var inlineKeyboard = new InlineKeyboardMarkup(kb);
        //    return inlineKeyboard;
        //}
        #endregion

        #region GiveResponseToCommand Methods
        private string GiveResponseToStart(Message message, long chatId, out IReplyMarkup menu)
        {
            if (!userChatCurrentState.ContainsKey(chatId))
            {
                userChatCurrentState.Add(chatId, ChatCurrentState.StartState);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("<b>Виберіть відповідну дію:\n</b>");
            sb.AppendLine();
            sb.AppendLine("🔎 Start quiz - вибір завданнь за темами\n");
            sb.AppendLine("📝 Add quiz - додавання нового завдання. Можлива імплементація перевірки прав доступу вчитель/учень.\n");
            sb.AppendLine("📈 Statistics - перехід у меню результатів користувачів з оцінками та датами. Можлива імплементація перевірки прав доступу вчитель/учень.\n");
            sb.AppendLine("🗑 Delete quiz - редагування списку завдань. Можлива імплементація перевірки прав доступу вчитель/учень.\n");
            string response = sb.ToString();
            menu = GetNewMainMenu();
            return response;
        }
        private string GiveResponseToStartQuiz(Message message, long chatId, out IReplyMarkup menu)
        {
            Console.WriteLine(chatId);
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
                    response = "<b>✔ Немає доступних вікторін.</b>";
                    return response;
                }

                else
                {
                    if (!userProgressState.ContainsKey(chatId))
                    {
                        var userName = message.Chat.Username == null ? "Unknown_User" : message.Chat.Username;
                        userProgressState.Add(chatId, new User(chatId, userName));
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<b>👉 Виберіть вікторину:</b>");
                    sb.AppendLine();
                    sb.AppendLine("Вибір завдання за темой. Вибране завдання має бути пройдено повністю. Під час виконання завдання пункти основного меню недоступні.");
                   
                    response = sb.ToString();
                    menu = GetQuizToPassMenu();
                }
            }
            return response;
        }
        private string GiveResponseToInfo(Update update, out IReplyMarkup menu)
        {
            menu = null;
            string response = "Невірний ввод.";
            long chatId = update.Message.Chat.Id;

            if (!userChatCurrentState.ContainsKey(chatId)) userChatCurrentState.Add(chatId, ChatCurrentState.StartState);
            if (userChatCurrentState[chatId] == ChatCurrentState.StartState)
            {
                userChatCurrentState[chatId] = ChatCurrentState.ResultsProcessState;
                menu = GetInfoMenu();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<b>👉 Виберіть потрібну дію?</b>");
                sb.AppendLine();
                sb.AppendLine("Меню статистики користувачів з можливістю перегляду та видалення результатів проходження завдань з оцінками та датами.");

                response = sb.ToString();
            }
            return response;
        }
        private string GiveResponseToShowUserInfoCallback(long chatId)
        {
            string response = "Невірний ввод.";

            if (!userChatCurrentState.ContainsKey(chatId)) userChatCurrentState.Add(chatId, ChatCurrentState.StartState);
            if (userChatCurrentState.ContainsKey(chatId) && (userChatCurrentState[chatId] is ChatCurrentState.ResultsProcessState | userChatCurrentState[chatId] is ChatCurrentState.StartState))
            {
                if (_dataService.Users.Count == 0)
                {
                    response = "Немає інформації";
                }
                else
                {
                    var sb = new StringBuilder();
                    foreach (var user in _dataService.Users)
                    {
                        sb.AppendLine($"Пользователь {user.UserName}:");
                        if (user.Scores.Count == 0)
                        {
                            sb.AppendLine("Немає інформації.");
                        }
                        foreach (var score in user.Scores)
                        {
                            sb.AppendLine($"Тема: {score.Topic}");
                            sb.AppendLine($"Бал: {(int)(score.Result * 100)}");
                            sb.AppendLine($"Час: {score.Time}");
                            sb.AppendLine(new string('-', 10));
                        }
                    }

                    response = sb.ToString();
                }
                userChatCurrentState[chatId] = ChatCurrentState.StartState;
            }
            return response;
        }
        private string GiveResponseToCreateNew(Update update, out IReplyMarkup menu)
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
                    response = $"✔ Завдання \n<b>-'{state.Title}'-</b> створене. \nКількість питань: <b> {state.Questions.Count} </b>.";

                    Quiz quiz = new Quiz();
                    quiz.Topic = state.Title;
                    quiz.Questions = state.Questions;
                    quiz.IsActive = true;
                    _dataService.SaveNewQuiz(quiz);

                    userCreateQuizState.Remove(chatId);
                    userChatCurrentState[chatId] = ChatCurrentState.StartState;


                    string notificationMessage = $"📚 У нас є нова вікторина! \n<b> -{quiz.Topic}- </b> \nПеревірте свої знання.";
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
                response = "<b>✔ Дані видалено.</b>";
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
                response = "<b>✔ Видалення скасовано.</b>";
                userChatCurrentState[chatId] = ChatCurrentState.StartState;
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
                response = "<b>✔ Дані оновлені</b>";
                userChatCurrentState[chatId] = ChatCurrentState.StartState;
            }

            return response;
        }
        private string GiveResponseToDeleteQuiz(long chatId, out IReplyMarkup menu)
        {
            menu = null;
            string response = "Невірний ввод.";
            if (userChatCurrentState.ContainsKey(chatId) && (userChatCurrentState[chatId] is ChatCurrentState.ResultsProcessState || userChatCurrentState[chatId] is ChatCurrentState.StartState))
            {
                userChatCurrentState[chatId] = ChatCurrentState.QuizDeletionState;
                menu = GetDeleteQuizMenu();
                response = "👉 Виберіть вікторину зі списку. \nНе переривайте операцію. Далі ви зможете підтвердити чи скасувати видалення.";
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
                response = "👉 Виберіть вікторину для видалення.";
            }
            return response;
        }
        //private string GiveResponseToAnswerCallback(string message, long chatId, out QuestionItem poll, out InlineKeyboardMarkup menu)
        //{
        //    poll = null;
        //    menu = null;
        //    string response = string.Empty;
        //    string subresponse = string.Empty;

        //    if (userQuizQuestionState.ContainsKey(chatId) && userChatCurrentState[chatId] == ChatCurrentState.QuizPassingState)
        //    {
        //        int questionIndex = userQuizQuestionState[chatId];
        //        int.TryParse(message.Replace("answer", string.Empty), out int index);

        //        // Проверяем правильность ответа
        //        if (userQuizState[chatId].Questions[questionIndex].CorrectOptionIndex == index)
        //        {
        //            userCorrectAnswers[chatId]++;
        //            subresponse = "<b>Вірно!</b>";
        //        }
        //        else
        //        {
        //            subresponse = $"<b>Невірно!</b> \nВірна відповідь: <b>{userQuizState[chatId].Questions[questionIndex].Answer}</b>";
        //        }

        //        // Переход к следующему вопросу
        //        userQuizQuestionState[chatId]++;
        //        if (userQuizQuestionState[chatId] < userQuizState[chatId].Questions.Count)
        //        {
        //            response = $"{subresponse}\n{SendNextQuestion(chatId, out poll, out menu)}";
        //        }
        //        else
        //        {
        //            int correctAnswers = userCorrectAnswers[chatId];
        //            float result = (float)correctAnswers / (float)userQuizState[chatId].Questions.Count;
        //            userProgressState[chatId].AddScore(DateTime.Now, userQuizState[chatId].Topic, result);


        //            _dataService.AddNewUserOrUpdate(userProgressState[chatId]);


        //            response = $"Викторина завершена! \nВи вірно відповіли на <b>{correctAnswers}</b> из <b>{userQuizState[chatId].Questions.Count}</b> питань.";

        //            // Сбрасываем состояние пользователя
        //            userChatCurrentState[chatId] = ChatCurrentState.StartState;
        //            userQuizState.Remove(chatId);
        //            userCorrectAnswers.Remove(chatId);
        //            userQuizQuestionState.Remove(chatId);
        //            userProgressState.Remove(chatId);
        //        }

        //    }
        //    else
        //    {
        //        response = "Невірний ввод!";
        //    }
        //    return response;
        //}
        private string GiveResponseToStartQuizCallback(string message, long chatId, out QuestionItem poll, out InlineKeyboardMarkup menu)
        {
            int.TryParse(message.Replace("startquiz", string.Empty), out int quizIndex);

            userChatCurrentState[chatId] = ChatCurrentState.QuizPassingState; // устанавливаем текущее состояние чата  
            userQuizState[chatId] = _dataService.Quizzes[quizIndex];          // устанавливаем текущую викторину для текущего чата
            userQuizQuestionState[chatId] = 0;                                // Устанавливаем начальный вопрос
            userCorrectAnswers[chatId] = 0;                                   // Сбрасываем счётчик правильных ответов

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{userQuizState[chatId].Topic}!");
            sb.AppendLine();
            sb.AppendLine("Ось ваше перше питання:");
            sb.AppendLine($"{SendNextQuestion(chatId, out poll, out menu)}");
            string response =  sb.ToString();
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
                response = $"👉 Підтвердження видалення викторини: \n\n<b>{_dataService.Quizzes[quizIndex].Topic}</b>";
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

            StringBuilder sb = new StringBuilder();
            sb.Append("<b>👉 Давайте створимо нове завдання!</b>");
            sb.AppendLine();
            sb.AppendLine("\nCтворення нового завдання. Послідовно запитується:");
            sb.AppendLine();
            sb.AppendLine("✏ Тема завдання.");
            sb.AppendLine("✏ Перше питання завдання.");
            sb.AppendLine("✏ Варіанти відповідей.");
            sb.AppendLine("✏ Номер правильної відповіді.");
            sb.AppendLine("✏ Наступне питання чи завершення створення завдання.");
            sb.AppendLine("До завершення створення нового завдання пункти основного меню недоступні.\n"); 
            sb.AppendLine("<b>📌 Будь ласка, введіть тему завдання:</b>");
            string response = sb.ToString();
            return response;
        }
        private string SendNextQuestion(long chatId, out QuestionItem poll, out InlineKeyboardMarkup menu)
        {
            menu = null;
            int questionIndex = userQuizQuestionState[chatId];
            var questionItem = userQuizState[chatId].Questions[questionIndex];
            var question = questionItem.Question;

            //menu = QuizMenu(questionItem);
            string response = $"\n📌 <b>Вопрос {questionIndex + 1}</b>";
            poll = questionItem;
            return response;
        }
        private string ProcessQuizStep(Update update, out IReplyMarkup menu)
        {
            menu = null;
            string response = "Невірній ввод.";
            Console.WriteLine(update.Message.Text);
            long chatId = update.Message.Chat.Id;
            if (!userCreateQuizState.ContainsKey(chatId))
            {
                return response;
            }
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
                        response = "📌<b> Будь ласка, введіть перше питання:</b>";
                        break;
                    case QuizStep.EnterQuestion:
                        var question = new QuestionItem { Question = input };
                        state.Questions.Add(question);
                        state.CurrentStep = QuizStep.EnterOption1;
                        response = "📌 <b>Введіть варіант відповіді 1:</b>";
                        break;
                    case QuizStep.EnterOption1:
                        state.Questions[^1].Options[0] = input;
                        state.CurrentStep = QuizStep.EnterOption2;
                        response = "📌<b>Введіть варіант відповіді 2:</b>";
                        break;
                    case QuizStep.EnterOption2:
                        state.Questions[^1].Options[1] = input;
                        state.CurrentStep = QuizStep.EnterOption3;
                        response = "📌 <b>Введіть варіант відповіді 3:</b>";
                        break;
                    case QuizStep.EnterOption3:
                        state.Questions[^1].Options[2] = input;
                        state.CurrentStep = QuizStep.EnterOption4;
                        response = "📌 <b>Введіть варіант відповіді 4:</b>";
                        break;
                    case QuizStep.EnterOption4:
                        state.Questions[^1].Options[3] = input;
                        state.CurrentStep = QuizStep.EnterCorrectOption;
                        response = "📌 <b>Будь ласка, введіть номер правильного варіанту (1-4):</b>";
                        break;
                    case QuizStep.EnterCorrectOption:
                        if (int.TryParse(input, out int correctOption) && correctOption >= 1 && correctOption <= 4)
                        {
                            state.Questions[^1].CorrectOptionIndex = correctOption - 1; // Зберігаємо як індекс (0-3)
                            state.Questions[^1].Answer = state.Questions[^1].Options[state.Questions[^1].CorrectOptionIndex];
                            state.CurrentStep = QuizStep.EnterQuestionOrFinish;
                            menu = SendNextNewQuestionOrFinishMenu();
                            response = "👉 Що б ви хотіли зробити далі?";
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
                            response = "📌 <b>Введіть наступне питання:</b>";
                        }
                        else if (callback == "finishquiz")
                        {
                            response = GiveResponseToFinishQuizCallback(chatId);
                        }
                        else
                        {
                            menu = SendNextNewQuestionOrFinishMenu();
                            response = "👉 Що б ви хотіли зробити далі?";
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
