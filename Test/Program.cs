using System;
using System.IO;
using System.Threading.Tasks;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace QuizBot
{
    class Program
    {
        static string token = "YOUR_API_KEY";
        static string filePath = "quiz_results.txt";

        static async Task Main(string[] args)
        {
            InitializeFile();
            var bot = new TelegramBot(token);
            var me = await bot.MakeRequestAsync(new GetMe());
            Console.WriteLine($"Hello, my name is {me.FirstName}");

            await bot.MakeRequestAsync(new DeleteWebhook());

            long offset = 0;

            while (true)
            {
                var updates = await bot.MakeRequestAsync(new GetUpdates() { Offset = offset });
                foreach (var update in updates)
                {
                    if (update.Message != null && update.Message.Text != null)
                    {
                        await Bot_OnMessage(bot, update.Message);
                    }
                    else if (update.PollAnswer != null)
                    {
                        await Bot_OnPollAnswer(bot, update.PollAnswer);
                    }

                    offset = update.UpdateId + 1;
                }

                await Task.Delay(1000); // Затримка для уникнення частого опитування API
            }
        }

        static void InitializeFile()
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
        }

        static async Task Bot_OnMessage(TelegramBot bot, Message message)
        {
            if (message.Text.ToLower() == "/start")
            {
                await bot.MakeRequestAsync(new SendMessage(message.Chat.Id, "Привіт! Готові до викторини? Ось перше питання:"));
                await SendQuizQuestion(bot, message.Chat.Id);
            }
        }

        static async Task SendQuizQuestion(TelegramBot bot, long chatId)
        {
            var question = "Яка столиця України?";
            var options = new[] { "Київ", "Львів", "Одеса", "Харків" };

            await bot.MakeRequestAsync(new SendPoll(chatId, question, options)
            {
                Type = "quiz",
                CorrectOptionId = 0
            });
        }

        static async Task Bot_OnPollAnswer(TelegramBot bot, PollAnswer pollAnswer)
        {
            var user = pollAnswer.User;
            int score = CalculateScore(pollAnswer.OptionIds);

            SaveQuizResultToFile(user.Id, user.Username, score);

            await bot.MakeRequestAsync(new SendMessage(user.Id, $"Вітаємо, {user.FirstName}! Ви набрали {score} балів."));
        }

        static int CalculateScore(int[] optionIds)
        {
            return optionIds.Length == 1 && optionIds[0] == 0 ? 1 : 0;
        }

        static void SaveQuizResultToFile(long userId, string username, int score)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"UserId: {userId}, Username: {username}, Score: {score}");
            }
        }
    }
}
