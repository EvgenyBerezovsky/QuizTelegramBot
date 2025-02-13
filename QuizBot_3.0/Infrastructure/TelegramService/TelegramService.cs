using QuizBot_3._0.Entities;
using QuizBot_3._0.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBot_3._0.Infrastructure.TelegramService
{
    public class TelegramService : ITelegramService<QuestionItem>
    {
        private string imagePath = @"D:\ITVDN\SmartTest\QuizBot\QuizBot_1.0\bin\Debug\net7.0\Data\botImage.jpg";

        private readonly TelegramBotClient _bot;

        public TelegramService(string token)
        {
            _bot = new TelegramBotClient(token, null);
        }
        public async Task<Update[]> GetUpdatesAsync(long offset)
        {
            return await _bot.GetUpdatesAsync((int)offset);
        }
        public async Task SendMessageAsync(long chatId, string text, QuestionItem poll = null, IReplyMarkup? replyMarkup = null)
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


            if (text == null || text == string.Empty) text = "Wrong input.";
            await _bot.SendTextMessageAsync(chatId: chatId, text: text, replyMarkup: replyMarkup, parseMode: ParseMode.Html);

            if (poll != null)
            {
                await _bot.SendPollAsync(
            chatId: chatId,
            question: (poll as QuestionItem).Question,
            options: poll.Options,
            isAnonymous: false,                         // Опитування анонімне
            type: PollType.Quiz,                        // Опитування типу "вікторина"
            correctOptionId: poll.CorrectOptionIndex);  // Правильна відповідь

            }

        }
        public async Task AnswerCallbackQueryAsync(string callbackQueryId)
        {
            await _bot.AnswerCallbackQueryAsync(callbackQueryId);
        }
    }
}

