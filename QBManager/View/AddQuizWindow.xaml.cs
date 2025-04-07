using QBManager.Model;
using QBManager.Utilities;
using QBManager.ViewModel;
using QuizBot_3._0.Entities;
using QuizBot_3._0.Infrastructure.DbDataService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QBManager.View
{
    /// <summary>
    /// Interaction logic for AddQuizWindow.xaml
    /// </summary>

    public partial class AddQuizWindow : Window, INotifyPropertyChanged
    {
        private string editedQuestion = string.Empty;
        private List<QuestionItem> questionItems = new List<QuestionItem>();



        QuestionItem _selectedQuestion = new();
        ObservableCollection<QuestionItem> _questions = new();
        public ICommand EditQuestionCommand { get; }
        public ICommand DeleteQuestionCommand { get; }
        public Quiz NewQuiz { get; set; } = new();
        public QuestionItem SelectedQuestion 
        {
            get { return _selectedQuestion; }
            set 
            { 
                _selectedQuestion = value;
                OnPropertyChanged(nameof(SelectedQuestion));
                OnPropertyChanged(nameof(SelectedQuestion.Question));
                OnPropertyChanged(nameof(SelectedQuestion.Options));
                OnPropertyChanged(nameof(SelectedQuestion.Answer));
            }
        }
        public ObservableCollection<QuestionItem> Questions 
        {
            get { return _questions; }
            set 
            { 
                _questions = value;
                OnPropertyChanged(nameof(Questions));
            }
        }

        public AddQuizWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public AddQuizWindow(Quiz quiz)
        {
            InitializeComponent();
            DataContext = this;
            NewQuiz = quiz;
            Questions = new ObservableCollection<QuestionItem>( quiz.Questions);
          
            txtTopic.Text = quiz.Topic;
            chkIsActive.IsChecked = quiz.IsActive;
            EditQuestionCommand = new RelayCommand(EditQuestion);
            DeleteQuestionCommand = new RelayCommand(DeleteQuestion);
            questionItems = Questions.ToList();
        }

        private void DeleteQuestion(object obj)
        {
            string deledetItem = $"Question: -{SelectedQuestion.Question}-";
            var confirmWindow = new ConfirmDeleteWindow(deledetItem);
            confirmWindow.ShowDialog();

            if (confirmWindow.IsConfirmed && SelectedQuestion != null)
            {

                Questions.Remove(SelectedQuestion);

                SelectedQuestion = new();
            }

            OnPropertyChanged(nameof(SelectedQuestion));
            OnPropertyChanged(nameof(Questions));

        }

        private void EditQuestion(object obj)
        {
            editedQuestion = SelectedQuestion.Question;

            txtNewQuestion.Text = SelectedQuestion.Question;
            txtOption1.Text = SelectedQuestion.Options[0];
            txtOption2.Text = SelectedQuestion.Options[1];
            txtOption3.Text = SelectedQuestion.Options[2];
            txtOption4.Text = SelectedQuestion.Options[3];
            txtNewAnswer.Text = SelectedQuestion.Answer;
            

        }

        // Метод для добавления нового вопроса
        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, чтобы все варианты ответа были заполнены
            if (string.IsNullOrWhiteSpace(txtOption1.Text) ||
                string.IsNullOrWhiteSpace(txtOption2.Text) ||
                string.IsNullOrWhiteSpace(txtOption3.Text) ||
                string.IsNullOrWhiteSpace(txtOption4.Text))
            {
                MessageBox.Show("Все варианты ответа должны быть заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, чтобы правильный ответ был заполнен
            if (string.IsNullOrWhiteSpace(txtNewAnswer.Text))
            {
                MessageBox.Show("Введите правильный ответ!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Собираем варианты ответа
            var options = new string[]
            {
                 txtOption1.Text,
                 txtOption2.Text,
                 txtOption3.Text,
                 txtOption4.Text
            };

            // Проверяем, чтобы правильный ответ был в списке вариантов
            int correctOptionIndex = -1;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i].Equals(txtNewAnswer.Text, StringComparison.OrdinalIgnoreCase))
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

            // Создаём новый объект вопроса
            var newQuestion = new QuestionItem
            {
                Question = txtNewQuestion.Text,
                Answer = txtNewAnswer.Text,
                Options = options,
                CorrectOptionIndex = correctOptionIndex
            };

            if (editedQuestion != string.Empty)
            {
                foreach (var q in questionItems)
                {
                    if (q.Question == editedQuestion)
                    {
                        q.Question = newQuestion.Question;
                        q.Options = newQuestion.Options;
                        q.Answer = newQuestion.Answer;
                        q.CorrectOptionIndex = newQuestion.CorrectOptionIndex;
                    }
                }



                txtNewQuestion.Text = string.Empty;
                txtNewAnswer.Text = string.Empty;
                txtOption1.Text = string.Empty;
                txtOption2.Text = string.Empty;
                txtOption3.Text = string.Empty;
                txtOption4.Text = string.Empty;



                Questions.Clear();
                SelectedQuestion = new();

                foreach (var q in questionItems)
                {
                    Questions.Add(q);
                }

                OnPropertyChanged(nameof(SelectedQuestion));
                OnPropertyChanged(nameof(Questions));

                return;
            }
            // Добавляем новый вопрос в список
            Questions.Add(newQuestion);

            // Обновляем отображение DataGrid
            //dgQuestions.Items.Refresh();

            // Очищаем поля после добавления
            txtNewQuestion.Text = string.Empty;
            txtNewAnswer.Text = string.Empty;
            txtOption1.Text = string.Empty;
            txtOption2.Text = string.Empty;
            txtOption3.Text = string.Empty;
            txtOption4.Text = string.Empty;
        }

        // Метод для сохранения викторины
        private void SaveQuiz_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, чтобы название викторины было заполнено
            if (string.IsNullOrWhiteSpace(txtTopic.Text))
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
            if (NewQuiz == null)
            {
                NewQuiz = new Quiz(txtTopic.Text, new List<QuestionItem>(Questions))
                {
                    IsActive = chkIsActive.IsChecked ?? false
                };
            }
            else
            {
                NewQuiz.Questions = Questions.ToList();
                NewQuiz.Topic = txtTopic.Text;
                NewQuiz.IsActive = chkIsActive.IsChecked ?? false;
            }

            DialogResult = true; // Успешное сохранение
            Close();
        }

        private void CloseWin_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

