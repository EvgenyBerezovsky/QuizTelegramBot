using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace QuizBot_1._0.Infrastructure
{
    public class TelegramService
    {
        private readonly TelegramBot _bot;

        public TelegramService(string token)
        {
            _bot = new TelegramBot(token, null);
        }
        public async Task<IEnumerable<Update>> GetUpdatesAsync(long offset)
        {
            return  await _bot.MakeRequestAsync(new GetUpdates() { Offset = offset });
        }

        public async Task SendMessageAsync(long chatId, string text, InlineKeyboardMarkup? replyMarkup = null)
        {
            await _bot.MakeRequestAsync(new SendMessage(chatId, text)
            {
                ReplyMarkup = replyMarkup
            });
        }

        public async Task AnswerCallbackQueryAsync(string callbackQueryId)
        {
            await _bot.MakeRequestAsync(new AnswerCallbackQuery(callbackQueryId));
        }
    }
}

