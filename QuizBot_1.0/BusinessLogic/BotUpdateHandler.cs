using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using QuizBot_1._0.Entities;
using QuizBot_1._0.Infrastructure;
using System.Text;
using User = QuizBot_1._0.Entities.User;

namespace QuizBot_1._0.BusinessLogic
{
    public class BotUpdateHandler
    {
        public event Func<object, NewQuizCreatedEventArgs, Task> NewQuizCreated;

        Dictionary<long, Quiz> userQuizState = new();              // ChatId -> текущая викторина
        Dictionary<long, User> userProgressState = new();          // ChatId -> Progress текущего User
        Dictionary<long, QuizState> userCreatedQuizState = new();  // ChatId -> текущая создаваемая викторина

        Dictionary<long, int> userCorrectAnswers = new();          // ChatId -> правильные ответы
        Dictionary<long, int> userQuizQuestionState = new();       // ChatId -> текущий вопрос

        DataService _dataService;

        public BotUpdateHandler()
        {
            _dataService = new DataService();
        }
        public string HandleMessage(Update update, out InlineKeyboardMarkup menu)
        {
            var message = update.Message;
            var chatId = message.Chat.Id;

            string response = string.Empty;
            menu = null;

            // Команда /start
            if (message.Text == "/start")
            {
                if (_dataService.Quizzes == null || _dataService.Quizzes.Count == 0)
                {
                    response = "Немає доступних вікторін.";
                    menu = null;
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
            else if (message.Text == "/create_new")
            {
                userCreatedQuizState[chatId] = new QuizState();
                response = StartQuizCreation(chatId);
                menu = null;
            }
            else if (userCreatedQuizState.ContainsKey(chatId))
            {
                response = ProcessQuizStep(update, out menu);
            }

            else if (message.Text == "/info")
            {
                string info = string.Empty;

                if (_dataService.Users.Count == 0)
                {
                    info = "Нет информации";
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

                    info = sb.ToString();
                }
                Console.WriteLine(info);
                response = info;
            }
            return response;
        }
        public string HandleCallbackQuery(Update update, out InlineKeyboardMarkup menu)
        {
            string subresponse;
            string response = string.Empty;
            menu = null;

            var callbackData = update.CallbackQuery.Data;
            var chatId = update.CallbackQuery.Message.Chat.Id;
            var message = callbackData.ToLower();

            if (message.StartsWith("startquiz"))
            {
                int.TryParse(message.Replace("startquiz", string.Empty), out int quizIndex);

                userQuizState[chatId] = _dataService.Quizzes[quizIndex]; // устанавливаем текущую викторину для текущего чата
                userQuizQuestionState[chatId] = 0; // Устанавливаем начальный вопрос
                userCorrectAnswers[chatId] = 0; // Сбрасываем счётчик правильных ответов

                response = $"{userQuizState[chatId].Topic}! \nОсь ваше перше питання: \n{SendNextQuestion(chatId, out menu)}";
            }
            if (message.StartsWith("/a"))
            {
                if (userQuizQuestionState.ContainsKey(chatId))
                {
                    int questionIndex = userQuizQuestionState[chatId];
                    int.TryParse(message.Replace("/a", string.Empty), out int index);

                    // Проверяем правильность ответа
                    if (userQuizState[chatId].Questions[questionIndex].CorrectOptionIndex == index)
                    {
                        userCorrectAnswers[chatId]++;
                        subresponse = "Вірно!";
                    }
                    else
                    {
                        subresponse = $"Невірно! Вірна відповідь: {userQuizState[chatId].Questions[questionIndex].Answer}";
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


                        response = $"Викторина завершена! Ви вірно відповіли на {correctAnswers} из {userQuizState[chatId].Questions.Count} питань.";

                        // Сбрасываем состояние пользователя
                        userQuizState.Remove(chatId);
                        userCorrectAnswers.Remove(chatId);
                        userQuizQuestionState.Remove(chatId);
                        userProgressState.Remove(chatId);
                    }
                }
            }
            else if (userCreatedQuizState.ContainsKey(chatId))
            {
                var state = userCreatedQuizState[chatId];
                if (state.CurrentStep == QuizStep.EnterQuestionOrFinish)
                {
                    if (message == "nextquestion")
                    {
                        state.CurrentStep = QuizStep.EnterQuestion;
                        response = "Будь ласка, введіть наступне питання:";
                    }
                    else if (message == "finishquiz")
                    {
                        menu = null;
                        response = FinishQuizCreation(chatId);
                    }
                    else
                    {
                        string badOption = "Невірний ввод. Виберіть опцію в меню.";
                        menu = SendNextNewQuestionOrFinishMenu();
                        response = $"{badOption} \nЩо б ви хотіли зробити далі?";
                    }
                }
            }
            return response;
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
        private string StartQuizCreation(long chatId)
        {
            var newQuizState = userCreatedQuizState[chatId];
            newQuizState.CurrentStep = QuizStep.EnterTitle;
            string response = "Давайте створимо нову вікторину! \nБудь ласка, введіть назву вікторини:";
            return response;
        }
        private string FinishQuizCreation(long chatId)
        {
            var state = userCreatedQuizState[chatId];
            string response = $"Quiz '{state.Title}' created with {state.Questions.Count} questions!";

            Quiz quiz = new Quiz();
            quiz.Topic = state.Title;
            quiz.Questions = state.Questions;
            _dataService.SaveNewQuiz(quiz);
            userCreatedQuizState.Remove(chatId);

            string notificationMessage = $"У нас есть новая викторина! -{quiz.Topic}-\nПроверьте свои знания.";

            var chatIdCollection = _dataService.Users.Where(u => u.ChatId != 0).Select(u => u.ChatId).ToList();
            OnNewQuizCreated(new NewQuizCreatedEventArgs(chatIdCollection, notificationMessage));

            return response;
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
            string callbackData = "/a";
            for (int i = 0; i < questionItem.Options.Length; i++)
            {
                kb[i] = new InlineKeyboardButton[1];
                kb[i][0] = new InlineKeyboardButton { Text = questionItem.Options[i], CallbackData = callbackData + i.ToString() };
            }
            inlineKeyboard.InlineKeyboard = kb;
            return inlineKeyboard;
        }
        private string SendNextQuestion(long chatId, out InlineKeyboardMarkup menu)
        {
            int questionIndex = userQuizQuestionState[chatId];
            var questionItem = userQuizState[chatId].Questions[questionIndex];
            var question = questionItem.Question;

            menu = QuizMenu(questionItem);
            string response = $"Вопрос {questionIndex + 1}: {question}";
            return response;
        }
        private string ProcessQuizStep(Update update, out InlineKeyboardMarkup menu)
        {
            menu = null;
            string response = string.Empty;
            long chatId = update.Message.Chat.Id;
            var state = userCreatedQuizState[chatId];
            string input = string.Empty;
            string callback = string.Empty;
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
                        response = FinishQuizCreation(chatId);
                    }
                    else
                    {
                        menu = SendNextNewQuestionOrFinishMenu();
                        response = "What would you like to do next?";
                    }
                    break;
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
