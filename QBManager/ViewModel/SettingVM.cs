using Newtonsoft.Json.Linq;
using QBManager.Model;
using QBManager.Utilities;
using QBManager.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QBManager.ViewModel
{
    class SettingVM : Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel;
        private const string TokenFileName = "telegram_token.txt";
        private string _telegramToken = "7968088181:AAGU_X_pe7wVm49h4BhfD6m3U_hUwtbUWB0";
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TokenFileName);

        public string TelegramToken
        {
            get => _telegramToken;
            set
            {
                _telegramToken = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveTokenCommand { get; }
        public SettingVM()
        {
            _pageModel = new PageModel();
            var token = LoadTokenFromFile(filePath);
            if (!string.IsNullOrEmpty(token))
            {
                _telegramToken = token;
            }
            SaveTokenCommand = new RelayCommand(SaveToken);
        }

        private void SaveToken(object obj)
        {
            SaveTokenToFile(TelegramToken);
        }

        private string LoadTokenFromFile(string path)
        {
            string data = string.Empty;
            try
            {

                if (File.Exists(filePath))
                {
                    data = File.ReadAllText(filePath); // Загружаем токен из файла
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки токена из файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return data;
        }
        private void SaveTokenToFile(string value)
        {
            try
            {
                File.WriteAllText(filePath, value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения токена в файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
