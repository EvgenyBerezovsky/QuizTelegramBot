using QuizBot_1._0.Entities;
using System.Xml.Serialization;

namespace QuizBot_1._0.Infrastructure
{
    public class DataService
    {
        #region Test data
        //Quiz quiz1 = new Quiz()
        //{
        //    Topic = "АРХІТЕКТУРНО-БУДІВЕЛЬНА ВІКТОРИНА",
        //    Questions = new List<QuestionItem>
        //    {
        //        new QuestionItem("Як перекладається з давньогрецької слово «архітектор»?", "Головний будівельник", new string[]{ "Головний будівельник", "Головний художник", "Мудрий геометр", "Старий скульптор"}, 0),
        //        new QuestionItem("Хто побудував найбільшу піраміду?", "Хеопс", new string[] { "Хефрен", "Рамсес", "Мавроді", "Хеопс" }, 3),
        //        new QuestionItem("Хто першим ввів термін ЗОЛОТИЙ ПЕРЕТИН?", "Леонардо да Вінчі", new string[] { "Джотто", "Фідій", "Леонардо да Вінчі", "Калликрат" }, 2),
        //        new QuestionItem("Як називається архітектурно оформлений вхід у будівлю?", "Портал", new string[] { "Портал", "Сайт", "Блог", "Форум" }, 0),
        //        new QuestionItem("Назва якого архітектурного стилю перекладається з французького як «імперія»?", "Ампір", new string[] { "Готика", "Ампір", "Бароко", "Модерн" }, 1),
        //        new QuestionItem("Як називали будівельника в старовину?", "Зодчий", new string[] { "Керманич", "Головний художник", "Зодчий", "Старий скульптор" }, 2),
        //        new QuestionItem("Майстер з кельмою - це...?", "Муляр", new string[] { "Муляр", "Тесляр", "Електрик", "Кошторисник" }, 0),
        //        new QuestionItem("Як в архітектурі називають перший поверх будівлі?", "Цоколь", new string[] { "Цоколь", "Бельетаж", "Пентхаус", "Мансарда" }, 0),
        //        new QuestionItem("Як називається опорна стіна будинку?", "Несуча", new string[] { "Плакуча", "Несуча", "Неминуча", "Страшнюча" }, 1),
        //        new QuestionItem("Як називається дерев'яна споруда, стіни якого зібрані з оброблених колод?", "Зруб", new string[] { "Зруб", "Зуб", "Мазанка", "Сарай" }, 0),
        //        new QuestionItem("Як називається будівельна машина для переміщення вантажів за допомогою рухомого каната (ланцюга)?", "Лебідка", new string[] { "Воронка", "Журавель", "Лебідка", "Сорока" }, 2)
        //    }
        //};
        //Quiz quiz2 = new Quiz()
        //{
        //    Topic = "ЗАЯЧА (КРОЛЯЧА) ВІКТОРИНА",
        //    Questions = new List<QuestionItem>
        //    {
        //        new QuestionItem("Куди заєць біжить швидше: в гору або з гори??", "В гору", new string[] { "В суп", "В нору", "В гору", "С гори" }, 2),
        //        new QuestionItem("Сліпими чи зрячими народжуються зайчата?", "Зрячими", new string[] { "Зрячими", "Сліпими", "Балакучими", "Хитручими" }, 0),
        //        new QuestionItem("Яку назву отримали зайченята, які народилися в червні, коли колоситься жито цвіте гречка?", "Колосовички", new string[] { "Носовички", "Колосовички", "Сніговички", "Чувачки" }, 1),
        //        new QuestionItem("Назва якої європейської країни походить від фінікійського «і-шпанім» - «берег кроликів»?", "Іспанія", new string[] { "Лапландія", "Замбія", "Іспанія", "Зенландія" }, 2),
        //        new QuestionItem("Під яким кущем сидить заєць під час дощу?", "Під мокрим", new string[] { "Під крапівним", "Під капустним", "Під мокрим", "під морковним" }, 2),
        //        new QuestionItem("Живуть зайці в норах?", "Ні", new string[] { "Так", "Якщо кріт дозволив ", "Ні", "Якщо там є світло" }, 2),
        //        new QuestionItem("Чи вірно, що зуби-різці у зайців постійно ростуть і потребують у сточуванні?", "Так", new string[] { "Так", "Ні", "Тільки взимку", "Треба погуглить" }, 0),
        //        new QuestionItem("Яку швидкість може розвивати заєць?", "40", new string[] { "40", "60", "80", "5" }, 0),
        //        new QuestionItem("Чи вірно, що зайці-самці крупніше самок?", "Ні", new string[] { "Так", "Ні", "Якщо гарно поїв", "Запитаю татка" }, 1),
        //        new QuestionItem("Скільки зубів у зайця?", "28", new string[] { "28", "4", "12", "32" }, 0),
        //        new QuestionItem("Як називають зайців, які народилися восени?", "Листопадничками", new string[] { "Подосіновичками", "Жовтопузіками", "Листопадничками", "Хітрожопиками" }, 2),
        //    }
        //};
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
