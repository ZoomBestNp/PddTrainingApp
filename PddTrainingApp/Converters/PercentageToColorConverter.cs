using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PddTrainingApp.Converters
{
    public class PercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int percentage)
            {
                if (percentage >= 80) return Brushes.Green;
                if (percentage >= 60) return Brushes.Orange;
                return Brushes.Red;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}