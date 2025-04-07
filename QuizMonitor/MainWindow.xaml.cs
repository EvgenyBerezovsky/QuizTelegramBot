using Microsoft.EntityFrameworkCore;
using QuizBot_3._0.Entities;
using QuizBot_3._0.Infrastructure.DbDataService.Context;
using QuizBot_3._0.Infrastructure.DbDataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace QuizMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process externalProcess;
        public DbDataService dataService;

        public ObservableCollection<Quiz> Quizzes { get; set; } = new();
        public ObservableCollection<User> Users { get; set; } = new();
        public Quiz Quiz { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            dataService = new DbDataService();
            GetQuizzesFromDataService();
            GetUsersFromDataService();

            DataContext = this;

            QuizListView.SelectedItem = Quizzes[0];
            QuestionsListView.ItemsSource = Quizzes[0].Questions;

        }


        private void QuizListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранную викторину
            if (QuizListView.SelectedItem is Quiz selectedQuiz)
            {
                // Обновляем источник данных для списка вопросов
                QuestionsListView.ItemsSource = selectedQuiz.Questions;
            }
        }

        private void QuizListView_TextMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем выбранную викторину
            if (QuizListView.SelectedItem is Quiz selectedQuiz)
            {
                // Обновляем источник данных для списка вопросов
                QuestionsListView.ItemsSource = selectedQuiz.Questions;
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (externalProcess != null && !externalProcess.HasExited)
            {
                externalProcess.Kill(); // Принудительное завершение процесса
                externalProcess = null;
            }
            Application.Current.Shutdown();
        }
        private void GetQuizzesFromDataService()
        {
            foreach (var quiz in dataService.Quizzes)
            {
                Quizzes.Add(quiz);
            }
        }
        private void GetUsersFromDataService()
        {
            foreach (var user in dataService.Users)
            {
                Users.Add(user);
            }
        }
        
    }
}
