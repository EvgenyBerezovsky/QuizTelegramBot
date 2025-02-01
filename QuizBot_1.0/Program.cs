using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using QuizBot_1._0.Application;
using QuizBot_1._0.BusinessLogic;
using QuizBot_1._0.Entities;
using QuizBot_1._0.Infrastructure;
using System.Text;
using User = QuizBot_1._0.Entities.User;

public class Program
{
    #region token
    // Створюємо змінну, що буде зберігати налаштування на наш бот
    private const string TelegramToken = "7968088181:AAHjHEiGSvxZrdtq4pJYmaeHF31xODTWq64";
    #endregion
    static async Task Main(string[] args)
    {
        #region EncodingSettings
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        #endregion

        var botUpdateHandler = new BotUpdateHandler();
        var telegramService = new TelegramService(TelegramToken);
        var cancellationTokenSource = new CancellationTokenSource();

        Console.WriteLine("Bot started...");
        var botRunner = new BotRunner(telegramService, botUpdateHandler, cancellationTokenSource.Token);
        await botRunner.Run();

        Console.ReadLine();
    }
}