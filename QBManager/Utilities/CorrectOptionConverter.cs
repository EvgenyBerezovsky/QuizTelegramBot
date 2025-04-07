using QuizBot_3._0.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QBManager.Utilities
{
    public class CorrectOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Преобразование: Извлекаем элемент по индексу
            if (value is QuestionItem question &&
                question.Options != null &&
                question.CorrectOptionIndex >= 0 &&
                question.CorrectOptionIndex < question.Options.Length)
            {
                return question.Options[question.CorrectOptionIndex];
            }

            // Возвращаем сообщение об ошибке, если что-то неверно
            return "Ошибка: неверный индекс";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Обратное преобразование не поддерживается.");
        }
    }
}
