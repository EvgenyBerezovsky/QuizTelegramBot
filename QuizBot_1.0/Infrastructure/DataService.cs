using QuizBot_1._0.Entities;
using System.Xml.Serialization;

namespace QuizBot_1._0.Infrastructure
{
    public class DataService
    {
        #region Test data
        Quiz quiz1 = new Quiz()
        {
            Topic = "MEDIA MOGULS",
            Questions = new List<QuestionItem>
            {
                new QuestionItem("Tending to spread aggressively; intrusive", "invasive", new string[]{ "invasive", "insidious", "internal", "convulsive"}, 0),
                new QuestionItem("Someone who owns and controls a large number of newspapers, television companies, magazines, etc. and is able to influence public opinion", "media mogul", new string[] { "influential", "it's not on", "defamation", "media mogul" }, 3),
                new QuestionItem("Having the power and importance to affect something", "influential", new string[] { "philanthropic", "fraud", "influential", "wealthy" }, 2),
                new QuestionItem("Dishonest", "corrupt", new string[] { "corrupt", "media mogul", "invasive", "it's not on" }, 0),
                new QuestionItem("The amount of time or space given to an event by the media", "media coverage", new string[] { "display", "media coverage", "stir somebody up", "it's not on" }, 1),
                new QuestionItem("Charitable, giving", "invasive", new string[] { "invasive", "corrupt", "philanthropic", "stir somebody up" }, 2),
                new QuestionItem("The action of damaging the good reputation of someone", "defamation", new string[] { "defamation", "corrupt", "wealthy", "display" }, 0),
                new QuestionItem("Wrongful or criminal deception intended to result in financial or personal gain", "fraud", new string[] { "fraud", "invasive", "display", "stir somebody up" }, 0),
                new QuestionItem("To show", "display", new string[] { "digital", "display", "exhibit", "reverse" }, 1),
                new QuestionItem("Far-reaching", "wide-spread", new string[] { "wide-spread", "influential", "confined", "wealthy" }, 0),
                new QuestionItem("Interesting and exciting character", "colorful personality", new string[] { "influential", "media mogul", "colorful personality", "media coverage" }, 2)
            }
        };
        Quiz quiz2 = new Quiz()
        {
            Topic = "IWorld",
            Questions = new List<QuestionItem>
            {
                new QuestionItem("A wearable device that keeps time and can communicate wirelessly with a smartphone", "smartwatch", new string[] { "smartwatch", "headphones", "accessibility", "smartphone" }, 2),
                new QuestionItem("A home equipped with technology that promotes safety, telemonitoring, comfort, and other benefits", "smart home", new string[] { "smart home", "accessibility", "eco-friendly home", "cofee mashine" }, 0),
                new QuestionItem("The fact that something is suitable for your purposes and causes no difficulty for your schedule or plans", "convenience", new string[] { "appliance", "convenience", "accessibility", "efficiency" }, 1),
                new QuestionItem("The state of experiencing no difficulty, effort, pain, etc.", "ease", new string[] { "convenience", "awake", "ease", "alleviate" }, 2),
                new QuestionItem("The degree of ease with which it is possible to reach a certain location from other locations.", "accessibility", new string[] { "universality", "availability", "accessibility", "affordability" }, 2),
                new QuestionItem("Affecting someone in a way that annoys them and makes them feel uncomfortable", "intrusive", new string[] { "irksome", "insidious", "intrusive", "accessibility" }, 2),
                new QuestionItem("To take control of something", "take over", new string[] { "take over", "intrusive", "remotely", "smart home" }, 0),
                new QuestionItem("A system that keeps air cool and dry", "air-conditioning", new string[] { "air-conditioning", "deforestation", "smartwatch", "fridge-freezer" }, 0),
                new QuestionItem("The system that keeps a building warm", "heating", new string[] { "burning", "heating", "boiling", "firing" }, 1),
                new QuestionItem("A piece of electrical equipment with a particular purpose in the home", "appliance", new string[] { "appliance", "furniture", "utilities", "accessibility" }, 0),
                new QuestionItem("From a distance", "remotely", new string[] { "heating", "externally", "remotely", "appliance" }, 2),
            }
        };
        #endregion

        private const string DirectoryName = "Data";
        private const string UsersXmlFileName = "users.xml";
        private const string QuizzesXmlFileName = "quizzes.xml";

        private string usersXmlFilePath;
        private string quizzesXmlFilePath;

