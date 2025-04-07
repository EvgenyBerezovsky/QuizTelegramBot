using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QBManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Process RunningProcess { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            StopApplication();
            Close();
        }

        public void StartApplication()
        {
            try
            {
                // Получение текущей директории сборки
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Указание имени вашего консольного приложения
                string consoleAppPath = System.IO.Path.Combine(currentDirectory, "QuizBot_3.0.exe");

                // Проверка существования файла
                if (!System.IO.File.Exists(consoleAppPath))
                {
                    MessageBox.Show($"Приложение {consoleAppPath} не найдено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Запуск консольного приложения
                RunningProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = consoleAppPath,
                        UseShellExecute = true
                    }
                };

                RunningProcess.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для остановки приложения
        public void StopApplication()
        {
            try
            {
                if (RunningProcess != null && !RunningProcess.HasExited)
                {
                    RunningProcess.Kill();
                    RunningProcess.Dispose();
                    RunningProcess = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

