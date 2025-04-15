using Newtonsoft.Json.Linq;
using QBManager.Utilities;
using QBManager.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace QBManager.ViewModel
{
    class NavigationVM : ViewModelBase
    {
        public Process RunningBot { get; set; }

        private const string DefaultToken = "7968088181:AAGU_X_pe7wVm49h4BhfD6m3U_hUwtbUWB0";

        string BotAppPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QuizBot_3.0.exe");
        string tokenFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "telegram_token.txt");
        
        private string _telegramToken = string.Empty;

        object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        private string _startStopButtonText = "Start Bot";
        public string StartStopButtonText
        {
            get => _startStopButtonText;
            private set
            {
                _startStopButtonText = value;
                OnPropertyChanged();
            }
        }


        bool _isBotRunning = false;
        public bool IsBotRunning
        {
            get => _isBotRunning;
            set
            {
                _isBotRunning = value;
                StartStopButtonText = _isBotRunning ? "Stop Bot" : "Start Bot";
                OnPropertyChanged();
            }
        }


        public ICommand ToggleBotCommand { get; set; }
        public ICommand HomeCommand { get; set; }
        public ICommand QuizzesCommand { get; set; }
        public ICommand AddNewQuizCommand { get; set; }
        public ICommand StudentsCommand { get; set; }
        public ICommand TransactionsCommand { get; set; }
        public ICommand ShipmentsCommand { get; set; }
        public ICommand SettingsCommand { get; set; }

        private void Home(object obj) => CurrentView = new HomeVM();
        private void Quiz(object obj) => CurrentView = new QuizVM();
        private void AddNewQuiz(object obj) => CurrentView = new NewQuizVM();
        private void Student(object obj) => CurrentView = new Students();
        private void Transaction(object obj) => CurrentView = new TransactionVM();
        private void Shipment(object obj) => CurrentView = new ShipmentVM();
        private void Setting(object obj) => CurrentView = new SettingVM();
        private void ToggleBot(object obj)
        {
            CurrentView = new HomeVM();
            if (IsBotRunning)
            {
                StopBot();
            }
            else
            {
                StartBot();
            }
        }

        public NavigationVM()
        {
            ToggleBotCommand = new RelayCommand(ToggleBot);
            HomeCommand = new RelayCommand(Home);
            QuizzesCommand = new RelayCommand(Quiz);
            AddNewQuizCommand = new RelayCommand(AddNewQuiz);
            StudentsCommand = new RelayCommand(Student);
            TransactionsCommand = new RelayCommand(Transaction);
            ShipmentsCommand = new RelayCommand(Shipment);
            SettingsCommand = new RelayCommand(Setting);

            // Startup Page
            CurrentView = new HomeVM();
        }


        public void StartBot()
        {
            try
            {

                // Проверка существования файла
                if (!System.IO.File.Exists(BotAppPath))
                {
                    MessageBox.Show($"Приложение {BotAppPath} не найдено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _telegramToken = ReadTokenFromFile(tokenFilePath);

                if (string.IsNullOrWhiteSpace(_telegramToken))
                {
                    throw new Exception("Токен Telegram-бота не указан!");
                }

                // Запуск консольного приложения
                RunningBot = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = BotAppPath,
                        Arguments = _telegramToken,
                        UseShellExecute = true
                    },
                    EnableRaisingEvents = true
                };


                RunningBot.Exited += OnProcessExited;
                IsBotRunning = true;
                RunningBot.Start();
                



            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            if (RunningBot != null)
            {
                RunningBot.Dispose();
                RunningBot = null;
                IsBotRunning = false;
            }
        }

        // Метод для остановки приложения
        public void StopBot()
        {
            try
            {
                if (RunningBot != null && !RunningBot.HasExited)
                {
                    RunningBot.Kill();
                    RunningBot.Dispose();
                    RunningBot = null;
                    IsBotRunning= false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ReadTokenFromFile(string path)
        {
            string token = string.Empty;
            try
            {

                if (File.Exists(path))
                {
                    token = File.ReadAllText(path); // Загружаем токен из файла
                }
                else
                {
                    File.WriteAllText(path, DefaultToken);
                    token = DefaultToken;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки токена из файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return token;
        }

       
    }
}
