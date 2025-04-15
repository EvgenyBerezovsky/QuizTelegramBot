using QBManager.Model;
using QBManager.Utilities;
using QBManager.View;
using QuizBot_3._0.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QBManager.ViewModel
{
    class QuizVM : Utilities.ViewModelBase, INotifyPropertyChanged
    {
        private readonly PageModel _pageModel;
        private Quiz? _selectedQuiz;
        private bool _isPublished;
        private QuestionItem? _selectedQuestion;
        private ObservableCollection<Quiz> _quizzes = new();

        public ICommand ToggleActiveCommand { get; }
        public ICommand AddQuizCommand { get; }
        public ICommand EditQuizCommand { get; }
        public ICommand ChangeCheckedCommand { get; }
        public ICommand DeleteQuizCommand { get; }



        public ObservableCollection<Quiz> Quizzes
        {
            get => _quizzes;
            set
            {
                _quizzes = value;
                OnPropertyChanged(nameof(Quizzes));
            }
        }  // Список викторин

        // Свойство для текущей выбранной викторины
        public Quiz? SelectedQuiz
        {
            get => _selectedQuiz;
            set
            {
                _selectedQuiz = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Questions));
                OnPropertyChanged(nameof(IsPublished));
            }
        }

        public bool IsPublished
        {
            get => _isPublished;
            set
            {
                if (_isPublished != value)
                {
                    _isPublished = value;
                    OnPropertyChanged(nameof(IsPublished));
                }
            }
        }

        // Свойство для текущего выбранного вопроса
        public QuestionItem SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                _selectedQuestion = value;
                OnPropertyChanged();
            }
        }

        // Список вопросов для выбранной викторины
        public ObservableCollection<QuestionItem> Questions
        {
            get => new ObservableCollection<QuestionItem>(SelectedQuiz?.Questions ?? new List<QuestionItem>());
        }
        public QuizVM()
        {
            _pageModel = new PageModel();
            GetQuizzesFromDataService();
            ToggleActiveCommand = new RelayCommand(ToggleActive, CanToggleActive);

            AddQuizCommand = new RelayCommand(OpenAddQuizWindow);

            EditQuizCommand = new RelayCommand(OpenEditQuizWindow);

            ChangeCheckedCommand = new RelayCommand(parameter =>
        {
            if (parameter is bool isChecked)
            {
                
                IsPublished = isChecked;
                if (SelectedQuiz != null) 
                {
                    SelectedQuiz.IsActive = isChecked;
                    _pageModel.dataService.SaveNewQuiz(SelectedQuiz);
                }
                
                OnPropertyChanged(nameof(SelectedQuiz));
                OnPropertyChanged(nameof(IsPublished));
                OnPropertyChanged(nameof(Quizzes));
            }
        });

            DeleteQuizCommand = new RelayCommand(DeleteQuiz);

        }

        private void DeleteQuiz(object obj)
        {
            string deletedItem = $"Quiz: -{SelectedQuiz.Topic}-";
            var confirmWindow = new ConfirmDeleteWindow(deletedItem);
            confirmWindow.ShowDialog();

            if (confirmWindow.IsConfirmed && SelectedQuiz != null)
            {
                Quiz quizToRemove = new Quiz()
                {
                    Topic = SelectedQuiz.Topic,
                    IsPublished = SelectedQuiz.IsPublished,
                    Questions = SelectedQuiz.Questions,
                };

                // Удаляем викторину
                Quizzes.Remove(SelectedQuiz);

                // Сбрасываем выбранную викторину
                SelectedQuiz = null;

                _pageModel.dataService.RemoveQuiz(quizToRemove);
            }

           

        }

        private void OpenEditQuizWindow(object obj)
        {
            var addQuizWindow = new AddQuizWindow(SelectedQuiz);
            Quiz oldQuiz = new Quiz()
            {
                Topic = SelectedQuiz.Topic,
                IsPublished = SelectedQuiz.IsPublished,
                Questions = SelectedQuiz.Questions,
            };

            if (addQuizWindow.ShowDialog() == true)
            {

                _pageModel.dataService.RemoveQuiz(oldQuiz);

                Quiz newQuiz = addQuizWindow.NewQuiz;

                if (newQuiz != null)
                {
                    SaveQuizInDataBase(addQuizWindow.NewQuiz);
                }

                Quizzes.Clear();
                Questions.Clear();
                SelectedQuestion = new();
                GetQuizzesFromDataService();
                SelectedQuiz = newQuiz;
                OnPropertyChanged(nameof(Quizzes));
                OnPropertyChanged(nameof(Questions));


            }
        }

        private void ToggleActive(object parameter)
        {
            if (SelectedQuiz != null)
            {
                SelectedQuiz.IsPublished = !SelectedQuiz.IsPublished;
                OnPropertyChanged(nameof(Quizzes));
            }
        }

        private bool CanToggleActive(object parameter) => SelectedQuiz != null;

        private void OpenAddQuizWindow(object o)
        {
            var addQuizWindow = new AddQuizWindow();
            if (addQuizWindow.ShowDialog() == true)
            {
                // Добавление новой викторины в список
                Quiz newQuiz = addQuizWindow.NewQuiz;

                if (newQuiz != null)
                {
                    Quizzes.Add(addQuizWindow.NewQuiz);
                    SaveQuizInDataBase(addQuizWindow.NewQuiz);
                }
                SelectedQuiz = newQuiz;
                ReloadQuizzes();
            }
        }

        private void GetQuizzesFromDataService()
        {
            foreach (var quiz in _pageModel.dataService.Quizzes)
            {
                Quizzes.Add(quiz);
            }
        }
        private void SaveQuizInDataBase(Quiz quiz)
        {
            _pageModel.dataService.SaveNewQuiz(quiz);
        }

        public void ReloadQuizzes()
        {
            // Очистить текущие данные
            Quizzes.Clear();

            // Загрузить данные заново из источника
            GetQuizzesFromDataService();

            // Уведомить интерфейс об изменении коллекции
            OnPropertyChanged(nameof(Quizzes));

            // Сбросить выбранный элемент, если необходимо
            SelectedQuiz = null;
            OnPropertyChanged(nameof(SelectedQuiz));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
