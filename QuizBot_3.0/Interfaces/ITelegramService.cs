using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace QuizBot_3._0.Interfaces
{
    public interface ITelegramService<T>
    {
        public Task<Update[]> GetUpdatesAsync(long offset);
        public Task SendMessageAsync(long chatId, string text, T? poll = default(T), IReplyMarkup? replyMarkup = null);
        public Task AnswerCallbackQueryAsync(string callbackQueryId);
    }
}
