using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QBManager.Utilities
{
    public class StartStopBtn : Button
    {
        static StartStopBtn()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StartStopBtn), new FrameworkPropertyMetadata(typeof(StartStopBtn)));
        }

        // Свойство для управления состоянием
        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register(
                nameof(IsRunning),
                typeof(bool),
                typeof(StartStopBtn),
                new PropertyMetadata(false, OnIsRunningChanged));

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        // Свойство текста кнопки
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(
                nameof(ButtonText),
                typeof(string),
                typeof(StartStopBtn),
                new PropertyMetadata("Start Bot"));

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            private set => SetValue(ButtonTextProperty, value);
        }

        // Команда для управления состоянием
        public ICommand ToggleCommand { get; }

        public StartStopBtn()
        {
            ToggleCommand = new RelayCommand(_ =>
            {
                IsRunning = !IsRunning;
            });
        }

        private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StartStopBtn button)
            {
                button.UpdateText();

                // Доступ к главному окну через Application.Current.MainWindow
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    if (button.IsRunning)
                    {
                        mainWindow.StartApplication();
                    }
                    else
                    {
                        mainWindow.StopApplication();
                    }
                }
            }
        }

        private void UpdateText()
        {
            ButtonText = IsRunning ? "Stop Bot" : "Start Bot";
        }
    }
} 