        private UserSet userSet = new UserSet();
        private QuizSet quizSet = new QuizSet();

        public List<User>? Users { get { return userSet.Users; } set { userSet.Users = value; } }
        public List<Quiz>? Quizzes { get { return quizSet.Quizzes; } set { quizSet.Quizzes = value; } }
        public DataService()
        {
            usersXmlFilePath = GetFilePath(UsersXmlFileName);
            quizzesXmlFilePath = GetFilePath(QuizzesXmlFileName);

            CreateEmptyUsersXmlFile();
            CreateEmptyQuizzesXmlFile();

            GetUsersData();
            GetQuizzesData();
            //AddTestData();
        }
        public void SaveNewQuiz(Quiz quiz)
        {
            quizSet.Quizzes.Add(quiz);
            SaveQuizzesData();
            GetQuizzesData(); // ?
        }
        public void AddNewUserOrUpdate(User newUser)
        {
            var user = (from u in userSet.Users
                          where u.ChatId == newUser.ChatId
                          select u).FirstOrDefault();
            if (user == default(User))
            {
                userSet.Users.Add(newUser);
            }
            else
            {
                user.UserName = newUser.UserName;
                user.Scores.AddRange(newUser.Scores);
            }

            SaveUsersData();
            GetUsersData(); // ?
        }
        public void CleanUsersData()
        {
            userSet = new UserSet();
            SaveUsersData();
        }
        public void RemoveQuiz(Quiz quize)
        {
            quizSet.Quizzes.Remove(quize);
            SaveQuizzesData();
        }
        public void SaveAllQuizzes()
        {
            SaveQuizzesData();
        }
        private void SaveUsersData()
        {
            SerializeUsersDataToFile(userSet);
        }
        private void SaveQuizzesData()
        {
            SerializeQuizzesDataToFile(quizSet);
        }
        private void GetUsersData()
        {
            userSet = DeserializeUsersDataFromFile();
        }
        private void GetQuizzesData()
        {
            quizSet = DeserializeQuizzesDataFromFile();
        }

        private string GetFilePath(string fn)
        {
            // Отримуємо шлях до поточної директорії, де знаходиться додаток
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Формуємо шлях до папки Data у поточній директорії
            string dataDirectory = Path.Combine(currentDirectory, DirectoryName);

            // Перевіряємо, чи існує папка Data, і створюємо її, якщо не існує
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            // Вказуємо шлях до XML файлу у папці Data
            string xmlFilePath = Path.Combine(dataDirectory, fn);
            return xmlFilePath;
        }

        private void CreateEmptyUsersXmlFile()
        {
            if (!File.Exists(usersXmlFilePath))
            {
                SerializeUsersDataToFile(new UserSet());
            }
        }
        private void CreateEmptyQuizzesXmlFile()
        {
            if (!File.Exists(quizzesXmlFilePath))
            {
                SerializeQuizzesDataToFile(new QuizSet());
            }
        }

        private void SerializeUsersDataToFile(UserSet us)
        {
            if (File.Exists(usersXmlFilePath)) File.Delete(usersXmlFilePath);

            XmlSerializer serializer = new XmlSerializer(typeof(UserSet));
            using (var stream = new FileStream(this.usersXmlFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                // Сохраняем объект в XML-файле на диске.
                serializer.Serialize(stream, us);
            }
        }
        private void SerializeQuizzesDataToFile(QuizSet qs)
        {
            if (File.Exists(quizzesXmlFilePath)) File.Delete(quizzesXmlFilePath);

            XmlSerializer serializer = new XmlSerializer(typeof(QuizSet));
            using (var stream = new FileStream(this.quizzesXmlFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                // Сохраняем объект в XML-файле на диске.
                serializer.Serialize(stream, qs);
            }
        }

        private UserSet? DeserializeUsersDataFromFile()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserSet));

            using (var stream = new FileStream(this.usersXmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Восстанавливаем объект из XML-файла.
                return serializer.Deserialize(stream) as UserSet;
            }
        }
        private QuizSet? DeserializeQuizzesDataFromFile()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(QuizSet));

            using (var stream = new FileStream(this.quizzesXmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Восстанавливаем объект из XML-файла.
                return serializer.Deserialize(stream) as QuizSet;
            }
        }

        //void AddTestData()
        //{
        //    quizSet.Quizzes.Add(quiz1);
        //    quizSet.Quizzes.Add(quiz2);
        //    SaveQuizzesData();
        //    GetQuizzesData(); // ?
        //}
    }
}
