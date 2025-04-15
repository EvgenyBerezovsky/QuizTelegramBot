using Microsoft.EntityFrameworkCore;
using QuizBot_3._0.Application;
using QuizBot_3._0.BusinessLogic;
using QuizBot_3._0.Infrastructure;
using QuizBot_3._0.Infrastructure.DbDataService.Context;
using QuizBot_3._0.Infrastructure.DbDataService.Models;
using QuizBot_3._0.Infrastructure.TelegramService;
using System;

internal static class Program
{

    #region token
    // Створюємо змінну, що буде зберігати налаштування на наш бот
    
    #endregion
    static async Task Main(string[] args)
    {
        #region EncodingSettings
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        #endregion

        //string telegramToken = "7968088181:AAGU_X_pe7wVm49h4BhfD6m3U_hUwtbUWB0";

        // Получение токена из аргументов
        string telegramToken = args[0];

        var botUpdateHandler = new BotUpdateHandler();
        var telegramService = new TelegramService(telegramToken);
        var cancellationTokenSource = new CancellationTokenSource();

        Console.WriteLine("Bot started...");
        try
        {
            var botRunner = new BotRunner(telegramService, botUpdateHandler, cancellationTokenSource.Token);
            await botRunner.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
        }

        Console.ReadLine();

    }
    //static void CreateDataBaseIfNotExist()
    //{
    //    using (var context = new DbDataServiceContext())
    //    {
    //        Quiz quiz1 = new Quiz()
    //        {
    //            Id = 1,
    //            Topic = "MEDIA MOGULS",
    //            IsActive = true,
    //            IsPublished = true,
    //            Questions = new List<QuestionItem>
    //            {
    //                new QuestionItem()
    //                {
    //                    Question = "Tending to spread aggressively; intrusive",
    //                    Answer = "invasive",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "invasive", Option2 = "insidious", Option3 = "internal", Option4 = "convulsive",},
    //                },
    //                new QuestionItem()
    //                {
    //                    Question = "Someone who owns and controls a large number of newspapers, television companies, magazines, etc. and is able to influence public opinion",
    //                    Answer = "media mogul",
    //                    CorrectOptionIndex = 3,
    //                    Options = new Options(){Option1 = "influential", Option2 = "it's not on", Option3 = "defamation", Option4 = "media mogul",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "Having the power and importance to affect something",
    //                    Answer = "influential",
    //                    CorrectOptionIndex = 2,
    //                    Options = new Options(){Option1 = "philanthropic", Option2 = "fraud", Option3 = "influential", Option4 = "wealthy",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "Dishonest",
    //                    Answer = "corrupt",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "corrupt", Option2 = "media mogul", Option3 = "invasive", Option4 = "it's not on",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "The amount of time or space given to an event by the media",
    //                    Answer = "media coverage",
    //                    CorrectOptionIndex = 1,
    //                    Options = new Options(){Option1 = "display", Option2 = "media coverage", Option3 = "stir somebody up", Option4 = "it's not on",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "Charitable, giving",
    //                    Answer = "philanthropic",
    //                    CorrectOptionIndex = 2,
    //                    Options = new Options(){Option1 = "invasive", Option2 = "corrupt", Option3 = "philanthropic", Option4 = "stir somebody up",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "The action of damaging the good reputation of someone",
    //                    Answer = "defamation",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "defamation", Option2 = "corrupt", Option3 = "wealthy", Option4 = "display",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "Wrongful or criminal deception intended to result in financial or personal gain",
    //                    Answer = "fraud",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "fraud", Option2 = "invasive", Option3 = "display", Option4 = "stir somebody up",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "To show",
    //                    Answer = "display",
    //                    CorrectOptionIndex = 1,
    //                    Options = new Options(){Option1 = "digital", Option2 = "display", Option3 = "exhibit", Option4 = "reverse",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "Far-reaching",
    //                    Answer = "wide-spread",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "wide-spread", Option2 = "influential", Option3 = "confined", Option4 = "wealthy",},
    //                },
    //                new QuestionItem()
    //                {
    //                    Question = "Interesting and exciting character",
    //                    Answer = "colorful personality",
    //                    CorrectOptionIndex = 2,
    //                    Options = new Options(){Option1 = "influential", Option2 = "media mogul", Option3 = "colorful personality", Option4 = "media coverage",},
    //                }
    //            }
    //        };
    //        Quiz quiz2 = new Quiz()
    //        {
    //            Id = 2,
    //            Topic = "IWorld",
    //            IsActive = true,
    //            IsPublished = true,
    //            Questions = new List<QuestionItem>
    //            {
    //                new QuestionItem()
    //                {
    //                    Question = "A wearable device that keeps time and can communicate wirelessly with a smartphone",
    //                    Answer = "smartwatch",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "smartwatch", Option2 = "headphones", Option3 = "accessibility", Option4 = "smartphone",},
    //                },
    //                new QuestionItem()
    //                {
    //                    Question = "A home equipped with technology that promotes safety, telemonitoring, comfort, and other benefits",
    //                    Answer = "smart home",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "smart home", Option2 = "accessibility", Option3 = "eco-friendly home", Option4 = "cofee mashine",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "The fact that something is suitable for your purposes and causes no difficulty for your schedule or plans",
    //                    Answer = "convenience",
    //                    CorrectOptionIndex = 1,
    //                    Options = new Options(){Option1 = "appliance", Option2 = "convenience", Option3 = "accessibility", Option4 = "efficiency",},
    //                },
    //                new QuestionItem()
    //                {
    //                    Question = "The state of experiencing no difficulty, effort, pain, etc.",
    //                    Answer = "ease",
    //                    CorrectOptionIndex = 2,
    //                    Options = new Options(){Option1 = "convenience", Option2 = "awake", Option3 = "ease", Option4 = "alleviate",},
    //                },
    //                new QuestionItem()
    //                {
    //                    Question = "The degree of ease with which it is possible to reach a certain location from other locations.",
    //                    Answer = "accessibility",
    //                    CorrectOptionIndex = 2,
    //                    Options = new Options(){Option1 = "universality", Option2 = "availability", Option3 = "accessibility", Option4 = "affordability",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "Affecting someone in a way that annoys them and makes them feel uncomfortable",
    //                    Answer = "intrusive",
    //                    CorrectOptionIndex = 2,
    //                    Options = new Options(){Option1 = "irksome", Option2 = "insidious", Option3 = "intrusive", Option4 = "accessibility",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "To take control of something",
    //                    Answer = "take over",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "take over", Option2 = "intrusive", Option3 = "remotely", Option4 = "smart home",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "A system that keeps air cool and dry",
    //                    Answer = "air-conditioning",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "air-conditioning", Option2 = "deforestation", Option3 = "smartwatch", Option4 = "fridge-freezer",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "The system that keeps a building warm",
    //                    Answer = "heating",
    //                    CorrectOptionIndex = 1,
    //                    Options = new Options(){Option1 = "burning", Option2 = "heating", Option3 = "boiling", Option4 = "firing",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "A piece of electrical equipment with a particular purpose in the home",
    //                    Answer = "appliance",
    //                    CorrectOptionIndex = 0,
    //                    Options = new Options(){Option1 = "appliance", Option2 = "furniture", Option3 = "utilities", Option4 = "accessibility",},
    //                },

    //                new QuestionItem()
    //                {
    //                    Question = "From a distance",
    //                    Answer = "remotely",
    //                    CorrectOptionIndex = 2,
    //                    Options = new Options(){Option1 = "heating", Option2 = "externally", Option3 = "remotely", Option4 = "appliance",},
    //                },
    //            }
    //        };

    //        context.Add(quiz1);
    //        context.Add(quiz2);
    //        context.SaveChanges();
    //    }
    //}

    //static void CreateNewDataBase()
    //{
    //    using (var context = new DbDataServiceContext())
    //    {
    //        context.Database.EnsureDeleted();
    //        context.Database.EnsureCreated();
    //    }
    //}
}