using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using QuizBot.Entities;
using QuizBot.Infrastructure;
using System.Text;
using User = QuizBot.Entities.User;

public class Program
{
    #region token
    // Створюємо змінну, що буде зберігати налаштування на наш бот
    private const string token = "7968088181:AAHjHEiGSvxZrdtq4pJYmaeHF31xODTWq64";
    #endregion

    static DataService dataService = new DataService();
    static UserService userService = new UserService();

    static Dictionary<long, User> userProgressState = new(); // ChatId -> Progress текущего User

    static Dictionary<long, Quiz> userQuizState = new(); // ChatId -> текущая викторина
    static Dictionary<long, int> userQuizQuestionState = new(); // ChatId -> текущий вопрос
    static Dictionary<long, int> userCorrectAnswers = new(); // ChatId -> правильные ответы

    static List<Quiz> quizzes = dataService.GetAllQuizzes();
    static List<User> users = userService.GetAllUsers();

    private static TelegramBot bot;
    private static Dictionary<long, QuizState> userCreatedQuizState = new Dictionary<long, QuizState>(); // ChatId -> текущая создаваемая викторина
    static async Task Main(string[] args)
    {
        #region EncodingSettings
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        #endregion
        #region TestData
        //Quiz quiz1 = new Quiz()
        //{
        //    Topic = "АРХІТЕКТУРНО-БУДІВЕЛЬНА ВІКТОРИНА",
        //    Questions = new List<QuestionItem>
        //    {
        //        new QuestionItem("Як перекладається з давньогрецької слово «архітектор»?", "Головний будівельник", new string[]{ "Головний будівельник", "Головний художник", "Мудрий геометр", "Старий скульптор"}, 0),
        //        new QuestionItem("Хто побудував найбільшу піраміду?", "Хеопс", new string[] { "Хефрен", "Рамсес", "Мавроді", "Хеопс" }, 3),
        //        new QuestionItem("Хто першим ввів термін ЗОЛОТИЙ ПЕРЕТИН?", "Леонардо да Вінчі", new string[] { "Джотто", "Фідій", "Леонардо да Вінчі", "Калликрат" }, 2),
        //        new QuestionItem("Як називається архітектурно оформлений вхід у будівлю?", "Портал", new string[] { "Портал", "Сайт", "Блог", "Форум" }, 0),
        //        new QuestionItem("Назва якого архітектурного стилю перекладається з французького як «імперія»?", "Ампір", new string[] { "Готика", "Ампір", "Бароко", "Модерн" }, 1),
        //        new QuestionItem("Як називали будівельника в старовину?", "Зодчий", new string[] { "Керманич", "Головний художник", "Зодчий", "Старий скульптор" }, 2),
        //        new QuestionItem("Майстер з кельмою - це...?", "Муляр", new string[] { "Муляр", "Тесляр", "Електрик", "Кошторисник" }, 0),
        //        new QuestionItem("Як в архітектурі називають перший поверх будівлі?", "Цоколь", new string[] { "Цоколь", "Бельетаж", "Пентхаус", "Мансарда" }, 0),
        //        new QuestionItem("Як називається опорна стіна будинку?", "Несуча", new string[] { "Плакуча", "Несуча", "Неминуча", "Страшнюча" }, 1),
        //        new QuestionItem("Як називається дерев'яна споруда, стіни якого зібрані з оброблених колод?", "Зруб", new string[] { "Зруб", "Зуб", "Мазанка", "Сарай" }, 0),
        //        new QuestionItem("Як називається будівельна машина для переміщення вантажів за допомогою рухомого каната (ланцюга)?", "Лебідка", new string[] { "Воронка", "Журавель", "Лебідка", "Сорока" }, 2)
        //    }
        //};
        //Quiz quiz2 = new Quiz()
        //{
        //    Topic = "ЗАЯЧА (КРОЛЯЧА) ВІКТОРИНА",
        //    Questions = new List<QuestionItem>
        //    {
        //        new QuestionItem("Куди заєць біжить швидше: в гору або з гори??", "В гору", new string[] { "В суп", "В нору", "В гору", "С гори" }, 2),
        //        new QuestionItem("Сліпими чи зрячими народжуються зайчата?", "Зрячими", new string[] { "Зрячими", "Сліпими", "Балакучими", "Хитручими" }, 0),
        //        new QuestionItem("Яку назву отримали зайченята, які народилися в червні, коли колоситься жито цвіте гречка?", "Колосовички", new string[] { "Носовички", "Колосовички", "Сніговички", "Чувачки" }, 1),
        //        new QuestionItem("Назва якої європейської країни походить від фінікійського «і-шпанім» - «берег кроликів»?", "Іспанія", new string[] { "Лапландія", "Замбія", "Іспанія", "Зенландія" }, 2),
        //        new QuestionItem("Під яким кущем сидить заєць під час дощу?", "Під мокрим", new string[] { "Під крапівним", "Під капустним", "Під мокрим", "під морковним" }, 2),
        //        new QuestionItem("Живуть зайці в норах?", "Ні", new string[] { "Так", "Якщо кріт дозволив ", "Ні", "Якщо там є світло" }, 2),
        //        new QuestionItem("Чи вірно, що зуби-різці у зайців постійно ростуть і потребують у сточуванні?", "Так", new string[] { "Так", "Ні", "Тільки взимку", "Треба погуглить" }, 0),
        //        new QuestionItem("Яку швидкість може розвивати заєць?", "40", new string[] { "40", "60", "80", "5" }, 0),
        //        new QuestionItem("Чи вірно, що зайці-самці крупніше самок?", "Ні", new string[] { "Так", "Ні", "Якщо гарно поїв", "Запитаю татка" }, 1),
        //        new QuestionItem("Скільки зубів у зайця?", "28", new string[] { "28", "4", "12", "32" }, 0),
        //        new QuestionItem("Як називають зайців, які народилися восени?", "Листопадничками", new string[] { "Подосіновичками", "Жовтопузіками", "Листопадничками", "Хітрожопиками" }, 2),
        //    }
        //};
        //dataService.AddQuiz(quiz2);
        //dataService.AddQuiz(quiz1);
        #endregion

        bot = new TelegramBot(token, null);
        var cancellationTokenSource = new CancellationTokenSource();
        Console.WriteLine("Bot started...");

        await ReceiveMessagesAsync(cancellationTokenSource.Token);

        Console.ReadLine();
    }

