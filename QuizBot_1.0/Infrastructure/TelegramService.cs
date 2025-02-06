using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace QuizBot_1._0.Infrastructure
{
    public class TelegramService 
    {
        private string imagePath = @"D:\ITVDN\SmartTest\QuizBot\QuizBot_1.0\bin\Debug\net7.0\Data\botImage.jpg";

        private readonly TelegramBot _bot;

        public TelegramService(string token)
        {
            _bot = new TelegramBot(token, null);
        }
        public async Task<IEnumerable<Update>> GetUpdatesAsync(long offset)
        {

            return await _bot.MakeRequestAsync(new GetUpdates() { Offset = offset });
        }
        public async Task SendMessageAsync(long chatId, string text, InlineKeyboardMarkup? replyMarkup = null)
        {
            #region If with image
            //using (Stream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    FileToSend image = new FileToSend(stream, imagePath);
            //    await _bot.MakeRequestAsync(new SendPhoto(chatId, image)
            //    {
            //        Caption = text,
            //        ReplyMarkup = replyMarkup
            //    });
            //}
            #endregion

            if (text == null || text == String.Empty) text = "Wrong input.";

            await _bot.MakeRequestAsync(new SendMessage(chatId, text)
            {
                ParseMode = SendMessage.ParseModeEnum.HTML,
                ReplyMarkup = replyMarkup
            }) ;
        }
        public async Task AnswerCallbackQueryAsync(string callbackQueryId)
        {
            await _bot.MakeRequestAsync(new AnswerCallbackQuery(callbackQueryId));
        }
    }
}

