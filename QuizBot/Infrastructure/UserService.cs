using QuizBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QuizBot.Infrastructure
{
    public class UserService
    {
        private const string FileName = "UserData.xml";
        private const string DirectoryName = "Data";
        private string path;
        private UserSet userSet = new UserSet();
        public List<User>? Users { get { return userSet.Users; } }
        public UserService()
        {
            path = GetPath(FileName);
            CreateEmptyStoreFile();
            GetData();
        }
        public List<User> GetAllUsers()
        {
            SaveData();
            GetData();
            return userSet.Users;
        }

        public void UpdateProgress(User user)
        {
            var users = userSet.Users;
            if(!users.Contains(user))
            {
                userSet.AddNewUser(user);
            }
            else
            {
                int index = users.IndexOf(user);
                userSet.Users[index].Scores = user.Scores;
            }
            SaveData();
            GetData(); // ?
        }
        private void SaveData()
        {
            SerializeDataToFile(userSet);
        }
        private void GetData()
        {
            userSet = DeserializeDataFromFile();
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
                SerializeDataToFile(new UserSet());
            }
        }
        private void SerializeDataToFile(UserSet dataStore)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserSet));
            using (var stream = new FileStream(this.path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                // Сохраняем объект в XML-файле на диске.
                serializer.Serialize(stream, dataStore);
            }
        }
        private UserSet? DeserializeDataFromFile()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserSet));

            using (var stream = new FileStream(this.path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Восстанавливаем объект из XML-файла.
                return serializer.Deserialize(stream) as UserSet;
            }
        }
    }
}
