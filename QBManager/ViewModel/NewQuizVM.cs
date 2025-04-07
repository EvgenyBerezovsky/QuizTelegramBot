using QBManager.Model;
using QBManager.Utilities;
using QuizBot_3._0.Entities;
using QuizBot_3._0.Infrastructure.DbDataService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QBManager.ViewModel
{
    class NewQuizVM : Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel;
        public NewQuizVM()
        {
            _pageModel = new PageModel();
            AddQuestionCommand = new RelayCommand(AddNewQuestion);
            SaveQuizCommand = new RelayCommand(SaveQuiz);
        }

        private void SaveQuiz(object obj)
        {
            // Проверяем, чтобы название викторины было заполнено
            if (string.IsNullOrWhiteSpace(NewQuiz.Topic))
            {
                MessageBox.Show("Введите название викторины!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, чтобы в викторине был хотя бы один вопрос
            if (Questions.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один вопрос в викторину!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаём новый объект Quiz с введёнными данными
            NewQuiz.Questions = Questions.ToList<QuestionItem>();
            _pageModel.dataService.SaveNewQuiz(NewQuiz);
            Questions.Clear();
            NewQuiz = new();
        }

        private void AddNewQuestion(object obj)
        {
            if (string.IsNullOrWhiteSpace(Question.Question))
            {
                MessageBox.Show("Текст вопроса должен быть заполнен!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Question.Options[0]) ||
                string.IsNullOrWhiteSpace(Question.Options[1]) ||
                string.IsNullOrWhiteSpace(Question.Options[2]) ||
                string.IsNullOrWhiteSpace(Question.Options[3]))
            {
                MessageBox.Show("Все варианты ответа должны быть заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, чтобы правильный ответ был заполнен
            if (string.IsNullOrWhiteSpace(Question.Answer))
            {
                MessageBox.Show("Введите правильный ответ!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, чтобы правильный ответ был в списке вариантов
            int correctOptionIndex = -1;
            for (int i = 0; i < Question.Options.Length; i++)
            {
                if (Question.Options[i].Equals(Question.Answer, StringComparison.OrdinalIgnoreCase))
                {
                    correctOptionIndex = i;
                    break;
                }
            }

            if (correctOptionIndex == -1)
            {
                MessageBox.Show("Правильный ответ должен быть одним из вариантов ответа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Добавляем новый вопрос в список
            Question.CorrectOptionIndex = correctOptionIndex;
            Questions.Add(Question);
            

            // Очищаем поля после добавления
            Question = new();
        }

        public ObservableCollection<QuestionItem> Questions
        {
            get { return _pageModel.Questions; }
            set { _pageModel.Questions = value; OnPropertyChanged(); }
        }

        public Quiz NewQuiz
        {
            get { return _pageModel.Quiz; }
            set
            {
                _pageModel.Quiz = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Questions));
            }
        }

        public QuestionItem Question
        {
            get { return _pageModel.Question; }
            set { _pageModel.Question = value; OnPropertyChanged(); }
        }

        public ICommand AddQuestionCommand { get; }
        public ICommand SaveQuizCommand { get; }



    }
}