    private static async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        long offset = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var updates = await bot.MakeRequestAsync(new GetUpdates { Offset = offset });
            foreach (var update in updates)
            {
                if (update.Message != null)
                {
                    await HandleMessage(update);
                }
                else if (update.CallbackQuery != null)
                {
                    await HandleCallbackQuery(update);
                }
                offset = update.UpdateId + 1;
            }
            await Task.Delay(1000);
        }
    }

    private static async Task HandleMessage(Update update)
    {
        var message = update.Message;
        var chatId = message.Chat.Id;

        if (!userProgressState.ContainsKey(chatId))
        {
            var userName = message.Chat.Username;
            userProgressState.Add(chatId, new User(chatId, userName));
        }


        // Команда /start
        if (message.Text == "/start")
        {
            if (quizzes == null || quizzes.Count == 0)
                await bot.MakeRequestAsync(new SendMessage(chatId, "Немає доступних вікторін."));
            else
                await ShowMainMenu(bot, chatId);
        }
        else if (message.Text == "/create_new")
        {
            userCreatedQuizState[chatId] = new QuizState();
            await StartQuizCreation(chatId);
        }
        else if (userCreatedQuizState.ContainsKey(chatId))
        {
            await ProcessQuizStep(chatId, update);
        }

        else if (message.Text == "/info")
        {
            string info = string.Empty;

            if (users == null)
            {
                info = "Нет информации";
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var user in users)
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
            await bot.MakeRequestAsync(new SendMessage(chatId, info));
        }
    }
    private static async Task HandleCallbackQuery(Update update)
    {
        var callbackData = update.CallbackQuery.Data;
        var chatId = update.CallbackQuery.Message.Chat.Id;
        var message = callbackData.ToLower();

        if (message.StartsWith("startquiz"))
        {
            int.TryParse(message.Replace("startquiz", string.Empty), out int quizIndex);
            //currentQuiz = quizzes[index];

            userQuizState[chatId] = quizzes[quizIndex]; // устанавливаем текущую викторину для текущего чата
            userQuizQuestionState[chatId] = 0; // Устанавливаем начальный вопрос
            userCorrectAnswers[chatId] = 0; // Сбрасываем счётчик правильных ответов

            await bot.MakeRequestAsync(new SendMessage(chatId, $"{userQuizState[chatId].Topic}! \n Ось ваше перше питання:"));
            await SendNextQuestion(bot, chatId);
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
                    await bot.MakeRequestAsync(new SendMessage(chatId, "Вірно!"));
                }
                else
                {
                    await bot.MakeRequestAsync(new SendMessage(chatId, $"Невірно! Вірна відповідь: {userQuizState[chatId].Questions[questionIndex].Answer}"));
                }

                // Переход к следующему вопросу
                userQuizQuestionState[chatId]++;
                if (userQuizQuestionState[chatId] < userQuizState[chatId].Questions.Count)
                {
                    await SendNextQuestion(bot, chatId);
                }
                else
                {
                    int correctAnswers = userCorrectAnswers[chatId];
                    float result = (float)correctAnswers / (float)userQuizState[chatId].Questions.Count;
                    userProgressState[chatId].AddScore(DateTime.Now, userQuizState[chatId].Topic, result);
                    userService.UpdateProgress(userProgressState[chatId]);


                    await bot.MakeRequestAsync(new SendMessage(chatId, $"Викторина завершена! Ви вірно відповіли на {correctAnswers} из {userQuizState[chatId].Questions.Count} питань."));

                    // Сбрасываем состояние пользователя
                    userQuizState.Remove(chatId);
                    userCorrectAnswers.Remove(chatId);
                    userQuizQuestionState.Remove(chatId);

                    await ShowMainMenu(bot, chatId);
                }
            }
        }
        if (userCreatedQuizState.ContainsKey(chatId))
        {
            var state = userCreatedQuizState[chatId];
            if (state.CurrentStep == QuizStep.EnterQuestionOrFinish)
            {
                if (message == "nextquestion")
                {
                    state.CurrentStep = QuizStep.EnterQuestion;
                    await bot.MakeRequestAsync(new SendMessage(chatId, "Будь ласка, введіть наступне питання:"));
                }
                else if (message == "finishquiz")
                {
                    await FinishQuizCreation(chatId);
                }
                else
                {
                    await bot.MakeRequestAsync(new SendMessage(chatId, "Невірний вхід. Виберіть опцію в меню."));
                    var menu = SendNextNewQuestionOrFinishMenu();
                    await bot.MakeRequestAsync(new SendMessage(chatId, "Що б ви хотіли зробити далі?") { ReplyMarkup = menu });
                }
            }
        }
    }


    private static async Task ProcessQuizStep(long chatId, Update update)
    {
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
                await bot.MakeRequestAsync(new SendMessage(chatId, "Будь ласка, введіть перше питання:"));
                break;
            case QuizStep.EnterQuestion:
                var question = new QuestionItem { Question = input };
                state.Questions.Add(question);
                state.CurrentStep = QuizStep.EnterOption1;
                await bot.MakeRequestAsync(new SendMessage(chatId, "Введіть варіант 1:"));
                break;
            case QuizStep.EnterOption1:
                state.Questions[^1].Options[0] = input;
                state.CurrentStep = QuizStep.EnterOption2;
                await bot.MakeRequestAsync(new SendMessage(chatId, "Введіть варіант 2:"));
                break;
            case QuizStep.EnterOption2:
                state.Questions[^1].Options[1] = input;
                state.CurrentStep = QuizStep.EnterOption3;
                await bot.MakeRequestAsync(new SendMessage(chatId, "Введіть варіант 3:"));
                break;
            case QuizStep.EnterOption3:
                state.Questions[^1].Options[2] = input;
                state.CurrentStep = QuizStep.EnterOption4;
                await bot.MakeRequestAsync(new SendMessage(chatId, "Введіть варіант 4:"));
                break;
            case QuizStep.EnterOption4:
                state.Questions[^1].Options[3] = input;
                state.CurrentStep = QuizStep.EnterCorrectOption;
                await bot.MakeRequestAsync(new SendMessage(chatId, "Будь ласка, введіть номер правильного варіанту (1-4):"));
                break;
            case QuizStep.EnterCorrectOption:
                if (int.TryParse(input, out int correctOption) && correctOption >= 1 && correctOption <= 4)
                {
                    state.Questions[^1].CorrectOptionIndex = correctOption - 1; // Зберігаємо як індекс (0-3)
                    state.Questions[^1].Answer = state.Questions[^1].Options[state.Questions[^1].CorrectOptionIndex];
                    state.CurrentStep = QuizStep.EnterQuestionOrFinish;
                    var menu = SendNextNewQuestionOrFinishMenu();
                    await bot.MakeRequestAsync(new SendMessage(chatId, "Що б ви хотіли зробити далі?") { ReplyMarkup = menu });
                }
                else
                {
                    await bot.MakeRequestAsync(new SendMessage(chatId, "Недійсний варіант. Будь ласка, введіть число між 1 and 4:"));
                }
                break;
            case QuizStep.EnterQuestionOrFinish:
                if (callback == "nextquestion")
                {
                    state.CurrentStep = QuizStep.EnterQuestion;
                    await bot.MakeRequestAsync(new SendMessage(chatId, "Введіть наступне питання:"));
                }
                else if (callback == "finishquiz")
                {
                    await FinishQuizCreation(chatId);
                }
                else
                {
                    await bot.MakeRequestAsync(new SendMessage(chatId, "Invalid input. Please select an option from the menu."));
                    var menu = SendNextNewQuestionOrFinishMenu();
                    await bot.MakeRequestAsync(new SendMessage(chatId, "What would you like to do next?") { ReplyMarkup = menu });
                }
                break;
        }
    }
    private static async Task FinishQuizCreation(long chatId)
    {
        var state = userCreatedQuizState[chatId];
        await bot.MakeRequestAsync(new SendMessage(chatId, $"Quiz '{state.Title}' created with {state.Questions.Count} questions!"));

        Quiz quiz = new Quiz();
        quiz.Topic = state.Title;
        quiz.Questions = state.Questions;
        quizzes.Add(quiz);
        dataService.AddQuiz(quiz);
        userCreatedQuizState.Remove(chatId);

        string notificationMessage = $"У нас есть новая викторина! -{quiz.Topic}-\n Проверьте свои знания.";

        foreach (var user in users)
        {
            await bot.MakeRequestAsync(new SendMessage(user.ChatId, notificationMessage));
        }
    }
    private static InlineKeyboardMarkup SendNextNewQuestionOrFinishMenu()
    {
        var inlineKeyboard = new InlineKeyboardMarkup();
        var kb = new InlineKeyboardButton[1][];


        kb[0] = new InlineKeyboardButton[2];
        kb[0][0] = new InlineKeyboardButton { Text = "Next Question", CallbackData = "NextQuestion" };
        kb[0][1] = new InlineKeyboardButton { Text = "Finish Quiz", CallbackData = "FinishQuiz" };

        inlineKeyboard.InlineKeyboard = kb;
        return inlineKeyboard;
        //await bot.MakeRequestAsync(new SendMessage(chatId, "What would you like to do next?") { ReplyMarkup = inlineKeyboard });
    }
    private static async Task StartQuizCreation(long chatId)
    {
        var newQuizState = userCreatedQuizState[chatId];
        newQuizState.CurrentStep = QuizStep.EnterTitle;
        await bot.MakeRequestAsync(new SendMessage(chatId, "Давайте створимо нову вікторину! \n Будь ласка, введіть назву вікторини:"));
    }


    private static async Task SendNextQuestion(TelegramBot bot, long chatId)
    {
        int questionIndex = userQuizQuestionState[chatId];
        var questionItem = userQuizState[chatId].Questions[questionIndex];
        var question = questionItem.Question;

        var kb = QuizMenu(questionItem);

        await bot.MakeRequestAsync(new SendMessage(chatId, $"Вопрос {questionIndex + 1}: {question}") { ReplyMarkup = kb });
    }
    static InlineKeyboardMarkup QuizMenu(QuestionItem questionItem)
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
    private static async Task ShowMainMenu(TelegramBot bot, long chatId)
    {

        var inlineKeyboard = new InlineKeyboardMarkup();
        var kb = new InlineKeyboardButton[quizzes.Count][];

        for (int i = 0; i < quizzes.Count; i++)
        {
            string callbackData = string.Concat("StartQuiz", i);
            kb[i] = new InlineKeyboardButton[1];
            kb[i][0] = new InlineKeyboardButton { Text = quizzes[i].Topic.ToString(), CallbackData = callbackData };
        }
        inlineKeyboard.InlineKeyboard = kb;

        await bot.MakeRequestAsync(new SendMessage(chatId, "Виберіть вікторину:") { ReplyMarkup = inlineKeyboard });
    }
}