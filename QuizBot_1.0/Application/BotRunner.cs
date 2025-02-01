using NetTelegramBotApi.Types;
using QuizBot_1._0.BusinessLogic;
using QuizBot_1._0.Infrastructure;

namespace QuizBot_1._0.Application
{
    public class BotRunner
    {
        private CancellationToken _token;
        private TelegramService _telegramService;
        private BotUpdateHandler _botUpdateHandler;
        public BotRunner(TelegramService telegramService, BotUpdateHandler botUpdateHandler, CancellationToken token)
        {
            _token = token;
            _telegramService = telegramService;
            _botUpdateHandler = botUpdateHandler;
            _botUpdateHandler.NewQuizCreated += _botUpdateHandler_NewQuizCreatedAsync;
        }

        private async Task _botUpdateHandler_NewQuizCreatedAsync(object? sender, Entities.NewQuizCreatedEventArgs e)
        {
            List<long> Chats = e.ChatIdCollection.ToList();
            string notification = e.NotificationMessage;

            foreach (var ch in Chats)
            {
                if (ch != 0)
                await _telegramService.SendMessageAsync(ch, notification);
            }
        }

        /// <summary>
        /// Метод базового циклу отримання та обробки вхідних повідомлень
        /// </summary>
        public async Task Run()
        {
            long offset = 0;

            while (!_token.IsCancellationRequested)
            {
                {
                    var updates = await _telegramService.GetUpdatesAsync(offset);
                    foreach (var update in updates)
                    {
                        if (update.Message != null)
                        {
                            await HandleMessageAsync(update);
                        }
                        else if (update.CallbackQuery != null)
                        {
                            await HandleCallbackQueryAsync(update);
                        }
                        offset = update.UpdateId + 1;
                    }
                    await Task.Delay(1000);
                }
            }
        }
        private async Task HandleMessageAsync(Update update)
        {
            var message = update.Message;
            var chatId = message.Chat.Id;

            InlineKeyboardMarkup menu;
            string response = _botUpdateHandler.HandleMessage(update, out menu);

            await _telegramService.SendMessageAsync(chatId, response, menu);
        }
        private async Task HandleCallbackQueryAsync(Update update)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;

            InlineKeyboardMarkup menu;
            string response = _botUpdateHandler.HandleCallbackQuery(update, out menu);

            await _telegramService.SendMessageAsync(chatId, response, menu);
        }
       
    }
}
