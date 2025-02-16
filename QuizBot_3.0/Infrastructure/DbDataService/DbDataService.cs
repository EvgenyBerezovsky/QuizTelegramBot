using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuizBot_3._0.Entities;
using QuizBot_3._0.Infrastructure.DbDataService.Context;
using QuizBot_3._0.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Infrastructure.DbDataService
{
    public class DbDataService : IDataService<Entities.Quiz, Entities.User>
    {
        public DbDataService()
        {
            InitializeDatabase();
            Users = GetAllUsersFromDb();
            Quizzes = GetAllQuizzesFromDb();
        }
        public List<Entities.Quiz> Quizzes { get; set; }
        public List<Entities.User> Users { get; set; }

        public void AddNewUserOrUpdate(Entities.User newUser)
        {
            var user = Users.Select(u => u).Where(u => u.UserName == newUser.UserName).FirstOrDefault();
            if (user == null)
            {
                Users.Add(newUser);
            }
            else
            {
                user.Scores.AddRange(newUser.Scores);
            }
            using (var context = new DbDataServiceContext())
            {
                Models.User userModelToAdd = CreateUserModel(newUser);
                var userModel = context.Users.Where(u => u.UserName == userModelToAdd.UserName).FirstOrDefault();
                if (userModel == null)
                {
                    context.Users.Add(userModelToAdd);
                }
                else
                {
                    userModel.Scores = userModelToAdd.Scores;
                }
                context.SaveChanges();
            }
        }

        public void CleanUsersData()
        {
            foreach (var user in Users)
            {
                user.Scores = new List<Entities.Score>();
            }
            using (var context = new DbDataServiceContext())
            {
                var users = context.Users
                    .Include(u => u.Scores).ToList();

                foreach (var user in users)
                {
                    user.Scores = new List<Models.Score>();
                }
                context.SaveChanges();
            }
        }

        public void RemoveQuiz(Entities.Quiz quiz)
        {
            Quizzes.Remove(quiz);
            using (var context = new DbDataServiceContext())
            {
                var quizToRemove = context.Quizzes.Select(q => q).Where(q => q.Topic == quiz.Topic).First();
                context.Quizzes.Remove(quizToRemove);
                context.SaveChanges();
            }
        }

        public void SaveAllQuizzes()
        {

            foreach (var quiz in Quizzes)
            {
                AddQuizInToDataBase(quiz);
            }
        }
        public void SaveNewQuiz(Entities.Quiz quiz)
        {
            AddQuizInToDataBase(quiz);
            this.Quizzes = GetAllQuizzesFromDb();
        }
        void AddQuizInToDataBase(Entities.Quiz quiz)
        {
            using (var context = new DbDataServiceContext())
            {
                Models.Quiz quizModel = new Models.Quiz();
                quizModel.Topic = quiz.Topic;

                if (context.Quizzes.Select(q => q).Where(q => q.Topic == quiz.Topic).FirstOrDefault() != null)
                {
                    return;
                }

                quizModel.IsActive = quiz.IsActive;
                quizModel.IsPublished = true;

                List<Models.QuestionItem> questionItems = new List<Models.QuestionItem>();
                List<Models.Options> optionsCollection = new List<Models.Options>();

                foreach (var question in quiz.Questions)
                {
                    Models.QuestionItem questionItem = new Models.QuestionItem();
                    questionItem.Question = question.Question;
                    questionItem.Quiz = quizModel;
                    questionItem.Answer = question.Answer;
                    questionItem.CorrectOptionIndex = question.CorrectOptionIndex;

                    Models.Options options = new Models.Options();
                    options.Option1 = question.Options[0];
                    options.Option2 = question.Options[1];
                    options.Option3 = question.Options[2];
                    options.Option4 = question.Options[3];

                    options.Question = questionItem;
                    questionItem.Options = options;
                    questionItems.Add(questionItem);
                    optionsCollection.Add(options);
                }
                quizModel.Questions = questionItems;

                context.Add(quizModel);
                context.AddRange(questionItems);
                context.AddRange(optionsCollection);

                context.SaveChanges();
            }
        }
        void InitializeDatabase()
        {
            using (var context = new DbDataServiceContext())
            {
                context.Database.Migrate();

                if (!context.Quizzes.Any())
                {
                    Models.Quiz quiz1 = new Models.Quiz()
                    {
                        Id = 1,
                        Topic = "MEDIA MOGULS",
                        IsActive = true,
                        IsPublished = true,
                        Questions = new List<Models.QuestionItem>
                {
                    new Models.QuestionItem()
                    {
                        Question = "Tending to spread aggressively; intrusive",
                        Answer = "invasive",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "invasive", Option2 = "insidious", Option3 = "internal", Option4 = "convulsive",},
                    },
                    new Models.QuestionItem()
                    {
                        Question = "Someone who owns and controls a large number of newspapers, television companies, magazines, etc. and is able to influence public opinion",
                        Answer = "media mogul",
                        CorrectOptionIndex = 3,
                        Options = new Models.Options(){Option1 = "influential", Option2 = "it's not on", Option3 = "defamation", Option4 = "media mogul",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "Having the power and importance to affect something",
                        Answer = "influential",
                        CorrectOptionIndex = 2,
                        Options = new Models.Options(){Option1 = "philanthropic", Option2 = "fraud", Option3 = "influential", Option4 = "wealthy",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "Dishonest",
                        Answer = "corrupt",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "corrupt", Option2 = "media mogul", Option3 = "invasive", Option4 = "it's not on",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "The amount of time or space given to an event by the media",
                        Answer = "media coverage",
                        CorrectOptionIndex = 1,
                        Options = new Models.Options(){Option1 = "display", Option2 = "media coverage", Option3 = "stir somebody up", Option4 = "it's not on",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "Charitable, giving",
                        Answer = "philanthropic",
                        CorrectOptionIndex = 2,
                        Options = new Models.Options(){Option1 = "invasive", Option2 = "corrupt", Option3 = "philanthropic", Option4 = "stir somebody up",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "The action of damaging the good reputation of someone",
                        Answer = "defamation",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "defamation", Option2 = "corrupt", Option3 = "wealthy", Option4 = "display",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "Wrongful or criminal deception intended to result in financial or personal gain",
                        Answer = "fraud",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "fraud", Option2 = "invasive", Option3 = "display", Option4 = "stir somebody up",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "To show",
                        Answer = "display",
                        CorrectOptionIndex = 1,
                        Options = new Models.Options(){Option1 = "digital", Option2 = "display", Option3 = "exhibit", Option4 = "reverse",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "Far-reaching",
                        Answer = "wide-spread",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "wide-spread", Option2 = "influential", Option3 = "confined", Option4 = "wealthy",},
                    },
                    new Models.QuestionItem()
                    {
                        Question = "Interesting and exciting character",
                        Answer = "colorful personality",
                        CorrectOptionIndex = 2,
                        Options = new Models.Options(){Option1 = "influential", Option2 = "media mogul", Option3 = "colorful personality", Option4 = "media coverage",},
                    }
                }
                    };
                    Models.Quiz quiz2 = new Models.Quiz()
                    {
                        Id = 2,
                        Topic = "IWorld",
                        IsActive = true,
                        IsPublished = true,
                        Questions = new List<Models.QuestionItem>
                {
                    new Models.QuestionItem()
                    {
                        Question = "A wearable device that keeps time and can communicate wirelessly with a smartphone",
                        Answer = "smartwatch",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "smartwatch", Option2 = "headphones", Option3 = "accessibility", Option4 = "smartphone",},
                    },
                    new Models.QuestionItem()
                    {
                        Question = "A home equipped with technology that promotes safety, telemonitoring, comfort, and other benefits",
                        Answer = "smart home",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "smart home", Option2 = "accessibility", Option3 = "eco-friendly home", Option4 = "cofee mashine",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "The fact that something is suitable for your purposes and causes no difficulty for your schedule or plans",
                        Answer = "convenience",
                        CorrectOptionIndex = 1,
                        Options = new Models.Options(){Option1 = "appliance", Option2 = "convenience", Option3 = "accessibility", Option4 = "efficiency",},
                    },
                    new Models.QuestionItem()
                    {
                        Question = "The state of experiencing no difficulty, effort, pain, etc.",
                        Answer = "ease",
                        CorrectOptionIndex = 2,
                        Options = new Models.Options(){Option1 = "convenience", Option2 = "awake", Option3 = "ease", Option4 = "alleviate",},
                    },
                    new Models.QuestionItem()
                    {
                        Question = "The degree of ease with which it is possible to reach a certain location from other locations.",
                        Answer = "accessibility",
                        CorrectOptionIndex = 2,
                        Options = new Models.Options(){Option1 = "universality", Option2 = "availability", Option3 = "accessibility", Option4 = "affordability",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "Affecting someone in a way that annoys them and makes them feel uncomfortable",
                        Answer = "intrusive",
                        CorrectOptionIndex = 2,
                        Options = new Models.Options(){Option1 = "irksome", Option2 = "insidious", Option3 = "intrusive", Option4 = "accessibility",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "To take control of something",
                        Answer = "take over",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "take over", Option2 = "intrusive", Option3 = "remotely", Option4 = "smart home",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "A system that keeps air cool and dry",
                        Answer = "air-conditioning",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "air-conditioning", Option2 = "deforestation", Option3 = "smartwatch", Option4 = "fridge-freezer",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "The system that keeps a building warm",
                        Answer = "heating",
                        CorrectOptionIndex = 1,
                        Options = new Models.Options(){Option1 = "burning", Option2 = "heating", Option3 = "boiling", Option4 = "firing",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "A piece of electrical equipment with a particular purpose in the home",
                        Answer = "appliance",
                        CorrectOptionIndex = 0,
                        Options = new Models.Options(){Option1 = "appliance", Option2 = "furniture", Option3 = "utilities", Option4 = "accessibility",},
                    },

                    new Models.QuestionItem()
                    {
                        Question = "From a distance",
                        Answer = "remotely",
                        CorrectOptionIndex = 2,
                        Options = new Models.Options(){Option1 = "heating", Option2 = "externally", Option3 = "remotely", Option4 = "appliance",},
                    },
                }
                    };

                    context.Add(quiz1);
                    context.Add(quiz2);
                    context.SaveChanges();
                }
            }
        }
        List<Entities.User> GetAllUsersFromDb()
        {
            List<Entities.User> users = new List<Entities.User>();

            using (var context = new DbDataServiceContext())
            {
                var userModelsQueryable = context.Users
                    .Include(u => u.Scores);
                var userModels = userModelsQueryable.ToList();

                foreach (var userModel in userModels)
                {
                    Entities.User user = new Entities.User();
                    user.UserName = userModel.UserName;
                    user.ChatId = userModel.ChatID;
                    user.Scores = new List<Entities.Score>();
                    foreach (var scoreModel in userModel.Scores)
                    {
                        Entities.Score score = new Entities.Score();
                        score.Topic = scoreModel.Topic;
                        score.Time = scoreModel.Time;
                        score.Result = scoreModel.Result;
                        user.Scores.Add(score);
                    }
                    users.Add(user);
                }
            }
            return users;
        }
        List<Entities.Quiz> GetAllQuizzesFromDb()
        {
            List<Entities.Quiz> quizzes = new List<Entities.Quiz>();

            using (var context = new DbDataServiceContext())
            {
                var quizeQuestionsQueryable = context
                    .Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(o => o.Options);

                var quizeModels = quizeQuestionsQueryable.ToList();

                foreach (var quizModel in quizeModels)
                {
                    Entities.Quiz quiz = new Entities.Quiz();

                    quiz.IsActive = quizModel.IsActive;
                    quiz.Topic = quizModel.Topic;
                    quiz.Questions = new List<QuestionItem>();
                    foreach (var question in quizModel.Questions)
                    {
                        Entities.QuestionItem questionItem = new Entities.QuestionItem();
                        questionItem.Question = question.Question;
                        questionItem.Answer = question.Answer;
                        questionItem.CorrectOptionIndex = question.CorrectOptionIndex;
                        questionItem.Options = new string[] {
                            question.Options.Option1,
                            question.Options.Option2,
                            question.Options.Option3,
                            question.Options.Option4,
                        };
                        quiz.Questions.Add(questionItem);
                    }
                    quizzes.Add(quiz);
                }
            }
            return quizzes;
        }
        Models.User CreateUserModel(Entities.User user)
        {
            Models.User userModel = new Models.User();

            userModel.UserName = user.UserName;
            userModel.ChatID = user.ChatId;

            List<Models.Score> scoreModels = new List<Models.Score>();
            foreach (var userScore in user.Scores)
            {
                Models.Score scoreModel = new Models.Score();
                scoreModel.Topic = userScore.Topic;
                scoreModel.User = userModel;
                scoreModel.Result = userScore.Result;
                scoreModel.Time = userScore.Time;
                scoreModels.Add(scoreModel);
            }
            userModel.Scores = scoreModels;
            return userModel;
        }
    }
}
