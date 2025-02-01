using QuizBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QuizBot.Infrastructure
{
    public class DataService
    {
        private const string FileName = "data.xml";
        private const string DirectoryName = "Data";
        private string path;
        private QuizSet quizSet = new QuizSet();
        public List<Quiz>? Quizzes { get { return quizSet.Quizzes; } }
        public DataService()
        {
            path = GetPath(FileName);
            CreateEmptyStoreFile();
            GetData();
        }
        public List<Quiz> GetAllQuizzes()
        {
            return quizSet.Quizzes.ToList();
        }
        public Quiz GetQuizByTopic(string topic)
        {
            return quizSet.Quizzes.Where(q => q.Topic.Equals(topic)).First();
        }
        public void AddQuiz(Quiz quiz)
        {
            quizSet.Quizzes.Add(quiz);
            SaveData();
            GetData(); // ?
        }
        private void SaveData()
        {
            SerializeDataToFile(quizSet);
        }
        private void GetData()
        {
            quizSet = DeserializeDataFromFile();
        }
        private string GetPath(string fn)
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
        private void CreateEmptyStoreFile()
        {
            if (!File.Exists(path))
            {
                SerializeDataToFile(new QuizSet());
            }
        }
        private void SerializeDataToFile(QuizSet dataStore)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(QuizSet));
            using (var stream = new FileStream(this.path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                // Сохраняем объект в XML-файле на диске.
                serializer.Serialize(stream, dataStore);
            }
        }
        private QuizSet? DeserializeDataFromFile()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(QuizSet));

            using (var stream = new FileStream(this.path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Восстанавливаем объект из XML-файла.
                return serializer.Deserialize(stream) as QuizSet;
            }
        }
    }
}
