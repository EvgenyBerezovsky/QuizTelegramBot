using QBManager.Model;
using QBManager.Utilities;
using QBManager.View;
using QuizBot_3._0.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QBManager.ViewModel
{
    class StudentVM : Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel;

        private User? _selectedUser;
        private ObservableCollection<User> _users = new();
        private ObservableCollection<Score> _scores = new(); // Постоянная коллекция для привязки
        public ICommand DeleteInfoCommand { get; }
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged(nameof(SelectedUser));

                    // Обновляем Scores при изменении SelectedUser
                    UpdateScores();
                }
            }
        }

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        public ObservableCollection<Score> Scores
        {
            get => _scores;
            private set
            {
                _scores = value;
                OnPropertyChanged(nameof(Scores));
            }
        }

        public StudentVM()
        {
            _pageModel = new PageModel();
            GetUsersFromDataService();
            DeleteInfoCommand = new RelayCommand(DeleteUserInfo);
        }

        void DeleteUserInfo(object obj)
        {
            string deletedItem = $"user data {SelectedUser.UserName}";
            var confirmWindow = new ConfirmDeleteWindow(deletedItem);
            confirmWindow.ShowDialog();

            if (confirmWindow.IsConfirmed && SelectedUser != null)
            {
                _pageModel.dataService.CleanUserData(SelectedUser);
                SelectedUser.Scores = new();
                SelectedUser = null;
            }
        }

        void GetUsersFromDataService()
        {
            foreach (var user in _pageModel.dataService.Users)
            {
                Users.Add(user);
            }
        }

        private void UpdateScores()
        {
            _scores.Clear();

            if (SelectedUser?.Scores != null)
            {
                foreach (var score in SelectedUser.Scores)
                {
                    _scores.Add(score); // Копируем данные в Scores
                }
            }
        }

        

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
