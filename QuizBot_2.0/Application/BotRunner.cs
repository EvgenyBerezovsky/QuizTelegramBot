using QuizBot_2._0.BusinessLogic;
using QuizBot_2._0.Entities;
using QuizBot_2._0.Infrastructure;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBot_2._0.Application
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
                            Console.WriteLine(update.Message.Text);
                            await HandleMessageAsync(update);
                        }
                        else if (update.CallbackQuery != null)
                        {
                            await HandleCallbackQueryAsync(update);
                        }
                        else if (update.PollAnswer != null)
                        {
                            await HandlePollAnswerAsync(update);
                        }
                        offset = update.Id + 1;
                    }
                    await Task.Delay(1000);
                }
            }
        }

        private async Task HandlePollAnswerAsync(Update update)
        {
            QuestionItem poll;
            InlineKeyboardMarkup menu;
           
            var chatId = update.PollAnswer.User.Id;

            string response = _botUpdateHandler.HandlePollAnswer(update, out poll, out menu);

            await _telegramService.SendMessageAsync(chatId, response, poll, menu);
        }

        private async Task HandleMessageAsync(Update update)
        {
            var message = update.Message;
            var chatId = message.Chat.Id;

            QuestionItem poll;
            InlineKeyboardMarkup menu;
            
            string response = _botUpdateHandler.HandleMessage(update, out poll, out menu);

            await _telegramService.SendMessageAsync(chatId, response, poll, menu);
        }
        private async Task HandleCallbackQueryAsync(Update update)
        {
            
            QuestionItem poll;
            InlineKeyboardMarkup menu;

            var chatId = update.CallbackQuery.Message.Chat.Id;

            string response = _botUpdateHandler.HandleCallbackQuery(update, out poll, out menu);

            await _telegramService.SendMessageAsync(chatId, response, poll, menu);
        }

    }
}
